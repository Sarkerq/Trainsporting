using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trainsporting
{
    public class Track
    {
        public Volume Model;
        public int Path;
        public bool isBranchingPoint;
        public List<int> branchingPointIndexes;


        public static float TRACK_LENGTH = 6.9f;
        public static Vector3 TRAIN_TRACK_OFFSET = new Vector3(-1.1f, 2.3f, 1.4f);
        public static float BRANCHING_POINT_ANGLE = (float)Math.PI / 80;

        public Track(Volume model, int path)
        {
            Model = model;
            Path = path;
        }

        public Track(Track lastTrack, float angleRadians, int path = -1)
        {
            if (path == -1)
                Path = lastTrack.Path;
            else
                Path = path;
            Vector3 newRotation = lastTrack.Model.Rotation + new Vector3(0, angleRadians, 0);
            Model = Volume.VolumeFactory("track.obj", "basic2.png", "AVE-BLANCO",
                lastTrack.Model.Position + new Vector3((float)Math.Sin(newRotation[1]) * TRACK_LENGTH, 0, (float)Math.Cos(newRotation[1]) * TRACK_LENGTH),
                newRotation,
                new Vector3(0.75f, 0.75f, 2.4f));
        }
        public List<Track> MakeBranchingPoint()
        {
            Track leftTrack = new Track(this, Track.BRANCHING_POINT_ANGLE, 0);
            Track rightTrack = new Track(this, 0, 1);
            isBranchingPoint = true;
            return new List<Track>() { leftTrack, rightTrack };
        }
        public static void UpdateBranchingPoints(List<Track> tracks)
        {
            for(int i = 0; i< tracks.Count;i++)
            {
                Track track = tracks[i];
                if (track.isBranchingPoint)
                {
                    track.branchingPointIndexes = new List<int>();
                    track.branchingPointIndexes.Add(i + 1);
                    int rightIndex = i + 2;
                    while (tracks[rightIndex].Path == 0)
                        rightIndex++;
                    track.branchingPointIndexes.Add(rightIndex);
                }
            }
        }
    }
}
