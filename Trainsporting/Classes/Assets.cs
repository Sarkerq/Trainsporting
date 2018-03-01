using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using static Trainsporting.Game;

namespace Trainsporting.Classes
{
    static public class Assets
    {

        public static void loadResources()
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

        public static void loadMaterials(String filename)
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
        public static int loadImage(string filename)
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

        public static int loadImage(Bitmap image)
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
