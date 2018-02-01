using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Point = SharpTracer.Vec3;

namespace SharpTracer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

            public uint NextUInt32()
            {
                m_z = 36969 * (m_z & 65535) + (m_z >> 16);
                m_w = 18000 * (m_w & 65535) + (m_w >> 16);
                return (m_z << 16) + m_w;
            }
        }

        Ray makeCameraRay(double fieldOfViewInDegrees, Point origin, Vec3 target, Vec3 targetUpDirection,
            double xScreenPos0to1, double yScreenPos0to1)
        {
            var forward = (target - origin).Normalize();
            var right = Vec3.Cross(forward, targetUpDirection).Normalize();
            var up = Vec3.Cross(right, forward).Normalize();

            var tanFov = Math.Tan(fieldOfViewInDegrees * Math.PI / 180.0f);

            var ray = new Ray
            {
                m_origin = origin,
                m_direction = forward +
                              right * ((xScreenPos0to1 - 0.5f) * tanFov) +
                              up * ((yScreenPos0to1 - 0.5f) * tanFov)
            };

            ray.m_direction = ray.m_direction.Normalize();

            return ray;
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            Shared.useParallel = CHK_useParallel.IsChecked.Value;
            var imageWidth = Convert.ToInt64(FLD_width.Text);
            var imageHeight = Convert.ToInt64(FLD_height.Text);
            var samples = Convert.ToInt64(FLD_samples.Text); 
            var masterSet = new ShapeSet();

            var plane = new Plane(new Point(0.0f, -2.0f, 0.0f),
                                  new Vec3(0.0f, 1.0f, 0.0f),
                                  new Color(0.8f, 0.8f, 0.8f),
                                  true);
            masterSet.AddShape(plane);

            var areaLightSize = 5.0f;
            var areaLight = new RectangleLight(new Point(-(areaLightSize / 2), 2.0f, -(areaLightSize / 2)),
                                               new Vec3(areaLightSize, 0.0f, 0.0f),
                                               new Vec3(0.0f, 0.0f, areaLightSize),
                                               new Color(1.0f, 0.5f, 1.0f),
                                               150.0f);
            masterSet.AddShape(areaLight);

            var smallAreaLightSize = 3.0f;
            var smallAreaLight = new RectangleLight(new Point(-(smallAreaLightSize / 2), -1.0f, -(smallAreaLightSize / 2)),
                                                    new Vec3(smallAreaLightSize, 0.0f, 0.0f),
                                                    new Vec3(0.0f, 0.0f, smallAreaLightSize),
                                                    new Color(1.0f, 1.0f, 0.5f),
                                                    0.75f);

            masterSet.AddShape(smallAreaLight);

            var lights = new List<Shape>();
            masterSet.FindLights(lights);

            var rng = new Rng();

            using (var sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"/" + FLD_fileName.Text))
            {
                sw.WriteLine("P3");
                sw.WriteLine(imageWidth + " " + imageHeight);
                sw.WriteLine("255");

                for (var y = 0; y < imageHeight; ++y)
                {
                    sw.WriteLine("");

                    for (var x = 0; x < imageWidth; ++x)
                    {
                        var pixelColor = new Color();

                        if (Shared.useParallel)
                        {
                            Parallel.For(0, samples, i =>
                            {
                                var yu = 1.0f - ((y + rng.NextDouble()) / (imageHeight - 1));
                                var xu = (x + rng.NextDouble()) / (imageWidth - 1);

                                var ray = makeCameraRay(45.0f,
                                    new Point(0.0f, 5.0f, 15.0f),
                                    new Point(0.0f, 0.0f, 0.0f),
                                    new Point(0.0f, 1.0f, 0.0f),
                                    xu,
                                    yu);

                                var intersection = new Intersection(ref ray);

                                if ( masterSet.Intersect(ref intersection) )
                                {
                                    pixelColor += intersection.m_emitted;
                                    var position = intersection.Position();
                                    foreach ( var light in lights )
                                    {
                                        var lightPoint = new Point();
                                        var lightNormal = new Vec3();
                                        var pLightShape = light;

                                        pLightShape.SampleSurface(rng.NextDouble(), rng.NextDouble(), position, ref lightPoint,
                                            ref lightNormal);

                                        var toLight = lightPoint - position;
                                        var lightDistance = toLight.Len();
                                        toLight = toLight.Normalize();
                                        var shadowRay = new Ray(position, toLight, lightDistance);
                                        var shadowIntersection = new Intersection(ref shadowRay);
                                        var intersected = masterSet.Intersect(ref shadowIntersection);

                                        if ( !intersected || shadowIntersection.m_pShape == pLightShape )
                                        {
                                            var lightAttenuation = 1.0f / (lightDistance * lightDistance);
                                            //var lightAttenuation = Math.Max(0.0f, Vec3.Dot(intersection.m_normal, toLight));
                                            pixelColor += intersection.m_color * pLightShape.Emitted() * lightAttenuation;
                                        }
                                    }
                                }
                            });
                        }
                        else
                        {
                            for ( var si = 0; si < samples; ++si )
                            {
                                var yu = 1.0f - ((y + rng.NextDouble()) / (imageHeight - 1));
                                var xu = (x + rng.NextDouble()) / (imageWidth - 1);

                                var ray = makeCameraRay(45.0f,
                                    new Point(0.0f, 5.0f, 15.0f),
                                    new Point(0.0f, 0.0f, 0.0f),
                                    new Point(0.0f, 1.0f, 0.0f),
                                    xu,
                                    yu);

                                var intersection = new Intersection(ref ray);

                                if ( masterSet.Intersect(ref intersection) )
                                {
                                    pixelColor += intersection.m_emitted;
                                    var position = intersection.Position();
                                    foreach ( var light in lights )
                                    {
                                        var lightPoint = new Point();
                                        var lightNormal = new Vec3();
                                        var pLightShape = light;

                                        pLightShape.SampleSurface(rng.NextDouble(), rng.NextDouble(), position, ref lightPoint,
                                            ref lightNormal);

                                        var toLight = lightPoint - position;
                                        var lightDistance = toLight.Len();
                                        toLight = toLight.Normalize();
                                        var shadowRay = new Ray(position, toLight, lightDistance);
                                        var shadowIntersection = new Intersection(ref shadowRay);
                                        var intersected = masterSet.Intersect(ref shadowIntersection);

                                        if ( !intersected || shadowIntersection.m_pShape == pLightShape )
                                        {
                                            var lightAttenuation = 1.0f / (lightDistance * lightDistance);
                                            //var lightAttenuation = Math.Max(0.0f, Vec3.Dot(intersection.m_normal, toLight));
                                            pixelColor += intersection.m_color * pLightShape.Emitted() * lightAttenuation;
                                        }
                                    }
                                }
                            }
                        }

                        pixelColor.DivValue(samples);
                        pixelColor.Clamp();

                        int r, g, b;

                        r = (int)(pixelColor.m_r * 255.0f);
                        g = (int)(pixelColor.m_g * 255.0f);
                        b = (int)(pixelColor.m_b * 255.0f);

                        sw.Write(r + " "  + g + " " + b + " ");
                    }
                }
            }
            
            
        }
    }
}
