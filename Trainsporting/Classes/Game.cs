using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;
using OpenTK.Graphics;
using System.Drawing.Imaging;
using OpenTK.Input;
using Trainsporting.Classes;

namespace Trainsporting
{
    public class Game : GameWindow
    {
        public static Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        public static string activeShader = "default";

        public static int ibo_elements;

        public static Dictionary<String, Material> materials = new Dictionary<string, Material>();

        public static Dictionary<string, int> textures = new Dictionary<string, int>();

        Vector3[] vertdata;
        Vector2[] texcoorddata;
        Vector3[] normdata;

        public static List<Volume> objects = new List<Volume>();

        public static List<Track> tracks = new List<Track>();


        int[] indicedata;

        public static List<Light> lights = new List<Light>();
        public const int MAX_LIGHTS = 7;


        public static Camera stillCamera = new Camera();
        public static Camera followCamera = new Camera();
        public static Camera onboardCamera = new Camera();

        public static Camera activeCamera;

        float time = 0.0f;
        public static Vector2 lastMousePos = new Vector2();

        Matrix4 view = Matrix4.Identity;
        int lightingMode = 0;

        public static KeyboardState keyboardState, lastKeyboardState;

        public static Train train;

        public static List<Track> branches;

        public const int NUMBER_OF_TRACKS_COLORED = 10;

        public const int TRACK_COLORING_OFFSET = 5;

        public static Light spotLight;

        public Game() : base(1536, 864, new GraphicsMode(32, 24, 0, 4))
        {

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            lastMousePos = new Vector2(Mouse.X, Mouse.Y);
            CursorVisible = false;

            Scene.initProgram();

            Title = "Trainsporting";
            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(500f);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
   
            time += (float)e.Time;

            InputManip.CheckKeyPresses();
            
            AssembleData();

            FillBuffersWithData();

            UpdateObjectPositions();

            UpdateModelViewMatrices();

            GL.UseProgram(shaders[activeShader].ProgramID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);



            ResetMousePosition();

            view = activeCamera.GetViewMatrix();
        }

        private void ResetMousePosition()
        {
            if (Focused)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                lastMousePos += delta;

                activeCamera.AddRotation(delta.X, delta.Y);
                ResetCursor();
            }
        }

