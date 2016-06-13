using System;

namespace MzLite.Processing
{
    public static class DecimalHelper
    {

        public static double Add(double x, double y)
        {
            decimal dx = new decimal(x);
            decimal dy = new decimal(y);
            return decimal.ToDouble(decimal.Add(dx, dy));
        }

        public static double Subtract(double x, double y)
        {
            decimal dx = new decimal(x);
            decimal dy = new decimal(y);
            return decimal.ToDouble(decimal.Subtract(dx, dy));
        }

        public static double AbsDiff(double x, double y)
        {
            return Math.Abs(Subtract(x, y));
        }

    }
}
