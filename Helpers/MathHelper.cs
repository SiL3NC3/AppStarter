using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppStarter.Helpers
{
    public static class MathHelper
    {
        public static int GetPercent(double current, double maximum)
        {
            double num = maximum - current;
            return (int)Convert.ToInt16(current / maximum * 100.0);
        }
    }
}
