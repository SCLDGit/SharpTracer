using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using SharpTracer_Stage3;
using Point = SharpTracer.Vec3;

namespace SharpTracer
{
    public class Intersection
    {
        public Ray m_ray;
        public double m_t;
        public Shape m_pShape;
        public Material m_pMaterial;
        public Color m_colorModifier;
        public Vec3 m_normal;

        public Intersection()
        {
            m_ray = new Ray();
            m_t = Shared.kRayTMax;
            m_pShape = null;
            m_pMaterial = null;
            m_colorModifier = new Color(1.0f, 1.0f, 1.0f);
            m_normal = new Vec3();
        }

        public Intersection(ref Intersection i)
        {
            m_ray = i.m_ray;
            m_t = i.m_t;
            m_pShape = i.m_pShape;
            m_pMaterial = i.m_pMaterial;
            m_colorModifier = i.m_colorModifier;
            m_normal = i.m_normal;
        }

        public Intersection(ref Ray r)
        {
            m_ray = r;
            m_t = r.m_tMax;
            m_pShape = null;
            m_pMaterial = null;
            m_colorModifier = new Color(1.0f, 1.0f, 1.0f);
            m_normal = new Vec3();
        }

        public void Equals(ref Intersection i)
        {
            m_ray = i.m_ray;
            m_t = i.m_t;
            m_pShape = i.m_pShape;
            m_pMaterial = i.m_pMaterial;
            m_colorModifier = i.m_colorModifier;
            m_normal = i.m_normal;
        }

        public bool Intersected()
        {
            return (m_pShape != null);
        }

        public Point Position()
        {
            return m_ray.Calculate(m_t);
        }
    }
}
