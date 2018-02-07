using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trainsporting
{
    class Track
    {
        public ObjVolume Model;
        public int Path;

        public static float TRACK_LENGTH = 6.9f;

        public static Vector3 TRAIN_TRACK_OFFSET = new Vector3(-1.1f, 2.3f, -2f);

        public Track(ObjVolume model, int path)
        {
            Model = model;
            Path = path;
        }

        public Track(Track lastTrack, float angleRadians)
        {
            Path = lastTrack.Path;
            Model = ObjVolume.LoadFromFile("track.obj");
            Model.TextureID = Game.textures["basic2.png"];
            Model.Rotation = lastTrack.Model.Rotation + new Vector3(0, angleRadians, 0);
            Model.Position = lastTrack.Model.Position + new Vector3((float)Math.Sin(Model.Rotation[1]) * TRACK_LENGTH, 0, (float)Math.Cos(Model.Rotation[1]) * TRACK_LENGTH);
            Model.Scale = new Vector3(0.75f, 0.75f, 2.4f);
            Model.Material = Game.materials["AVE-BLANCO"];
        }
    }
}
