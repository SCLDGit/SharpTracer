using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace SharpTracer
{
    public class Vec3
    {
        public double m_x;
        public double m_y;
        public double m_z;

        public Vec3()
        {
            m_x = 0.0f;
            m_y = 0.0f;
            m_z = 0.0f;
        }

        public Vec3( Vec3 v)
        {
            m_x = v.m_x;
            m_y = v.m_y;
            m_z = v.m_z;
        }

        public Vec3( double x, double y, double z)
        {
            m_x = x;
            m_y = y;
            m_z = z;
        }

        public Vec3( double f)
        {
            m_x = f;
            m_y = f;
            m_z = f;
        }

        public double Len2()
        {
            return m_x * m_x + m_y * m_y + m_z * m_z;
        }

        public double Len()
        {
            return Math.Sqrt(Len2());
        }

        public Vec3 Normalize()
        {
            var v = this;
            v = v / Len();
            return v;
        }

        public void Equals(Vec3 v)
        {
            m_x = v.m_x;
            m_y = v.m_y;
            m_z = v.m_z;
        }

        public void Add( Vec3 v )
        {
            m_x += v.m_x;
            m_y += v.m_y;
            m_z += v.m_z;
        }

        public void Sub( Vec3 v )
        {
            m_x -= v.m_x;
            m_y -= v.m_y;
            m_z -= v.m_z;
        }

        public void MultVec3( Vec3 v )
        {
            m_x *= v.m_x;
            m_y *= v.m_y;
            m_z *= v.m_z;
        }

        public void DivVec3( Vec3 v )
        {
            m_x /= v.m_x;
            m_y /= v.m_y;
            m_z /= v.m_z;
        }

        public void MultValue( double f )
        {
            m_x *= f;
            m_y *= f;
            m_z *= f;
        }

        public void DivValue( double f )
        {
            m_x /= f;
            m_y /= f;
            m_z /= f;
        }
        public static Vec3 operator +( Vec3 v1, Vec3 v2 )
        {
            return new Vec3(v1.m_x + v2.m_x,
                v1.m_y + v2.m_y,
                v1.m_z + v2.m_z);
        }
        public static Vec3 operator -( Vec3 v1, Vec3 v2 )
        {
            return new Vec3(v1.m_x - v2.m_x,
                v1.m_y - v2.m_y,
                v1.m_z - v2.m_z);
        }
        public static Vec3 operator *( Vec3 v, double f )
        {
            return new Vec3(v.m_x * f,
                v.m_y * f,
                v.m_z * f);
        }
        public static Vec3 operator *( double f, Vec3 v )
        {
            return new Vec3(v.m_x * f,
                v.m_y * f,
                v.m_z * f);
        }
        public static Vec3 operator *( Vec3 v1, Vec3 v2 )
        {
            return new Vec3(v1.m_x * v2.m_x,
                v1.m_y * v2.m_y,
                v1.m_z * v2.m_z);
        }
        public static Vec3 operator /(Vec3 v, double f)
        {
            return new Vec3(v.m_x / f,
                            v.m_y / f,
                            v.m_z / f);
        }

        public static double Dot(Vec3 v1, Vec3 v2)
        {
            return v1.m_x * v2.m_x + v1.m_y * v2.m_y + v1.m_z * v2.m_z;
        }

        public static Vec3 Cross(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.m_y * v2.m_z - v1.m_z * v2.m_y,
                            v1.m_z * v2.m_x - v1.m_x * v2.m_z,
                            v1.m_x * v2.m_y - v1.m_y * v2.m_x);
        }
    }
}
