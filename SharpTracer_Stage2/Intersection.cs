using Point = SharpTracer.Vec3;

namespace SharpTracer
{
    public class Intersection
    {
        public Ray m_ray;
        public double m_t;
        public Shape m_pShape;
        public Color m_color;
        public Color m_emitted;
        public Vec3 m_normal;

        public Intersection()
        {
            m_ray = new Ray();
            m_t = Shared.kRayTMax;
            m_pShape = null;
            m_color = new Color();
            m_emitted = new Color();
            m_normal = new Vec3();
        }

        public Intersection(ref Intersection i)
        {
            m_ray = i.m_ray;
            m_t = i.m_t;
            m_pShape = i.m_pShape;
            m_color = i.m_color;
            m_emitted = i.m_emitted;
            m_normal = i.m_normal;
        }

        public Intersection(ref Ray r)
        {
            m_ray = r;
            m_t = r.m_tMax;
            m_pShape = null;
            m_color = new Color();
            m_emitted = new Color();
            m_normal = new Vec3();
        }

        public void Equals(ref Intersection i)
        {
            m_ray = i.m_ray;
            m_t = i.m_t;
            m_pShape = i.m_pShape;
            m_color = i.m_color;
            m_emitted = i.m_emitted;
            m_normal = i.m_normal;
        }

        public bool Intersected()
        {
            return m_pShape != null;
        }

        public Point Position()
        {
            return m_ray.Calculate(m_t);
        }
    }
}
