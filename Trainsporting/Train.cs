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

        public Train(ObjVolume model, List<Track> tracks)
        {
            Model = model;
            Tracks = tracks;
            currentTrackIndex = 1;
        }

        public void UpdatePosition(float velocity)
        {
            float partOfTrackPassed = PartOfTrackPassed();
            Model.Rotation = 
                Vector3.Multiply(new Vector3(Tracks[currentTrackIndex].Model.Rotation), 1 - partOfTrackPassed) +
                Vector3.Multiply(new Vector3(Tracks[NextTrackIndex()].Model.Rotation), partOfTrackPassed);
            Model.Position += new Vector3((float)(velocity * Math.Sin(Model.Rotation[1])), 0, (float)(velocity * Math.Cos(Model.Rotation[1])));

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
            Vector3 distance = Model.Position - Tracks[PreviousTrackIndex()].Model.Position - trainTrackOffsetAngled;
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
                (float)Track.TRAIN_TRACK_OFFSET[0] * (float)Math.Sin(Model.Rotation[1]),
                Track.TRAIN_TRACK_OFFSET[1],
                (float)Track.TRAIN_TRACK_OFFSET[2] * (float)Math.Cos(Model.Rotation[1]));
            Model.Position = new Vector3(Tracks[currentTrackIndex].Model.Position) + trainTrackOffsetAngled;
            Model.Rotation = new Vector3(Tracks[NextTrackIndex()].Model.Rotation);

        }

        private int NextTrackIndex()
        {
           return (currentTrackIndex + 1) % Tracks.Count;
        }
        private int PreviousTrackIndex()
        {
            return (currentTrackIndex - 1 + Tracks.Count) % Tracks.Count;
        }
    }
}