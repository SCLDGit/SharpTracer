﻿using System;

namespace SharpTracer
{
    public class Color
    {
        public double m_r;
        public double m_g;
        public double m_b;

        public Color()
        {
            m_r = 0.0f;
            m_g = 0.0f;
            m_b = 0.0f;
        }

        public Color(Color c)
        {
            m_r = c.m_r;
            m_g = c.m_g;
            m_b = c.m_b;
        }

        public Color(double r, double g, double b)
        {
            m_r = r;
            m_g = g;
            m_b = b;
        }

        public Color(double f)
        {
            m_r = f;
            m_g = f;
            m_b = f;
        }

        public void Clamp(double min = 0.0f, double max = 1.0f)
        {
            m_r = Math.Max(min, Math.Min(max, m_r));
            m_g = Math.Max(min, Math.Min(max, m_g));
            m_b = Math.Max(min, Math.Min(max, m_b));
        }

        public void Equals(Color c)
        {
            m_r = c.m_r;
            m_g = c.m_g;
            m_b = c.m_b;
        }

        public void Add(Color c)
        {
            m_r += c.m_r;
            m_g += c.m_g;
            m_b += c.m_b;
        }

        public void Sub(Color c)
        {
            m_r -= c.m_r;
            m_g -= c.m_g;
            m_b -= c.m_b;
        }

        public void MultColor(Color c)
        {
            m_r *= c.m_r;
            m_g *= c.m_g;
            m_b *= c.m_b;
        }

        public void DivColor( Color c )
        {
            m_r /= c.m_r;
            m_g /= c.m_g;
            m_b /= c.m_b;
        }

        public void MultValue( double f )
        {
            m_r *= f;
            m_g *= f;
            m_b *= f;
        }

        public void DivValue( double f )
        {
            m_r /= f;
            m_g /= f;
            m_b /= f;
        }

        public static Color operator +( Color c1, Color c2 )
        {
            return new Color(c1.m_r + c2.m_r,
                             c1.m_g + c2.m_g,
                             c1.m_b + c2.m_b);
        }
        public static Color operator -( Color c1, Color c2 )
        {
            return new Color(c1.m_r - c2.m_r,
                c1.m_g - c2.m_g,
                c1.m_b - c2.m_b);
        }
        public static Color operator *( Color c1, Color c2 )
        {
            return new Color(c1.m_r * c2.m_r,
                c1.m_g * c2.m_g,
                c1.m_b * c2.m_b);
        }

        public static Color operator /( Color c1, Color c2 )
        {
            return new Color(c1.m_r / c2.m_r,
                c1.m_g / c2.m_g,
                c1.m_b / c2.m_b);
        }
        public static Color operator *( Color c, double f )
        {
            return new Color(c.m_r * f,
                c.m_g * f,
                c.m_b * f);
        }

        public static Color operator *( double f, Color c )
        {
            return new Color(c.m_r * f,
                c.m_g * f,
                c.m_b * f);
        }
        public static Color operator /( double f, Color c )
        {
            return new Color(c.m_r / f,
                c.m_g / f,
                c.m_b / f);
        }
    }
}
