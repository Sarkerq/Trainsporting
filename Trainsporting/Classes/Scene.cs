using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trainsporting;

using static Trainsporting.Game;

namespace Trainsporting.Classes
{
    public class Scene
    {
        static public void initProgram()
        {
            GL.GenBuffers(1, out ibo_elements);
            Assets.loadResources();
            activeShader = "phong";
            setupScene();
        }
        static void setupScene()
        {
            setupObjects();
            setupLights();
            setupCameras();
        }

        static void setupCameras()
        {
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

        static void setupLights()
        {
            spotLight = new Light(train.Model.Position + new Vector3(0, 1.0f, -3.0f), new Vector3(1.0f, 0.0f, 0.0f));
            spotLight.Type = LightType.Spot;
            spotLight.ConeAngle = 60.0f;
            spotLight.QuadraticAttenuation = 0.01f;
            spotLight.Direction = (spotLight.Position - new Vector3(100, 1.0f, -3.0f)).Normalized();
            lights.Add(spotLight);

            for (int i = 0; i < tracks.Count; i += tracks.Count / (MAX_LIGHTS - 1))
            {
                Light pointLight = new Light(new Vector3(2, 20, 2) + tracks[i].Model.Position, new Vector3(1.0f, 1.0f, 1.0f));
                pointLight.Type = LightType.Point;
                pointLight.QuadraticAttenuation = 0.0008f;
                lights.Add(pointLight);
            }
        }

        static void setupObjects()
        {
            setupTrainAndTracks();
            setupTrees();
            setupRock();
            setupFloor();
        }

        static void setupFloor()
        {
            TexturedCube floor = new TexturedCube();
            floor.TextureID = textures[materials["opentk1"].DiffuseMap];
            floor.Scale = new Vector3(2000, 0.1f, 2000);
            floor.Position += new Vector3(0, -2, 0);
            floor.CalculateNormals();
            floor.Material = materials["opentk1"];
            objects.Add(floor);
        }

        static void setupRock()
        {
            ObjVolume rockModel = ObjVolume.ObjVolumeFactory("rock.obj", "rock.png", "AVE-BLANCO",
                    new Vector3(-0.0f, 40.7f, -300f),
                    new Vector3(0, (float)Math.PI / 2, 0),
                    new Vector3(5.0f, 5.0f, 5.0f));
            objects.Add(rockModel);
        }

        static void setupTrees()
        {
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
        }

        static void setupTrainAndTracks()
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
        }

    }
}
