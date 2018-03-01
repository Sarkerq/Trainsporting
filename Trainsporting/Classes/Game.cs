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
    class Game : GameWindow
    {
        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        string activeShader = "default";

        int ibo_elements;

        public static Dictionary<String, Material> materials = new Dictionary<string, Material>();

        public static Dictionary<string, int> textures = new Dictionary<string, int>();

        Vector3[] vertdata;
        Vector3[] coldata;
        Vector2[] texcoorddata;
        Vector3[] normdata;

        List<Volume> objects = new List<Volume>();

        List<Track> tracks = new List<Track>();


        int[] indicedata;

        List<Light> lights = new List<Light>();
        const int MAX_LIGHTS = 7;


        Camera stillCamera = new Camera();
        Camera followCamera = new Camera();
        Camera onboardCamera = new Camera();

        Camera activeCamera;

        float time = 0.0f;
        Vector2 lastMousePos = new Vector2();

        Matrix4 view = Matrix4.Identity;
        int lightingMode = 0;

        KeyboardState keyboardState, lastKeyboardState;

        Train train;

        List<Track> branches;

        public int NUMBER_OF_TRACKS_COLORED = 10;

        public int TRACK_COLORING_OFFSET = 5;

        Light spotLight;

        public Game() : base(800, 600, new GraphicsMode(32, 24, 0, 4))
        {

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            Title = "Trainsporting";
            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(500f);
        }
        void initProgram()
        {
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);
            CursorVisible = false;

            GL.GenBuffers(1, out ibo_elements);

            loadResources();

            activeShader = "phong";

            setupScene();
        }
        private void loadResources()
        {
            // Load shaders from file
            shaders.Add("phong", new ShaderProgram("vs_phong.glsl", "fs_phong.glsl", true));
            shaders.Add("gourard", new ShaderProgram("vs_gourard.glsl", "fs_gourard.glsl", true));

            // Load materials and textures
            loadMaterials("opentk.mtl");
            loadMaterials("train.mtl");
            loadMaterials("track.mtl");
            loadMaterials("tree.mtl");
            loadMaterials("rock.mtl");
        }

        private void setupScene()
        {

            ObjVolume trainModel = ObjVolume.ObjVolumeFactory("train.obj", "basic1.png", "AVE-BLANCO",
                new Vector3(0, 0.7f, -2f),
                new Vector3(0, (float)Math.PI / 80, 0),
                new Vector3(10.0f, 10.0f, 10.0f));
            objects.Add(trainModel);

            ObjVolume trackModel = ObjVolume.ObjVolumeFactory("track.obj", "basic2.png", "AVE-BLANCO",
                trainModel.Position - Track.TRAIN_TRACK_OFFSET,
                new Vector3(0, 0, 0),
                new Vector3(0.75f, 0.75f, 2.4f));
            objects.Add(trackModel);
            tracks.Add(new Track(trackModel, 0));

            Track lastTrack = tracks[0];
            for (int i = 0; i < 79; i++)
            {
                lastTrack = new Track(lastTrack, (float)Math.PI / 80);
                objects.Add(lastTrack.Model);
                tracks.Add(lastTrack);

            }

            branches = lastTrack.MakeBranchingPoint();

            lastTrack = branches[0];
            objects.Add(lastTrack.Model);
            tracks.Add(lastTrack);
            for (int i = 0; i < 79; i++)
            {
                lastTrack = new Track(lastTrack, (float)Math.PI / 80);
                objects.Add(lastTrack.Model);
                tracks.Add(lastTrack);

            }
            lastTrack = branches[1];
            objects.Add(lastTrack.Model);
            tracks.Add(lastTrack);
            for (int i = 0; i < 49; i++)
            {
                lastTrack = new Track(lastTrack, 0);
                objects.Add(lastTrack.Model);
                tracks.Add(lastTrack);

            }
            for (int i = 0; i < 80; i++)
            {
                lastTrack = new Track(lastTrack, (float)Math.PI / 80);
                objects.Add(lastTrack.Model);
                tracks.Add(lastTrack);

            }
            for (int i = 0; i < 50; i++)
            {
                lastTrack = new Track(lastTrack, 0);
                objects.Add(lastTrack.Model);
                tracks.Add(lastTrack);

            }
            int branch1Index = tracks.IndexOf(branches[1]);
            train = new Train(trainModel, tracks);
            for (int i = TRACK_COLORING_OFFSET; i < TRACK_COLORING_OFFSET + NUMBER_OF_TRACKS_COLORED; i++)
            {
                tracks[branch1Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic3.png"];
            }
            for (int i = 0; i < 100; i++)
            {
                ObjVolume treeModel = ObjVolume.ObjVolumeFactory("tree.obj", "tree.png", "AVE-BLANCO",
                    new Vector3(180.0f + (float)(160 - i * 1.5) * (float)Math.Sin(i), 1.4f, -2f + (float)(160 - i * 1.5) * (float)Math.Cos(i)),
                    new Vector3(0, (float)Math.PI / 80, 0),
                    new Vector3(2.0f, 2.5f, 2.0f));
                objects.Add(treeModel);
            }
            for (int i = 0; i < 80; i++)
            {
                ObjVolume treeModel = ObjVolume.ObjVolumeFactory("tree.obj", "tree.png", "AVE-BLANCO",
                    new Vector3(210.0f + (float)(140 - i * 1.5) * (float)Math.Sin(i), 1.4f, -370f + (float)(140 - i * 1.5) * (float)Math.Cos(i)),
                    new Vector3(0, (float)Math.PI / 80, 0),
                    new Vector3(3.0f, 3.5f, 3.0f));
                objects.Add(treeModel);
            }

            for (int i = 0; i < 70; i++)
            {
                ObjVolume treeModel = ObjVolume.ObjVolumeFactory("tree.obj", "tree.png", "AVE-BLANCO",
                    tracks[i * 3].Model.Position +
                    new Vector3(
                        (float)(5) * (float)Math.Sqrt(Math.Abs(Math.Sin(tracks[i * 3].Model.Rotation[1]))) +
                        (float)(35) * (float)Math.Abs(Math.Cos(i) + 0.45f) + 1.3f,
                    1.4f,
                    (float)(5) * (float)Math.Sqrt(Math.Abs(Math.Cos(tracks[i * 3].Model.Rotation[1]))) +
                    (float)(25) * (float)Math.Abs(Math.Cos(i) + 1.5f) + 6.9f),
                    new Vector3(0, (float)Math.PI / 80, 0),
                    new Vector3(4.0f, 5.0f, 4.0f));
                objects.Add(treeModel);
            }


            ObjVolume rockModel = ObjVolume.ObjVolumeFactory("rock.obj", "rock.png", "AVE-BLANCO",
                    new Vector3(-0.0f, 40.7f, -300f),
                    new Vector3(0, (float)Math.PI / 2, 0),
                    new Vector3(5.0f, 5.0f, 5.0f));
            objects.Add(rockModel);

            TexturedCube floor = new TexturedCube();
            floor.TextureID = textures[materials["opentk1"].DiffuseMap];
            floor.Scale = new Vector3(2000, 0.1f, 2000);
            floor.Position += new Vector3(0, -2, 0);
            floor.CalculateNormals();
            floor.Material = materials["opentk1"];
            objects.Add(floor);


            // Create lights

            spotLight = new Light(train.Model.Position + new Vector3(0, 1.0f, -3.0f), new Vector3(1.0f, 0.0f, 0.0f));
            spotLight.Type = LightType.Spot;
            spotLight.ConeAngle = 30.0f;
            spotLight.Direction = (spotLight.Position - new Vector3(100, 1.0f, -3.0f)).Normalized();
            spotLight.QuadraticAttenuation = 0.00001f;
            lights.Add(spotLight);

            for (int i = 0; i < tracks.Count; i += tracks.Count / (MAX_LIGHTS - 1))
            {
                Light pointLight = new Light(new Vector3(2, 20, 2) + tracks[i].Model.Position, new Vector3(1.0f, 1.0f, 1.0f));
                pointLight.Type = LightType.Point;
                pointLight.QuadraticAttenuation = 0.0008f;
                lights.Add(pointLight);
            }



            // Setup cameras
            activeCamera = stillCamera;


            stillCamera.Position = new Vector3(300f, 120f, 300f);
            stillCamera.AddRotation(0.5f, -30.5f);

            followCamera.Position = new Vector3(170f, 120f, -20f);
            followCamera.Target = train.Model.Position;

            onboardCamera.Position = train.Model.Position +
                new Vector3(
                    Train.ONBOARD_TRAIN_OFFSET[0] * (float)Math.Sin(train.Model.Rotation[1]),
                    Train.ONBOARD_TRAIN_OFFSET[1],
                    Train.ONBOARD_TRAIN_OFFSET[2] * (float)Math.Cos(train.Model.Rotation[1]));
            onboardCamera.Target = train.Model.Position;

        }
        private void loadMaterials(String filename)
        {
            foreach (var mat in Material.LoadFromFile(Global.MATERIALS_RELATIVE_PATH + filename))
            {
                if (!materials.ContainsKey(mat.Key))
                {
                    materials.Add(mat.Key, mat.Value);
                }
            }

            // Load textures
            foreach (Material mat in materials.Values)
            {
                if (File.Exists(mat.AmbientMap) && !textures.ContainsKey(mat.AmbientMap))
                {
                    textures.Add(mat.AmbientMap, loadImage(mat.AmbientMap));
                }

                if (File.Exists(mat.DiffuseMap) && !textures.ContainsKey(mat.DiffuseMap))
                {
                    textures.Add(mat.DiffuseMap, loadImage(mat.DiffuseMap));
                }

                if (File.Exists(mat.SpecularMap) && !textures.ContainsKey(mat.SpecularMap))
                {
                    textures.Add(mat.SpecularMap, loadImage(mat.SpecularMap));
                }

                if (File.Exists(mat.NormalMap) && !textures.ContainsKey(mat.NormalMap))
                {
                    textures.Add(mat.NormalMap, loadImage(mat.NormalMap));
                }

                if (File.Exists(mat.OpacityMap) && !textures.ContainsKey(mat.OpacityMap))
                {
                    textures.Add(mat.OpacityMap, loadImage(mat.OpacityMap));
                }
            }
        }
        public bool KeyPress(Key key)
        {
            return (keyboardState[key] && (keyboardState[key] != lastKeyboardState[key]));
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Get current state
            keyboardState = OpenTK.Input.Keyboard.GetState();

            // Check Key Presses
            if (KeyPress(Key.LShift))
            {
                int branch0Index = tracks.IndexOf(branches[0]);
                int branch1Index = tracks.IndexOf(branches[1]);
                if (train.branchSetting == 1)
                {
                    train.branchSetting = 0;
                    for (int i = TRACK_COLORING_OFFSET; i < TRACK_COLORING_OFFSET + NUMBER_OF_TRACKS_COLORED; i++)
                    {
                        tracks[branch0Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic3.png"];
                        tracks[branch1Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic2.png"];
                    }
                }
                else
                {
                    train.branchSetting = 1;
                    for (int i = TRACK_COLORING_OFFSET; i < TRACK_COLORING_OFFSET + NUMBER_OF_TRACKS_COLORED; i++)
                    {
                        tracks[branch0Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic2.png"];
                        tracks[branch1Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic3.png"];
                    }
                }
            }
            // Store current state for next comparison;
            lastKeyboardState = keyboardState;



            int vertsCount = 0, indsCount = 0, texcoordsCount = 0, normalsCount = 0;
            Vector3[][] verts = new Vector3[objects.Count][];
            int[][] inds = new int[objects.Count][];
            Vector3[][] colors = new Vector3[objects.Count][];
            Vector2[][] texcoords = new Vector2[objects.Count][];
            Vector3[][] normals = new Vector3[objects.Count][];

            // Assemble vertex and indice data for all volumes
            int vertcount = 0;
            int index = 0;
            foreach (Volume v in objects)
            {
                verts[index] = v.GetVerts();
                vertsCount += verts[index].Length;
                inds[index] = v.GetIndices(vertcount);
                indsCount += inds[index].Length;

                //colors.AddRange(v.GetColorData().ToList());
                texcoords[index] = v.GetTextureCoords();
                texcoordsCount += texcoords[index].Length;

                normals[index] = v.GetNormals();
                normalsCount += normals[index].Length;

                index++;
                vertcount += v.VertCount;
            }

            vertdata = new Vector3[vertsCount];
            indicedata = new int[indsCount];
            coldata = colors[0];
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex color if shader supports it
            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }


            // Buffer texture coordinates if shader supports it
            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            if (shaders[activeShader].GetAttribute("vNormal") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vNormal"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(normdata.Length * Vector3.SizeInBytes), normdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Update object positions
            time += (float)e.Time;


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
            spotLight.Type = LightType.Spot;
            spotLight.ConeAngle = 60.0f;
            spotLight.Direction = (
                new Vector3(
                    30 * (float)Math.Sin(time),
                    -1.0f,
                    30 * (float)Math.Cos(time))).Normalized();
            spotLight.QuadraticAttenuation = 0.01f;
            // Update model view matrices
            foreach (Volume v in objects)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = activeCamera.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 4000.0f);
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }

            GL.UseProgram(shaders[activeShader].ProgramID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);


            // Reset mouse position
            if (Focused)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                lastMousePos += delta;

                activeCamera.AddRotation(delta.X, delta.Y);
                ResetCursor();
            }

            view = activeCamera.GetViewMatrix();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            GL.UseProgram(shaders[activeShader].ProgramID);
            shaders[activeShader].EnableVertexAttribArrays();

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

                if (shaders[activeShader].GetUniform("light_position") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("light_position"), ref lights[0].Position);
                }

                if (shaders[activeShader].GetUniform("light_color") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("light_color"), ref lights[0].Color);
                }

                if (shaders[activeShader].GetUniform("light_diffuseIntensity") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("light_diffuseIntensity"), lights[0].DiffuseIntensity);
                }

                if (shaders[activeShader].GetUniform("light_ambientIntensity") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("light_ambientIntensity"), lights[0].AmbientIntensity);
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

            shaders[activeShader].DisableVertexAttribArrays();

            GL.Flush();
            SwapBuffers();
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

        int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException)
            {
                return -1;
            }
        }

        int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}
