using OpenTK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trainsporting.Classes;

using static Trainsporting.Classes.Global;

namespace Trainsporting
{
    public class Volume
    {

        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ModelViewProjectionMatrix = Matrix4.Identity;

        public Material Material = new Material();

        public int TextureID;

        private List<Tuple<FaceVertex, FaceVertex, FaceVertex>> faces = new List<Tuple<FaceVertex, FaceVertex, FaceVertex>>();

        public int VertCount { get { return GetVerts().Length; } }
        public int IndiceCount { get { return faces.Count * 3; } }
        public int ColorDataCount { get { return GetColorData().Length; } }

        Vector3[] normals;
        Vector3[] verts;
        Vector2[] coords;
        int[] indices;

        public static Volume VolumeFactory(string objectFilename, string textureFilename, string materialName, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Volume obj = Volume.LoadFromFile(objectFilename);
            obj.TextureID = Game.textures[Global.TEXTURES_RELATIVE_PATH + textureFilename];
            obj.Position = position;
            obj.Rotation = rotation;
            obj.Scale = scale;
            obj.Material = Game.materials[materialName];
            return obj;
        }
        /// <summary>
        /// Get vertice data for this object
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetVerts()
        {
            return verts;
        }

        /// <summary>
        /// Get indices
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int[] GetIndices(int offset = 0)
        {
            if (offset == 0) return indices;
            int[] offsetIndices = new int[IndiceCount];
            for (int i = 0; i < IndiceCount; i++)
                offsetIndices[i] = indices[i] + offset;
            return offsetIndices;
        }

        /// <summary>
        /// Get color data.
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetColorData()
        {
            return new Vector3[ColorDataCount];
        }

        /// <summary>
        /// Get texture coordinates.
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetTextureCoords()
        {
            return coords;
        }


        /// <summary>
        /// Calculates the model matrix from transforms
        /// </summary>
        public void CalculateModelMatrix()
        {
            ModelMatrix =
                Matrix4.CreateScale(Scale) *
                Matrix4.CreateRotationX(Rotation.X) *
                Matrix4.CreateRotationY(Rotation.Y) *
                Matrix4.CreateRotationZ(Rotation.Z) *
                Matrix4.CreateTranslation(Position);
        }
        public static Volume LoadFromFile(string filename)
        {
            Volume obj = new Volume();
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(Global.OBJECTS_RELATIVE_PATH + filename, FileMode.Open, FileAccess.Read)))
                {
                    obj = LoadFromString(reader.ReadToEnd());
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found: {0}", filename);
            }
            catch (Exception)
            {
                Console.WriteLine("Error loading file: {0}", filename);
            }

