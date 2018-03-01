using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trainsporting
{

    public class Light
    {
        public Light(Vector3 position, Vector3 color, float diffuseintensity = 0.8f, float ambientintensity = 0.8f, float specularintensity = 0.8f)
        {
            Position = position;
            Color = color;

            DiffuseIntensity = diffuseintensity;
            AmbientIntensity = ambientintensity;
            SpecularIntensity = specularintensity;
            Type = LightType.Point;
            Direction = new Vector3(0, 0, 1);
            ConeAngle = 15.0f;
        }

        public Vector3 Position;
        public Vector3 Color;

        public float AmbientIntensity;
        public float DiffuseIntensity;
        public float SpecularIntensity;

        public LightType Type;
        public Vector3 Direction;
        public float ConeAngle;

        public float LinearAttenuation;
        public float QuadraticAttenuation;

    }

    public enum LightType { Point, Spot }
}
