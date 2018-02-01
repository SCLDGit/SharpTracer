using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Point = SharpTracer.Vec3;

namespace SharpTracer
{
    public class Shape
    {
        public Shape()
        {

        }

        public virtual bool Intersect(ref Intersection i)
        {
            return false;
        }

        public virtual bool SampleSurface(double u1, double u2, Point referencePosition, ref Point outPosition, ref Vec3 outNormal)
        {
            return false;
        }

        public virtual void FindLights(List<Shape> outLightList)
        {

        }

        public virtual Color Emitted()
        {
            return new Color();
        }
    }

    public class ShapeSet : Shape
    {
        public List<Shape> m_shapes = new List<Shape>();
        public ShapeSet()
        {

        }

        public virtual bool Intersect(ref Intersection i)
        {
            var intersectedAny = false;
            foreach (var shape in m_shapes)
            {
                var pShape = shape;
                var intersected = pShape.Intersect(ref i);
                if (intersected)
                {
                    intersectedAny = true;
                }
            }

            return intersectedAny;
        }

        public virtual void FindLights(List<Shape> outLightList)
        {
            foreach (var shape in m_shapes)
            {
                var pShape = shape;
                pShape.FindLights(outLightList);
            }
        }

        public void AddShape(Shape pShape)
        {
            m_shapes.Add(pShape);
        }

        public void ClearShapes()
        {
            m_shapes.Clear();
        }
    }

    public class Light : Shape
    {
        public Color m_color;
        public double m_power;

        public Light(Color c, double power)
        {
            m_color = c;
            m_power = power;
        }

        public Light()
        {

        }

        public override void FindLights(List<Shape> outLightList)
        {
            outLightList.Add(this);
        }

        public override Color Emitted()
        {
            return m_color * m_power;
        }
    }

    public class RectangleLight : Light
    {
        public Point m_position;
        public Vec3 m_side1;
        public Vec3 m_side2;
        public Light light;

        public RectangleLight(Point pos, Vec3 side1, Vec3 side2, Color color, double power)
        {
            //light = new Light(color, power);
            m_color = color;
            m_power = power;
            m_position = pos;
            m_side1 = side1;
            m_side2 = side2;
        }

        public RectangleLight()
        {

        }

        public override bool Intersect(ref Intersection i)
        {
            var normal = Vec3.Cross(m_side1, m_side2).Normalize();
            var nDotD = Vec3.Dot(normal, i.m_ray.m_direction);
            if (nDotD.Equals(0.0f))
            {
                return false;
            }

            var t = (Vec3.Dot(m_position, normal) - Vec3.Dot(i.m_ray.m_origin, normal)) /
                       Vec3.Dot(i.m_ray.m_direction, normal);

            if (t >= i.m_t || t < Shared.kRayTMin)
            {
                return false;
            }

            var side1Length = m_side1.Len();
            var side2Length = m_side2.Len();
            var side1Norm = m_side1.Normalize();
            var side2Norm = m_side2.Normalize();

            var worldPoint = i.m_ray.Calculate(t);
            var worldRelativePoint = worldPoint - m_position;
            var localPoint = new Point(Vec3.Dot(worldRelativePoint, side1Norm),
                Vec3.Dot(worldRelativePoint, side2Norm),
                0.0f);

            if (localPoint.m_x < 0.0f || localPoint.m_x > side1Length ||
                localPoint.m_y < 0.0f || localPoint.m_y > side2Length)
            {
                return false;
            }

            i.m_t = t;
            i.m_pShape = this;
            i.m_color = new Color();
            i.m_emitted = m_color * m_power;
            i.m_normal = normal;

            if (Vec3.Dot(i.m_normal, i.m_ray.m_direction) > 0.0f)
            {
                i.m_normal *= -1.0f;
            }

            return true;
        }

        public override bool SampleSurface(double u1, double u2, Point referencePosition, ref Point outPosition,
            ref Vec3 outNormal)
        {
            outNormal = Vec3.Cross(m_side1, m_side2).Normalize();
            outPosition = m_position + m_side1 * u1 + m_side2 * u2;

            if (Vec3.Dot(outNormal, outPosition - referencePosition) > 0.0f)
            {
                outNormal *= -1.0f;
            }

            return true;
        }

        //public override Color Emitted()
        //{
        //    return m_color * m_power;
        //}
    }

    public class Plane : Shape
    {
        public Point m_position;
        public Vec3 m_normal;
        public Color m_color;
        public bool m_bullseye;
        public Plane(Point position, Vec3 normal, Color color, bool bullseye = false)
        {
            m_position = position;
            m_normal = normal;
            m_color = color;
            m_bullseye = bullseye;
        }

        public override bool Intersect(ref Intersection i)
        {
            var nDotD = Vec3.Dot(m_normal, i.m_ray.m_direction);
            if (nDotD >= 0.0f)
            {
                return false;
            }

            var t = (Vec3.Dot(m_position, m_normal) - Vec3.Dot(i.m_ray.m_origin, m_normal)) /
                       Vec3.Dot(i.m_ray.m_direction, m_normal);

            if (t >= i.m_t || t < Shared.kRayTMin)
            {
                return false;
            }

            i.m_t = t;
            i.m_pShape = this;
            i.m_color = m_color;
            i.m_emitted = new Color();
            i.m_normal = m_normal;

            if (m_bullseye && ((i.Position() - m_position).Len() * 0.25f) % 1.0f > 0.5f)
            {
                i.m_color *= 0.2f;
            }

            return true;
        }
    }
}