            return obj;
        }
        public static Volume LoadFromString(string obj)
        {
            // Seperate lines from the file
            List<String> lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texs = new List<Vector2>();
            List<Tuple<TempVertex, TempVertex, TempVertex>> faces = new List<Tuple<TempVertex, TempVertex, TempVertex>>();

            // Base values
            verts.Add(new Vector3());
            texs.Add(new Vector2());
            normals.Add(new Vector3());

            // Read file line by line
            foreach (String line in lines)
            {
                if (line.StartsWith("v ")) // Vertex definition
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Vector3 vec = new Vector3();

                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a vertex
                    {
                        String[] vertparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], style, culture, out vec.X);
                        success |= float.TryParse(vertparts[1], style, culture, out vec.Y);
                        success |= float.TryParse(vertparts[2], style, culture, out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing vertex: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing vertex: {0}", line);
                    }

                    verts.Add(vec);
                }
                else if (line.StartsWith("vt ")) // Texture coordinate
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Vector2 vec = new Vector2();

                    if (temp.Trim().Count((char c) => c == ' ') > 0) // Check if there's enough elements for a vertex
                    {
                        String[] texcoordparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(texcoordparts[0], style, culture, out vec.X);
                        success |= float.TryParse(texcoordparts[1], style, culture, out vec.Y);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing texture coordinate: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing texture coordinate: {0}", line);
                    }

                    texs.Add(vec);
                }
                else if (line.StartsWith("vn ")) // Normal vector
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Vector3 vec = new Vector3();

                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a normal
                    {
                        String[] vertparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], style, culture, out vec.X);
                        success |= float.TryParse(vertparts[1], style, culture, out vec.Y);
                        success |= float.TryParse(vertparts[2], style, culture, out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing normal: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing normal: {0}", line);
                    }

                    normals.Add(vec);
                }
                else if (line.StartsWith("f ")) // Face definition
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Tuple<TempVertex, TempVertex, TempVertex> face = new Tuple<TempVertex, TempVertex, TempVertex>(new TempVertex(), new TempVertex(), new TempVertex());

                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a face
                    {
                        String[] faceparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int v1, v2, v3;
                        int t1, t2, t3;
                        int n1, n2, n3;

                        // Attempt to parse each part of the face
                        bool success = int.TryParse(faceparts[0].Split('/')[0], out v1);
                        success |= int.TryParse(faceparts[1].Split('/')[0], out v2);
                        success |= int.TryParse(faceparts[2].Split('/')[0], out v3);

                        if (faceparts[0].Count((char c) => c == '/') >= 2)
                        {
                            success |= int.TryParse(faceparts[0].Split('/')[1], out t1);
                            success |= int.TryParse(faceparts[1].Split('/')[1], out t2);
                            success |= int.TryParse(faceparts[2].Split('/')[1], out t3);
                            success |= int.TryParse(faceparts[0].Split('/')[2], out n1);
                            success |= int.TryParse(faceparts[1].Split('/')[2], out n2);
                            success |= int.TryParse(faceparts[2].Split('/')[2], out n3);
                        }
                        else
                        {
                            if (texs.Count > v1 && texs.Count > v2 && texs.Count > v3)
                            {
                                t1 = v1;
                                t2 = v2;
                                t3 = v3;
                            }
                            else
                            {
                                t1 = 0;
                                t2 = 0;
                                t3 = 0;
                            }


                            if (normals.Count > v1 && normals.Count > v2 && normals.Count > v3)
                            {
                                n1 = v1;
                                n2 = v2;
                                n3 = v3;
                            }
                            else
                            {
                                n1 = 0;
                                n2 = 0;
                                n3 = 0;
                            }
                        }


                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing face: {0}", line);
                        }
                        else
                        {
                            TempVertex tv1 = new TempVertex(v1, n1, t1);
                            TempVertex tv2 = new TempVertex(v2, n2, t2);
                            TempVertex tv3 = new TempVertex(v3, n3, t3);
                            face = new Tuple<TempVertex, TempVertex, TempVertex>(tv1, tv2, tv3);
                            faces.Add(face);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing face: {0}", line);
                    }
                }
            }

            // Create the Volume
            Volume vol = new Volume();

            foreach (var face in faces)
            {
                FaceVertex v1 = new FaceVertex(verts[face.Item1.Vertex], normals[face.Item1.Normal], texs[face.Item1.Texcoord]);
                FaceVertex v2 = new FaceVertex(verts[face.Item2.Vertex], normals[face.Item2.Normal], texs[face.Item2.Texcoord]);
                FaceVertex v3 = new FaceVertex(verts[face.Item3.Vertex], normals[face.Item3.Normal], texs[face.Item3.Texcoord]);

                vol.faces.Add(new Tuple<FaceVertex, FaceVertex, FaceVertex>(v1, v2, v3));
            }

            vol.SetVerts();
            vol.SetNormals();
            vol.SetTextureCoords();
            vol.SetIndices();


            return vol;
        }

        private void SetIndices()
        {
            indices = Enumerable.Range(0, IndiceCount).ToArray();
        }

        private void SetTextureCoords()
        {
            List<Vector2> _coords = new List<Vector2>();

            foreach (var face in faces)
            {
                _coords.Add(face.Item1.TextureCoord);
                _coords.Add(face.Item2.TextureCoord);
                _coords.Add(face.Item3.TextureCoord);
            }

            coords = _coords.ToArray();
        }

        private void SetNormals()
        {

            List<Vector3> _normals = new List<Vector3>();

            foreach (var face in faces)
            {
                _normals.Add(face.Item1.Normal);
                _normals.Add(face.Item2.Normal);
                _normals.Add(face.Item3.Normal);
            }

            normals = _normals.ToArray();
        }

        private void SetVerts()
        {
            List<Vector3> _verts = new List<Vector3>();

            foreach (var face in faces)
            {
                _verts.Add(face.Item1.Position);
                _verts.Add(face.Item2.Position);
                _verts.Add(face.Item3.Position);
            }

            verts = _verts.ToArray();
        }

        private class TempVertex
        {
            public int Vertex;
            public int Normal;
            public int Texcoord;

            public TempVertex(int vert = 0, int norm = 0, int tex = 0)
            {
                Vertex = vert;
                Normal = norm;
                Texcoord = tex;
            }
        }

        public Vector3[] GetNormals()
        {
            return normals;
        }

        public int NormalCount
        {
            get
            {
                return faces.Count * 3;
            }
        }

    }
    class FaceVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoord;

        public FaceVertex(Vector3 pos, Vector3 norm, Vector2 texcoord)
        {
            Position = pos;
            Normal = norm;
            TextureCoord = texcoord;
        }
    }
}
