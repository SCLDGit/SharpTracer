namespace SharpTracer
{
    class Shared
    {
        public const double kRayTMin = 0.00001f;
        public const double kRayTMax = 1.0e30f;
        public static bool useParallel = false;

        public static uint imageHeight = 0;
        public static uint imageWidth = 0;
        public static uint pixelSamplesU = 0;
        public static uint pixelSamplesV = 0;
        public static uint lightSamplesU = 0;
        public static uint lightSamplesV = 0;
    }
}
