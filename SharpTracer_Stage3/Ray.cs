using Point = SharpTracer.Vec3;

namespace SharpTracer
{ 
    public class Ray
    {
        public bool m_isShadowRay;
        public Point m_origin;
        public Vec3 m_direction;
        public double m_tMax;

        public Ray()
        {
            m_isShadowRay = false;
            m_origin = new Point();
            m_direction = new Vec3(0.0f, 0.0f, 1.0f);
            m_tMax = Shared.kRayTMax;
        }

        public Ray(Ray r)
        {
            m_isShadowRay = r.m_isShadowRay;
            m_origin = r.m_origin;
            m_direction = r.m_direction;
            m_tMax = r.m_tMax;
        }

        public Ray(bool isShadowRay, Point origin, Vec3 direction, double tMax = Shared.kRayTMax)
        {
            m_isShadowRay = isShadowRay;
            m_origin = origin;
            m_direction = direction;
            m_tMax = tMax;
        }

        public void Equals(Ray r)
        {
            m_isShadowRay = r.m_isShadowRay;
            m_origin = r.m_origin;
            m_direction = r.m_direction;
            m_tMax = r.m_tMax;
        }

        public Point Calculate(double t)
        {
            return m_origin + t * m_direction;
        }
    }
}
