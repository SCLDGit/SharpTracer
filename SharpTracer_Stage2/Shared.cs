using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTracer
{
    class Shared
    {
        public const double kRayTMin = 0.00001f;
        public const double kRayTMax = 1.0e30f;
        public static bool useParallel = true;
    }
}
