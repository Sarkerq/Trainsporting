using OpenTK;
using System;
using System.Collections.Generic;

namespace Trainsporting
{
    class Train
    {
        public ObjVolume Model;
        public List<Track> Tracks;
        public int currentTrackIndex;
        public Vector3 TargetOffset = new Vector3();
        public int branchSetting = 1;
        public float Velocity = 0;
        public static Vector3 ONBOARD_TRAIN_OFFSET = new Vector3(-20f, 10f, -20f);
        public Train(ObjVolume model, List<Track> tracks)
        {
            Model = model;
            Tracks = tracks;
            currentTrackIndex = 0;
            Track.UpdateBranchingPoints(tracks);
        }

        public void UpdatePosition()
        {
            float partOfTrackPassed = PartOfTrackPassed();
            partOfTrackPassed = Math.Max(Math.Min(1, partOfTrackPassed), 0);
            if (TrackIndexPlusTwo() == 0)
            {
                Model.Rotation =
                    Vector3.Multiply(new Vector3(Tracks[NextTrackIndex()].Model.Rotation), 1 - partOfTrackPassed ) +
                    Vector3.Multiply(new Vector3(Tracks[TrackIndexPlusTwo()].Model.Rotation[0],
                                                  Tracks[TrackIndexPlusTwo()].Model.Rotation[1] + 2 * (float)Math.PI,
                                                  Tracks[TrackIndexPlusTwo()].Model.Rotation[2]), partOfTrackPassed);
            }
            else
            {
                Model.Rotation =
                    Vector3.Multiply(new Vector3(Tracks[NextTrackIndex()].Model.Rotation), 1 - partOfTrackPassed) +
                    Vector3.Multiply(new Vector3(Tracks[TrackIndexPlusTwo()].Model.Rotation), partOfTrackPassed);
            }
            Model.Position += new Vector3((float)(Velocity * Math.Sin(Model.Rotation[1])), 0, (float)(Velocity * Math.Cos(Model.Rotation[1])));
            Model.Position += Vector3.Multiply(TargetOffset, 0.1f);
            TargetOffset = Vector3.Multiply(TargetOffset, 0.9f);
            if (TrainOnNextTrack())
            {
                SetNextTrack();
            }
        }



        private float PartOfTrackPassed()
        {
            Vector3 trainTrackOffsetAngled = new Vector3(
               (float)Track.TRAIN_TRACK_OFFSET[0] * (float)Math.Sin(Model.Rotation[1]),
               Track.TRAIN_TRACK_OFFSET[1],
               (float)Track.TRAIN_TRACK_OFFSET[2] * (float)Math.Cos(Model.Rotation[1]));
            Vector3 distance = Model.Position - Tracks[currentTrackIndex].Model.Position - trainTrackOffsetAngled;
            return (float)Math.Sqrt(distance[0] * distance[0] + distance[1] * distance[1] + distance[2] * distance[2]) / Track.TRACK_LENGTH;
        }

        private bool TrainOnNextTrack()
        {
            return PartOfTrackPassed() > 1.0;
        }


        private void SetNextTrack()
        {
            currentTrackIndex = NextTrackIndex();
            Vector3 trainTrackOffsetAngled = new Vector3(
                (float)Track.TRAIN_TRACK_OFFSET[0] * (float)Math.Cos(Tracks[PreviousTrackIndex()].Model.Rotation[1]) 
               ,
                Track.TRAIN_TRACK_OFFSET[1],
                (float)Track.TRAIN_TRACK_OFFSET[2] * (float)Math.Sin(Tracks[PreviousTrackIndex()].Model.Rotation[1]));
            Vector3 TargetPosition = new Vector3(Tracks[currentTrackIndex].Model.Position) + trainTrackOffsetAngled;
            TargetOffset = TargetPosition - Model.Position;
            //Model.Rotation = new Vector3(Tracks[NextTrackIndex()].Model.Rotation);

        }

        private int NextTrackIndex()
        {
            int newIndex = currentTrackIndex;
            if(Tracks[newIndex].isBranchingPoint)
            {
                return Tracks[newIndex].branchingPointIndexes[branchSetting];
            }
            if(Tracks[newIndex].Path == 0)
            {
                do
                {
                    newIndex = (newIndex + 1) % Tracks.Count;
                }
                while (Tracks[newIndex].Path != 0);

                return newIndex;
            }
            return (newIndex + 1) % Tracks.Count;
        }
        private int TrackIndexPlusTwo()
        {
            int newIndex = NextTrackIndex();
            if (Tracks[newIndex].isBranchingPoint)
            {
                return Tracks[newIndex].branchingPointIndexes[branchSetting];
            }
            if (Tracks[newIndex].Path == 0)
            {
                do
                {
                    newIndex = (newIndex + 1) % Tracks.Count;
                }
                while (Tracks[newIndex].Path != 0);

                return newIndex;
            }
            return (newIndex + 1) % Tracks.Count;
        }
        private int PreviousTrackIndex()
        {
            return (currentTrackIndex - 1 + Tracks.Count) % Tracks.Count;
        }
        public void SwitchBranch()
        {
            if (branchSetting == 0)
                branchSetting = 1;
            if (branchSetting == 1)
                branchSetting = 0;
        }

        internal void Accelerate()
        {
            if (Velocity < 0.1)
                Velocity = Math.Max(2.25f * Velocity, 0.01f);
            else if (Velocity < 1)
                Velocity = 1.25f * Velocity;
            else if (Velocity < 2.5)
                Velocity = 1.05f * Velocity;
            else
                Velocity = Math.Min(6f, 1.02f * Velocity);
        }

        internal void Decelerate()
        {
            Velocity = Math.Max(0, Velocity - 0.05f);
        }
    }
}