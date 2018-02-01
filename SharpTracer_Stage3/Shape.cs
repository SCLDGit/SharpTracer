using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;
using System.Windows.Navigation;
using SharpTracer_Stage3;
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
        public Emitter m_material;

        public Light(Color c, double power)
        {
            m_color = c;
            m_power = power;
            m_material = new Emitter(c, power);
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
        //public Light light;

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
            i.m_pMaterial = new Emitter(m_color, m_power);
            i.m_colorModifier = new Color(1.0f, 1.0f, 1.0f);
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

    public class ShapeLight : Light
    {
        public Shape m_pShape;
        public Color m_color;
        public double m_power;
        public Material m_material;

        public ShapeLight(Shape pShape, Color c, double power)
        {
            m_pShape = pShape;
            m_color = c;
            m_power = power;
            m_material = new Emitter(c, power);
        }

        public override bool Intersect(ref Intersection i)
        {
            if (m_pShape.Intersect(ref i))
            {
                i.m_pMaterial = m_material;
                return true;
            }

            return false;
        }

        public override bool SampleSurface(double u1, double u2, Point referencePosition, ref Point outPosition,
            ref Vec3 outNormal)
        {
            return m_pShape.SampleSurface(u1, u2, referencePosition, ref outPosition, ref outNormal);
        }

        public override Color Emitted()
        {
            return m_color * m_power;
        }
    }

    public class Plane : Shape
    {
        public Point m_position;
        public Vec3 m_normal;
        public Material m_pMaterial;
        public bool m_bullseye;
        public Plane(Point position, Vec3 normal, Material pMaterial, bool bullseye = false)
        {
            m_position = position;
            m_normal = normal;
            m_pMaterial = pMaterial;
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
            i.m_pMaterial = m_pMaterial;
            i.m_colorModifier = new Color(1.0f, 1.0f, 1.0f);
            i.m_normal = m_normal;

            if (m_bullseye && ((i.Position() - m_position).Len() * 0.25f) % 1.0f > 0.5f)
            {
                i.m_colorModifier *= 0.2f;
            }

            return true;
        }
    }

    public class Sphere : Shape
    {
        public Point m_position;
        public double m_radius;
        public Material m_pMaterial;

        public Sphere( Point position, double radius, Material pMaterial )
        {
            m_position = position;
            m_radius = radius;
            m_pMaterial = pMaterial;
        }

        public override bool Intersect(ref Intersection i)
        {
            var localRay = new Ray(i.m_ray);
            localRay.m_origin -= m_position;

            double a = localRay.m_direction.Len2();
            double b = 2.0f * Vec3.Dot(localRay.m_direction, localRay.m_origin);
            double c = localRay.m_origin.Len2() - m_radius * m_radius;

            double t0, t1, discriminant;

            discriminant = b * b - 4.0f * a * c;
            if (discriminant < 0.0f)
            {
                return false;
            }

            discriminant = Math.Sqrt(discriminant);

            double q;
            if (b < 0.0f)
            {
                q = -0.5f * (b - discriminant);
            }
            else
            {
                q = -0.5f * (b + discriminant);
            }

            t0 = q / a;
            if (!q.Equals(0.0f))
            {
                t1 = c / q;
            }
            else
            {
                t1 = i.m_t;
            }

            if (t0 > t1)
            {
                double temp = t1;
                t1 = t0;
                t0 = temp;
            }

            if ( t0 >= i.m_t || t1 < Shared.kRayTMin )
            {
                return false;
            }

            if (t0 >= Shared.kRayTMin)
            {
                i.m_t = t0;
            }
            else if (t1 < i.m_t)
            {
                i.m_t = t1;
            }
            else
            {
                {
                    return false;
                }
            }

            Point localPos = localRay.Calculate(i.m_t);
            Vec3 worldNorm = localPos.Normalize();
            localPos = localPos.Normalize();

            i.m_pShape = this;
            i.m_pMaterial = m_pMaterial;
            i.m_normal = worldNorm;
            i.m_colorModifier = new Color(1.0f, 1.0f, 1.0f);

            return true;
        }

        public override bool SampleSurface(double u1, double u2, Point referencePosition, ref Point outPosition,
            ref Vec3 outNormal)
        {
            outNormal = UniformToSphere(u1, u2);
            outPosition = outNormal * m_radius + m_position;
            if (Vec3.Dot(outNormal, referencePosition - outPosition) < 0.0f)
            {
                outNormal *= -1.0f;
                outPosition = outNormal * m_radius + m_position;
            }
            return true;
        }
        

        protected Vec3 UniformToSphere(double u1, double u2)
        {
            var z = 1.0f - 2.0f * u1;
            var radius = Math.Sqrt(Math.Max(0.0f, 1.0f - z * z));
            var phi = Math.PI * 2.0f * u2;

            return new Vec3(radius * Math.Cos(phi), radius * Math.Sin(phi), z);
        }
    }
}
