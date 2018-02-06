using System;

using SharpTracer;

using Point = SharpTracer.Vec3;

namespace SharpTracer_Stage3
{
    public class Material
    {
        public virtual Color Emittance()
        {
            return new Color();
        }

        public virtual Color Shade(Point position, Vec3 normal, Vec3 incomingRayDirection, Vec3 lightDirection)
        {
            return new Color();
        }
    }

    public class Lambert : Material
    {
        private readonly Color m_color;

        public Lambert(Color c)
        {
            m_color = c;
        }

        public override Color Shade(Point position, Vec3 normal, Vec3 incomingRayDirection, Vec3 lightDirectionNorm)
        {
            return Math.Max(0.0f, Vec3.Dot(lightDirectionNorm, normal)) * m_color;
        }
    }

    public class Phong : Material
    {
        private readonly Color m_color;
        private readonly double m_exponent;

        public Phong( Color c, double exponent)
        {
            m_color = c;
            m_exponent = exponent;
        }

        public override Color Shade( Point position, Vec3 normal, Vec3 incomingRayDirection, Vec3 lightDirectionNorm )
        {
            var halfVec = (lightDirectionNorm - incomingRayDirection).Normalize();
            return Math.Pow(Math.Max(0.0f, Vec3.Dot(halfVec, normal)), m_exponent) * m_color;
        }
    }

    public class Emitter : Material
    {
        private readonly Color m_color;
        private readonly double m_power;

        public Emitter( Color c, double power )
        {
            m_color = c;
            m_power = power;
        }

        public override Color Emittance()
        {
            return m_color * m_power;
        }

        public override Color Shade( Point position, Vec3 normal, Vec3 incomingRayDirection, Vec3 lightDirectionNorm )
        {
            return new Color();
        }
    }
}
