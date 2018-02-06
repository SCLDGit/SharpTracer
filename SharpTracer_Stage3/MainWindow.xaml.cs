using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

using SharpTracer_Stage3;

using Point = SharpTracer.Vec3;

namespace SharpTracer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
         public MainWindow()
        {
            InitializeComponent();
        }

        public class Rng
        {
            private uint m_z;
            private uint m_w;

            public Rng(uint z = 362436069, uint w = 521288629)
            {
                m_z = z;
                m_w = w;
            }

            public double NextDouble()
            {
                var i = NextUInt32();
                return i * 2.328306e-10f;
            }

            private uint NextUInt32()
            {
                m_z = 36969 * (m_z & 65535) + (m_z >> 16);
                m_w = 18000 * (m_w & 65535) + (m_w >> 16);
                return (m_z << 16) + m_w;
            }
        }

        private static Ray MakeCameraRay(double fieldOfViewInDegrees, Point origin, Vec3 target, Vec3 targetUpDirection,
            double xScreenPos0To1, double yScreenPos0To1)
        {
            var forward = (target - origin).Normalize();
            var right = Vec3.Cross(forward, targetUpDirection).Normalize();
            var up = Vec3.Cross(right, forward).Normalize();

            var tanFov = Math.Tan(fieldOfViewInDegrees * Math.PI / 180.0f);

            var ray = new Ray
            {
                m_origin = origin,
                m_direction = forward +
                              right * ((xScreenPos0To1 - 0.5f) * tanFov) +
                              up * ((yScreenPos0To1 - 0.5f) * tanFov)
            };

            ray.m_direction = ray.m_direction.Normalize();

            return ray;
        }

        private Color Trace(Ray ray, ShapeSet scene, List<Shape> lights, Rng rng)
        {
            var result = new Color();

            var i = new Intersection(ref ray);

            if (!scene.Intersect(ref i))
            {
                return result;
            }

            result += i.m_pMaterial.Emittance();

            var position = i.Position();
            foreach (var light in lights)
            {
                var lightResult = new Color();
                for (var lsv = 0; lsv < Shared.lightSamplesV; ++lsv)
                {
                    for (var lsu = 0; lsu < Shared.lightSamplesU; ++lsu)
                    {
                        var lightPoint = new Point();
                        var lightNormal = new Vec3();
                        var pLightShape = light;

                        // var testDouble = (lsu + rng.NextDouble()) / (double) Shared.lightSamplesU;

                        pLightShape.SampleSurface((lsu + rng.NextDouble()) / Shared.lightSamplesU, 
                                                  (lsv + rng.NextDouble()) / Shared.lightSamplesV, 
                                                   position, ref lightPoint, ref lightNormal);

                        var toLight = lightPoint - position;
                        var lightDistance = toLight.Len();
                        toLight = toLight.Normalize();
                        var shadowRay = new Ray(true, position, toLight, lightDistance);
                        var shadowIntersection = new Intersection(ref shadowRay);
                        var intersected = scene.Intersect(ref shadowIntersection);

                        if ( !intersected || shadowIntersection.m_pShape == pLightShape )
                        {
                            lightResult += pLightShape.Emitted() * i.m_colorModifier *
                                           i.m_pMaterial.Shade(position, i.m_normal, ray.m_direction, toLight) * (1.0f / (lightDistance * lightDistance));
                        }
                    }
                }

                lightResult = lightResult.DivValue(Shared.lightSamplesU * Shared.lightSamplesV);

                result += lightResult;
            }

            return result;
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            // Available materials:
            var blueishLambert = new Lambert(new Color(0.9f, 0.9f, 1.0f));
            var purplishLambert = new Lambert(new Color(0.9f, 0.7f, 0.8f));
            var greenishLambert = new Lambert(new Color(0.7f, 0.9f, 0.7f));
            var greenishPhong = new Phong(new Color(0.7f, 0.9f, 0.7f), 16.0f);

            Shared.imageWidth = Convert.ToUInt32(FLD_width.Text);
            Shared.imageHeight = Convert.ToUInt32(FLD_height.Text);
            Shared.pixelSamplesU = Convert.ToUInt32(FLD_pixelSamples.Text);
            Shared.pixelSamplesV = Convert.ToUInt32(FLD_pixelSamples.Text);
            Shared.lightSamplesU = Convert.ToUInt32(FLD_lightSamples.Text);
            Shared.lightSamplesV = Convert.ToUInt32(FLD_lightSamples.Text);

            var masterSet = new ShapeSet();

            var plane = new Plane(new Point(0.0f, -2.0f, 0.0f),
                                  new Vec3(0.0f, 1.0f, 0.0f),
                                  blueishLambert,
                                  true);
            masterSet.AddShape(plane);

            var sphere1 = new Sphere(new Point(3.0f, -1.0f, 0.0f),
                                     1.0f, purplishLambert);
            masterSet.AddShape(sphere1);

            var sphere2 = new Sphere(new Point(-3.0f, 0.0f, -2.0f),
                2.0f, greenishLambert);
            masterSet.AddShape(sphere2);

            var pointLamp = new PointLamp(new Vec3(0.0f, 15.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f), 300.0f);
            masterSet.AddShape(pointLamp);

            //var areaLightSize = 5.0f;
            //var areaLight = new RectangleLight(new Point(-(areaLightSize / 2), 4.0f, -(areaLightSize / 2)),
            //                                   new Vec3(areaLightSize, 0.0f, 0.0f),
            //                                   new Vec3(0.0f, 0.0f, areaLightSize),
            //                                   new Color(1.0f, 1.0f, 1.0f),
            //                                   1.0f);
            //masterSet.AddShape(areaLight);

            //var smallAreaLightSize = 3.0f;
            //var smallAreaLight = new RectangleLight(new Point(-(smallAreaLightSize / 2), -1.0f, -(smallAreaLightSize / 2)),
            //                                        new Vec3(smallAreaLightSize, 0.0f, 0.0f),
            //                                        new Vec3(0.0f, 0.0f, smallAreaLightSize),
            //                                        new Color(1.0f, 1.0f, 0.5f),
            //                                        0.75f);

            //masterSet.AddShape(smallAreaLight);

            //var sphereForLight = new Sphere(new Point(0.0f, 0.0f, 2.0f),
            //                                1.0f, blueishLambert);
            //var sphereLight = new ShapeLight(sphereForLight, new Color(1.0f, 1.0f, 0.1f), 4.0f);
            //masterSet.AddShape(sphereLight);

            var lights = new List<Shape>();
            masterSet.FindLights(lights);

            var rng = new Rng();

            using (var sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"/" + FLD_fileName.Text))
            {
                sw.WriteLine("P3");
                sw.WriteLine(Shared.imageWidth + " " + Shared.imageHeight);
                sw.WriteLine("255");

                for (var y = 0; y < Shared.imageHeight; ++y)
                {
                    sw.WriteLine("");

                    for (var x = 0; x < Shared.imageWidth; ++x)
                    {
                        var pixelColor = new Color();

                        if (Shared.useParallel)
                        {
                            //Parallel.For(0, samples, i =>
                            //{
                            //    var yu = 1.0f - ((y + rng.NextDouble()) / (imageHeight - 1));
                            //    var xu = (x + rng.NextDouble()) / (imageWidth - 1);

                            //    var ray = makeCameraRay(45.0f,
                            //        new Point(0.0f, 5.0f, 15.0f),
                            //        new Point(0.0f, 0.0f, 0.0f),
                            //        new Point(0.0f, 1.0f, 0.0f),
                            //        xu,
                            //        yu);

                            //    var intersection = new Intersection(ref ray);

                            //    if ( masterSet.Intersect(ref intersection) )
                            //    {
                            //        pixelColor += intersection.m_emitted;
                            //        var position = intersection.Position();
                            //        foreach ( var light in lights )
                            //        {
                            //            var lightPoint = new Point();
                            //            var lightNormal = new Vec3();
                            //            var pLightShape = light;

                            //            pLightShape.SampleSurface(rng.NextDouble(), rng.NextDouble(), position, ref lightPoint,
                            //                ref lightNormal);

                            //            var toLight = lightPoint - position;
                            //            var lightDistance = toLight.Len();
                            //            toLight = toLight.Normalize();
                            //            var shadowRay = new Ray(position, toLight, lightDistance);
                            //            var shadowIntersection = new Intersection(ref shadowRay);
                            //            var intersected = masterSet.Intersect(ref shadowIntersection);

                            //            if ( !intersected || shadowIntersection.m_pShape == pLightShape )
                            //            {
                            //                var lightAttenuation = 1.0f / (lightDistance * lightDistance);
                            //                //var lightAttenuation = Math.Max(0.0f, Vec3.Dot(intersection.m_normal, toLight));
                            //                pixelColor += intersection.m_color * pLightShape.Emitted() * lightAttenuation;
                            //            }
                            //        }
                            //    }
                            //});
                        }
                        else
                        {
                            for ( var vsi = 0; vsi < Shared.pixelSamplesV; ++vsi )
                            {
                                for (var usi = 0; usi < Shared.pixelSamplesU; ++usi)
                                {
                                    var yu = 1.0f - (y + (vsi + rng.NextDouble()) / Shared.pixelSamplesV) / Shared.imageHeight;
                                    var xu = (x + (usi + rng.NextDouble()) / Shared.pixelSamplesU) / Shared.imageWidth;

                                    var ray = MakeCameraRay(45.0f,
                                        new Point(0.0f, 5.0f, 15.0f),
                                        new Point(0.0f, 0.0f, 0.0f),
                                        new Point(0.0f, 1.0f, 0.0f),
                                        xu,
                                        yu);
                                    pixelColor += Trace(ray, masterSet, lights, rng);
                                }
                            }
                            pixelColor.DivValue(Shared.pixelSamplesV * Shared.pixelSamplesU);
                            pixelColor.Clamp();

                            var r = (int)(pixelColor.m_r * 255.0f);
                            var g = (int)(pixelColor.m_g * 255.0f);
                            var b = (int)(pixelColor.m_b * 255.0f);

                            sw.Write(r + " " + g + " " + b + " ");
                        }
                    }
                }
            }
            
            
        }
    }
}
