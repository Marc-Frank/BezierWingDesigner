using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BezierAirfoilDesigner
{
    public class PointD : IEquatable<PointD>
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double Z { get; set; }

        public PointD(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public PointD()
        {
        }

        public bool Equals(PointD other)
        {
            if (other == null)
                return false;

            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals((PointD)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
    }
}