        private void UpdateModelViewMatrices()
        {
            foreach (Volume v in objects)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = activeCamera.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 4000.0f);
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }
        }

        private void UpdateObjectPositions()
        {


            train.UpdatePosition();
            followCamera.Target = train.Model.Position;
            onboardCamera.Position = train.Model.Position +
                new Vector3(
                    Train.ONBOARD_TRAIN_OFFSET[0] * (float)Math.Sin(train.Model.Rotation[1]),
                    Train.ONBOARD_TRAIN_OFFSET[1],
                    Train.ONBOARD_TRAIN_OFFSET[2] * (float)Math.Cos(train.Model.Rotation[1]));
            onboardCamera.Target = train.Model.Position;

            spotLight.Position = train.Model.Position + new Vector3(
                -5.0f * (float)Math.Sin(train.Model.Rotation[1]),
                2.0f,
                -5.0f * (float)Math.Cos(train.Model.Rotation[1]));
            spotLight.Direction = (
                new Vector3(
                    30 * (float)Math.Sin(time),
                    -1.0f,
                    30 * (float)Math.Cos(time))).Normalized();
        }

        private void AssembleData()
        {
            int vertsCount = 0, indsCount = 0, texcoordsCount = 0, normalsCount = 0;
            Vector3[][] verts = new Vector3[objects.Count][];
            int[][] inds = new int[objects.Count][];
            Vector2[][] texcoords = new Vector2[objects.Count][];
            Vector3[][] normals = new Vector3[objects.Count][];

            int vertcount = 0;
            int index = 0;
            foreach (Volume v in objects)
            {
                verts[index] = v.GetVerts();
                vertsCount += verts[index].Length;
                inds[index] = v.GetIndices(vertcount);
                indsCount += inds[index].Length;

                texcoords[index] = v.GetTextureCoords();
                texcoordsCount += texcoords[index].Length;

                normals[index] = v.GetNormals();
                normalsCount += normals[index].Length;

                index++;
                vertcount += v.VertCount;
            }

            vertdata = new Vector3[vertsCount];
            indicedata = new int[indsCount];
            texcoorddata = new Vector2[texcoordsCount];
            normdata = new Vector3[normalsCount];

            vertsCount = 0; indsCount = 0; texcoordsCount = 0; normalsCount = 0;

            for (int i = 0; i < objects.Count; i++)
            {
                Array.Copy(verts[i], 0, vertdata, vertsCount, verts[i].Length);
                vertsCount += verts[i].Length;

                Array.Copy(inds[i], 0, indicedata, indsCount, inds[i].Length);
                indsCount += inds[i].Length;

                Array.Copy(texcoords[i], 0, texcoorddata, texcoordsCount, texcoords[i].Length);
                texcoordsCount += texcoords[i].Length;

                Array.Copy(normals[i], 0, normdata, normalsCount, normals[i].Length);
                normalsCount += normals[i].Length;
            }
        }

        private void FillBuffersWithData()
        {
            FillBuffer("vPosition", vertdata, Vector3.SizeInBytes);
            
            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                FillBuffer("texcoord", texcoorddata, Vector2.SizeInBytes);
            }

            if (shaders[activeShader].GetAttribute("vNormal") != -1)
            {
                FillBuffer("vNormal", normdata, Vector3.SizeInBytes);
            }

            // Buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);
        }

        private void FillBuffer<T>(string bufferName, T[] fillerData,int sizeInBytes) where T : struct
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer(bufferName));
            GL.BufferData<T>(BufferTarget.ArrayBuffer, (IntPtr)(fillerData.Length * sizeInBytes), fillerData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute(bufferName), sizeInBytes/sizeof(float), VertexAttribPointerType.Float, false, 0, 0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            GL.UseProgram(shaders[activeShader].ProgramID);

            shaders[activeShader].EnableVertexAttribArrays();

            DrawAllObjects();

            shaders[activeShader].DisableVertexAttribArrays();

            GL.Flush();
            SwapBuffers();
        }

        private void DrawAllObjects()
        {
            int indiceat = 0;

            GL.Uniform1(shaders[activeShader].GetUniform("mode"), lightingMode);

            // Draw all objects
            foreach (Volume v in objects)
            {
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);

                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                if (shaders[activeShader].GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetAttribute("maintexture"), v.TextureID);
                }

                if (shaders[activeShader].GetUniform("view") != -1)
                {
                    GL.UniformMatrix4(shaders[activeShader].GetUniform("view"), false, ref view);
                }

                if (shaders[activeShader].GetUniform("model") != -1)
                {
                    GL.UniformMatrix4(shaders[activeShader].GetUniform("model"), false, ref v.ModelMatrix);
                }

                if (shaders[activeShader].GetUniform("material_ambient") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_ambient"), ref v.Material.AmbientColor);
                }

                if (shaders[activeShader].GetUniform("material_diffuse") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_diffuse"), ref v.Material.DiffuseColor);
                }

                if (shaders[activeShader].GetUniform("material_specular") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_specular"), ref v.Material.SpecularColor);
                }

                if (shaders[activeShader].GetUniform("material_specExponent") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("material_specExponent"), v.Material.SpecularExponent);
                }

                if (shaders[activeShader].GetUniform("map_specular") != -1)
                {
                    // Object has a specular map
                    if (v.Material.SpecularMap != "")
                    {
                        GL.ActiveTexture(TextureUnit.Texture1);
                        GL.BindTexture(TextureTarget.Texture2D, textures[v.Material.SpecularMap]);
                        GL.Uniform1(shaders[activeShader].GetUniform("map_specular"), 1);
                        GL.Uniform1(shaders[activeShader].GetUniform("hasSpecularMap"), 1);
                        GL.ActiveTexture(TextureUnit.Texture0);
                    }
                    else // Object has no specular map
                    {
                        GL.Uniform1(shaders[activeShader].GetUniform("hasSpecularMap"), 0);
                    }
                }

                for (int i = 0; i < Math.Min(lights.Count, MAX_LIGHTS); i++)
                {
                    if (shaders[activeShader].GetUniform("lights[" + i + "].position") != -1)
                    {
                        GL.Uniform3(shaders[activeShader].GetUniform("lights[" + i + "].position"), ref lights[i].Position);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].color") != -1)
                    {
                        GL.Uniform3(shaders[activeShader].GetUniform("lights[" + i + "].color"), ref lights[i].Color);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].diffuseIntensity") != -1)
                    {
                        GL.Uniform1(shaders[activeShader].GetUniform("lights[" + i + "].diffuseIntensity"), lights[i].DiffuseIntensity);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].ambientIntensity") != -1)
                    {
                        GL.Uniform1(shaders[activeShader].GetUniform("lights[" + i + "].ambientIntensity"), lights[i].AmbientIntensity);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].specularIntensity") != -1)
                    {
                        GL.Uniform1(shaders[activeShader].GetUniform("lights[" + i + "].specularIntensity"), lights[i].SpecularIntensity);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].direction") != -1)
                    {
                        GL.Uniform3(shaders[activeShader].GetUniform("lights[" + i + "].direction"), ref lights[i].Direction);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].type") != -1)
                    {
                        GL.Uniform1(shaders[activeShader].GetUniform("lights[" + i + "].type"), (int)lights[i].Type);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].coneAngle") != -1)
                    {
                        GL.Uniform1(shaders[activeShader].GetUniform("lights[" + i + "].coneAngle"), lights[i].ConeAngle);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].linearAttenuation") != -1)
                    {
                        GL.Uniform1(shaders[activeShader].GetUniform("lights[" + i + "].linearAttenuation"), lights[i].LinearAttenuation);
                    }

                    if (shaders[activeShader].GetUniform("lights[" + i + "].quadraticAttenuation") != -1)
                    {
                        GL.Uniform1(shaders[activeShader].GetUniform("lights[" + i + "].quadraticAttenuation"), lights[i].QuadraticAttenuation);
                    }
                }

                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }
        }

        protected override void OnResize(EventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);

            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadMatrix(ref projection);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == 27)
            {
                Exit();
            }

            switch (e.KeyChar)
            {
                case '1':
                    activeCamera = stillCamera;
                    break;
                case '2':
                    activeCamera = followCamera;
                    break;
                case '3':
                    activeCamera = onboardCamera;
                    break;
                case 'o':
                    lightingMode = 0;
                    break;
                case 'p':
                    lightingMode = 1;
                    break;
                case 'k':
                    activeShader = "gourard";
                    break;
                case 'l':
                    activeShader = "phong";
                    break;
                case 'w':
                    train.Accelerate();
                    break;
                case 's':
                    train.Decelerate();
                    break;
            }
        }

        void ResetCursor()
        {
            OpenTK.Input.Mouse.SetPosition(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);

            if (Focused)
            {
                ResetCursor();
            }
        }

    }
}
