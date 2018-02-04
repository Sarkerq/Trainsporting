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

namespace Trainsporting
{
    class Game : GameWindow
    {
        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        string activeShader = "default";

        int ibo_elements;

        Dictionary<String, Material> materials = new Dictionary<string, Material>();

        Dictionary<string, int> textures = new Dictionary<string, int>();

        Vector3[] vertdata;
        Vector3[] coldata;
        Vector2[] texcoorddata;
        Vector3[] normdata;

        List<Volume> objects = new List<Volume>();
        int[] indicedata;

        List<Light> lights = new List<Light>();
        const int MAX_LIGHTS = 5;

        Camera cam = new Camera();

        float time = 0.0f;
        Vector2 lastMousePos = new Vector2();

        Matrix4 view = Matrix4.Identity;

        public Game() : base(512, 512, new GraphicsMode(32, 24, 0, 4))
        {

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            Title = "Hello OpenTK!";
            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(5f);
        }
        void initProgram()
        {
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);
            CursorVisible = false;

            GL.GenBuffers(1, out ibo_elements);

            loadResources();

            activeShader = "lit_advanced";

            setupScene();
        }
        private void loadResources()
        {
            // Load shaders from file
            shaders.Add("default", new ShaderProgram("vs.glsl", "fs.glsl", true));
            shaders.Add("textured", new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", true));
            shaders.Add("normal", new ShaderProgram("vs_norm.glsl", "fs_norm.glsl", true));
            shaders.Add("lit", new ShaderProgram("vs_lit.glsl", "fs_lit.glsl", true));
            shaders.Add("lit_multiple", new ShaderProgram("vs_lit.glsl", "fs_lit_multiple.glsl", true));
            shaders.Add("lit_advanced", new ShaderProgram("vs_lit.glsl", "fs_lit_advanced.glsl", true));

            // Load materials and textures
            loadMaterials("opentk.mtl");
            loadMaterials("earth.mtl");
            loadMaterials("train.mtl");

        }

        private void setupScene()
        {
            // Create our objects
            TexturedCube tc = new TexturedCube();
            tc.TextureID = textures[materials["opentk1"].DiffuseMap];
            tc.CalculateNormals();
            tc.Material = materials["opentk1"];
            objects.Add(tc);

            TexturedCube tc2 = new TexturedCube();
            tc2.Position += new Vector3(1f, 1f, 1f);
            tc2.TextureID = textures[materials["opentk2"].DiffuseMap];
            tc2.CalculateNormals();
            tc2.Material = materials["opentk2"];
            objects.Add(tc2);

            ObjVolume earth = ObjVolume.LoadFromFile("earth.obj");
            earth.TextureID = textures["earth.png"];
            earth.Position += new Vector3(1f, 1f, -2f);
            earth.Material = materials["earth"];
            objects.Add(earth);

            ObjVolume train = ObjVolume.LoadFromFile("train.obj");
            train.TextureID = textures["earth.png"];
            train.Position += new Vector3(1f, 1f, -2f);
            train.Material = materials["train"];
            objects.Add(train);

            TexturedCube floor = new TexturedCube();
            floor.TextureID = textures[materials["opentk1"].DiffuseMap];
            floor.Scale = new Vector3(20, 0.1f, 20);
            floor.Position += new Vector3(0, -2, 0);
            floor.CalculateNormals();
            floor.Material = materials["opentk1"];
            objects.Add(floor);

            TexturedCube backWall = new TexturedCube();
            backWall.TextureID = textures[materials["opentk1"].DiffuseMap];
            backWall.Scale = new Vector3(20, 20, 0.1f);
            backWall.Position += new Vector3(0, 8, -10);
            backWall.CalculateNormals();
            backWall.Material = materials["opentk1"];
            objects.Add(backWall);

            // Create lights
            Light sunLight = new Light(new Vector3(), new Vector3(0.7f, 0.7f, 0.7f));
            sunLight.Type = LightType.Directional;
            sunLight.Direction = (sunLight.Position - floor.Position).Normalized();
            lights.Add(sunLight);



            Light pointLight = new Light(new Vector3(2, 7, 0), new Vector3(1.5f, 0.2f, 0.2f));
            pointLight.QuadraticAttenuation = 0.05f;
            lights.Add(pointLight);

            Light pointLight2 = new Light(new Vector3(2, 0, 3), new Vector3(0.2f, 1f, 0.25f));
            pointLight2.QuadraticAttenuation = 0.05f;
            lights.Add(pointLight2);

            Light pointLight3 = new Light(new Vector3(6, 4, 0), new Vector3(0.2f, 0.25f, 1.5f));
            pointLight3.QuadraticAttenuation = 0.05f;
            lights.Add(pointLight3);

            // Move camera away from origin
            cam.Position += new Vector3(0f, 1f, 3f);
        }
        private void loadMaterials(String filename)
        {
            foreach (var mat in Material.LoadFromFile(filename))
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

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

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

            objects[0].Position = new Vector3(0.3f, -0.5f + (float)Math.Sin(time), -3.0f);
            objects[0].Rotation = new Vector3(0.55f * time, 0.25f * time, 0);
            objects[0].Scale = new Vector3(0.5f, 0.5f, 0.5f);

            objects[1].Position = new Vector3(-1f, 0.5f + (float)Math.Cos(time), -2.0f);
            objects[1].Rotation = new Vector3(-0.25f * time, -0.35f * time, 0);
            objects[1].Scale = new Vector3(0.7f, 0.7f, 0.7f);

            // Update model view matrices
            foreach (Volume v in objects)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = cam.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);
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

                cam.AddRotation(delta.X, delta.Y);
                ResetCursor();
            }

            view = cam.GetViewMatrix();
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
                case 'w':
                    cam.Move(0f, 0.1f, 0f);
                    break;
                case 'a':
                    cam.Move(-0.1f, 0f, 0f);
                    break;
                case 's':
                    cam.Move(0f, -0.1f, 0f);
                    break;
                case 'd':
                    cam.Move(0.1f, 0f, 0f);
                    break;
                case 'q':
                    cam.Move(0f, 0f, 0.1f);
                    break;
                case 'e':
                    cam.Move(0f, 0f, -0.1f);
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
