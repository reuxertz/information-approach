using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;


namespace EvolutionTools
{
    using DPoint = System.Drawing.Point;
    using DColor = System.Drawing.Color;

    //Constants
    public static class Science
    {
        public const double GasR = 8.314621;        
    }
    public struct Stats
    {
        //Functions
        public static Point[] Distribution(double[] vals, double splitWidth)
        {
            var dist = new List<Point>();

            for (int i = 0; i < vals.Length; i++)
            {
                var nv = ((int)(vals[i] / splitWidth)) * splitWidth;

                var match = false;
                for (int j = 0; j < dist.Count; j++)
                {
                    if (dist[j].X == nv)
                    {
                        dist[j] = new Point(dist[j].X, dist[j].Y + 1);
                        match = true;
                        break;
                    }
                }
                if (!match)
                    dist.Add(new Point(nv, 1.0));
            }

            //Normlaize
            for (int i = 0; i < dist.Count; i++)
                dist[i] = new Point(dist[i].X, dist[i].Y / vals.Length);

            return dist.ToArray<Point>();
        }
        public static double Sum(double[] vals)
        {
            var r = 0.0;

            for (int i = 0; i < vals.Length; i++)
                r += vals[i];

            return r;
        }
        public static double Mean(double[] vals)
        {
            var r = 0.0;

            for (int i = 0; i < vals.Length; i++)
                r += vals[i];

            return r / vals.Length;
        }
        public static double StdDev(double[] vals, double mean)
        {
            var r = 0.0;

            for (int i = 0; i < vals.Length; i++)
            {
                var d = vals[i];
                if (mean != 0.0)
                    d = vals[i] - mean;
                r += d * d;
            }

            return Math.Sqrt(r / vals.Length);
        }
        public static Point[] StdDevError(double[] trainingVals, double[] vals)
        {
            var trainingValueCount = new List<int>();
            var trainingValueTypes = new List<double>();

            //Create ValueTypes
            for (int i = 0; i < trainingVals.Length; i++)
            {
                var indexOf = trainingValueTypes.IndexOf(trainingVals[i]);
                if (indexOf == -1)
                {
                    trainingValueTypes.Add(trainingVals[i]);
                    trainingValueCount.Add(1);
                    continue;
                }
                else
                {
                    trainingValueCount[indexOf] = trainingValueCount[indexOf] + 1;
                }
            }

            //Add difference squred of Each ValueType
            var pow = 2.0;
            var invPow = 1.0 / pow;
            var tvSums = new double[trainingValueTypes.Count];
            for (int i = 0; i < trainingVals.Length; i++)
            {
                var dif = vals[i] - trainingVals[i];
                tvSums[trainingValueTypes.IndexOf(trainingVals[i])] += Math.Pow(dif, pow);// *dif;
            }
            
            //Take sqrt of Sums with respect to valuetype
            var np = new Point[tvSums.Length];
            for (int i = 0; i < tvSums.Length; i++)
                np[i] = new Point(trainingValueTypes[i], Math.Pow((tvSums[i] / trainingValueCount[i]), invPow));

            return np;
        }
        public static Point[] EnergyResolutions(double[] stdDevs, double[] energyValues, bool ignoreResDivision)
        {
            var rp = new Point[stdDevs.Length];
            for (int i = 0; i < stdDevs.Length; i++)
                if (ignoreResDivision)
                    rp[i] = new Point(energyValues[i], stdDevs[i]);
                else
                    rp[i] = new Point(energyValues[i], stdDevs[i] / energyValues[i]);

            return rp;
        }
        public static Point[] EnergyResolutions(double[] stdDevs, double[] energyValues)
        {
            return Stats.EnergyResolutions(stdDevs, energyValues, false);
        }
        public static Point[] AverageEnergyResolutions(double[] stdDevs, double[] energyValues)
        {
            var enVals = energyValues.ToList<double>();
            var energyRess = Stats.EnergyResolutions(stdDevs, energyValues);

            //Create average
            Point[] rpts = new Point[energyValues.Length];
            double[] rptsCounter = new double[energyValues.Length];
            
            //Create return point array
            for (int i = 0; i < rpts.Length; i++)
                rpts[i] = new Point(energyValues[i], 0);

            //Sort and Add
            for (int i = 0; i < energyRess.Length; i++)
            {
                for (int j = 0; j < rpts.Length; j++)
                {
                    if (rpts[j].X == energyRess[i].X)
                    {
                        rpts[j].Y += energyRess[i].Y;
                        rptsCounter[j]++;
                    }
                }
            }

            //Normalize to counter
            for (int i = 0; i < rpts.Length; i++)
                rpts[i].Y /= rptsCounter[i];

            return rpts;
        }
        public static double[] Resolutions(double[] stdDevs, double[] energyValues)
        {
            var rp = new double[stdDevs.Length];
            for (int i = 0; i < stdDevs.Length; i++)
                rp[i] = stdDevs[i] / energyValues[i];

            return rp;
        }

        public double MySum, MyMean, MyStdDev, MyTestMean, MyTestStdDevError;

        public Stats(double[] vals, double TestMean)
        {
            var rs = 0.0;
            for (int i = 0; i < vals.Length; i++)
                rs += vals[i];

            var rm = rs / vals.Length;

            var rd = 0.0;
            var rdt = 0.0;
            for (int i = 0; i < vals.Length; i++)
            {
                var dm = vals[i] - rm;
                var dt = vals[i] - TestMean;

                rd += dm * dm;
                rdt += dt * dt;
            }

            rd = Math.Sqrt(rd / vals.Length);
            rdt = Math.Sqrt(rdt / vals.Length);

            this.MySum = rs;
            this.MyMean = rm;
            this.MyStdDev = rd;

            this.MyTestMean = TestMean;
            this.MyTestStdDevError = rdt;
        }
    }

    //Class
    public class PointN
    {
        private int _dim = 0;
        private double[] _values;

        public double[] GetVectorN()
        {
            return (double[])this._values.Clone();
        }

        public PointN(double[] values)
        {
            this._dim = values.Length;
            values.CopyTo(this._values, 0);
        }
        public PointN(int dimensions)
        {
            this._dim = dimensions;
            this._values = new double[dimensions];
        }

        //Functions
        public double[] Add(double[] v)
        {
            var r = new double[this._dim];

            for (int i = 0; i < this._dim; i++)
                r[i] = this._values[i] + v[i];

            return r;
        }
        public double[] Subtract(double[] v)
        {
            var r = new double[this._dim];

            for (int i = 0; i < this._dim; i++)
                r[i] = this._values[i] - v[i];

            return r;
        }
        public double DistanceSQRD(double[] v)
        {
            var r = this.Subtract(v);
            double sum = 0;

            for (int i = 0; i < r.Length; i++)
                sum += r[i] * r[i];

            return sum;
        }
    }

    //Structs
    public struct Point
    {
        public double X, Y;

        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public Point(DPoint dpt)
        {
            this.X = dpt.X;
            this.Y = dpt.Y;
        }
        public Point(Size s)
        {
            this.X = s.Width;
            this.Y = s.Height;
        }
        public Point(double[] d)
        {
            if (d.Length != 2)
                throw new Exception("Unhandled");

            this.X = d[0];
            this.Y = d[1];
        }
        //Creators
        public System.Drawing.Point ToDrawPoint()
        {
            return new System.Drawing.Point((int)this.X, (int)this.Y);
        }

        //Methods
        public override string ToString()
        {
            return this.X + ", " + this.Y;
        }
        public override bool Equals(object obj)
        {
            Point pt = (Point)obj;

            return this.X == pt.X && this.Y == pt.Y;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }
        public bool IsEqual(Point p, int round)
        {
            if (round >= 0)
            {
                Point p1 = this.Round(round);
                Point p2 = p.Round(round);
                return p1.X == p2.X && p1.Y == p2.Y;
            }

            return this.X == p.X && this.Y == p.Y;
        }
        public bool IsEqual(Point p)
        {
            return this.X == p.X && this.Y == p.Y;
        }
        public Point Add(Point p)
        {
            return new Point(this.X + p.X, this.Y + p.Y);
        }
        public Point Multiply(Point p)
        {
            return new Point(this.X * p.X, this.Y * p.Y);
        }
        public Point Multiply(double m)
        {
            return new Point(this.X * m, this.Y * m);
        }
        public Point Subtract(Point p)
        {
            return new Point(this.X - p.X, this.Y - p.Y);
        }
        public Point Divide(Point p)
        {
            return new Point(this.X / p.X, this.Y / p.Y);
        }
        public Point Divide(double d)
        {
            return new Point(this.X / d, this.Y / d);
        }
        public Point Mod(Point p)
        {
            Point rpt = new Point(this.X % p.X, this.Y % p.Y);

            if (rpt.X < 0)
                rpt.X += p.X;

            if (rpt.Y < 0)
                rpt.Y += p.Y;

            return rpt;
        }
        public Point Round(int round)
        {
            Point rpt = new Point(Math.Round(this.X, round), Math.Round(this.Y, round));

            return rpt;
        }
        public bool IsWithin(Point p1, Point p2)
        {
            //Adjust Buffer

            //Return
            return (
                ((this.X > p1.X && this.X < p2.X) || (this.X > p2.X && this.X < p1.X))
                &&
                ((this.Y > p1.Y && this.Y < p2.Y) || (this.Y > p2.Y && this.Y < p1.Y))
                );
        }
        public bool IsWithin(Point p1, Point p2, double buffer)
        {
            //Adjust Buffer

            //Return
            return (
                ((this.X > p1.X + buffer && this.X < p2.X - buffer) || (this.X > p2.X + buffer && this.X < p1.X - buffer))                
                &&            
                ((this.Y > p1.Y + buffer && this.Y < p2.Y - buffer) || (this.Y > p2.Y + buffer && this.Y < p1.Y - buffer))
                );
        }
        public bool IsWithin(Point p1, Point p2, double buffer, bool greatThanEqual, bool lessThanEqual)
        {
            return (
                ((!greatThanEqual && this.X > p1.X + buffer) || (greatThanEqual && this.X >= p1.X + buffer) && 
                ((!lessThanEqual && this.X < p2.X - buffer) || (lessThanEqual && this.X <= p2.X - buffer)))
                ||
                ((!greatThanEqual && this.X > p2.X + buffer) || (greatThanEqual && this.X >= p2.X + buffer) &&
                ((!lessThanEqual && this.X < p1.X - buffer) || (lessThanEqual && this.X <= p1.X - buffer)))
                )                
                &&
                (
                //(this.Y > p1.Y + buffer && this.Y < p2.Y - buffer) || (this.Y > p2.Y + buffer && this.Y < p1.Y - buffer)
                ((!greatThanEqual && this.Y > p1.Y + buffer) || (greatThanEqual && this.Y >= p1.Y + buffer) &&
                ((!lessThanEqual && this.Y < p2.Y - buffer) || (lessThanEqual && this.Y <= p2.Y - buffer)))
                ||
                ((!greatThanEqual && this.Y > p2.Y + buffer) || (greatThanEqual && this.Y >= p2.Y + buffer) &&
                ((!lessThanEqual && this.Y < p1.Y - buffer) || (lessThanEqual && this.Y <= p1.Y - buffer)))
                );
        }
        public double Distance(Point p)
        {
            return Math.Sqrt(((p.X - this.X) * (p.X - this.X)) + ((p.Y - this.Y) * (p.Y - this.Y)));
        }
        public double DistanceSqrd(Point p)
        {
            return ((p.X - this.X) * (p.X - this.X)) + ((p.Y - this.Y) * (p.Y - this.Y));
        }
        public double DistanceSqrd(double x, double y)
        {
            return ((x - this.X) * (x - this.X)) + ((y - this.Y) * (y - this.Y));
        }

        public static Point Center(Point bounds, Point size)
        {
            var x = (bounds.X / 2.0) - (size.X / 2.0);
            var y = (bounds.Y / 2.0) - (size.Y / 2.0);

            return new Point(x, y);
        }
    }
    public struct Angle
    {
        public double Degree;

        public double Radians
        {
            get
            {
                return (this.Degree / 180.0) * Math.PI;
            }
            set
            {
                this.Degree = (value / Math.PI) * 180.0;
            }
        }

        public Angle(double degree)
        {
            this.Degree = degree;
        }



    }
    public struct Color
    {
        public int MyRed, MyGreen, MyBlue;
        public int Limit;

        public void SetRed(int Red)
        {
            this.MyRed = Red;

            if (this.MyRed > this.Limit)
                this.MyRed = (int)this.Limit;

            if (this.MyRed < 0)
                this.MyRed = 0;
        }
        public void SetGreen(int Green)
        {
            this.MyGreen = Green;

            if (this.MyGreen > this.Limit)
                this.MyGreen = (int)this.Limit;

            if (this.MyGreen < 0)
                this.MyGreen = 0;
        }
        public void SetBlue(int Blue)
        {
            this.MyBlue = Blue;

            if (this.MyBlue > this.Limit)
                this.MyBlue = (int)this.Limit;

            if (this.MyBlue < 0)
                this.MyBlue = 0;
        }

        public static Color White
        {
            get
            {
                return new Color(255, 255, 255);
            }
        }
        public static Color Red
        {
            get
            {
                return new Color(255, 0, 0);
            }
        }
        public static Color Green
        {
            get
            {
                return new Color(0, 255, 0);
            }
        }
        public static Color Blue
        {
            get
            {
                return new Color(0, 0, 255);
            }
        }

        public double RedScaled
        {
            get
            {
                return this.MyRed / (1.0 * Limit);
            }
        }
        public double GreenScaled
        {
            get
            {
                return this.MyGreen / (1.0 * Limit);
            }
        }
        public double BlueScaled
        {
            get
            {
                return this.MyBlue / (1.0 * Limit);
            }
        }

        public double[] ToNormArray()
        {
            return new double[3] {
                this.MyRed / this.Limit,                
                this.MyGreen / this.Limit,                
                this.MyBlue / this.Limit};
        }
        public Color ToInverse()
        {
            return new Color((int)(this.Limit - this.MyRed), (int)(this.Limit - this.MyGreen), (int)(this.Limit - this.MyBlue), (int)this.Limit);

        }
        public Color(int r, int g, int b, int l)
        {
            this.MyRed = r;
            this.MyGreen = g;
            this.MyBlue = b;
            this.Limit = l;
        }
        public Color(int r, int g, int b)
        {
            this.MyRed = r;
            this.MyGreen = g;
            this.MyBlue = b;
            this.Limit = 255;
        }
        public Color(System.Drawing.Color c)
        {
            this.MyRed = c.R;
            this.MyGreen = c.G;
            this.MyBlue = c.B;
            this.Limit = 255;


        }
        public Color(Color c)
        {
            this.MyRed = c.MyRed;
            this.MyBlue = c.MyBlue;
            this.MyGreen = c.MyGreen;
            this.Limit = c.Limit;
        }
        public static Color FromDouble(double d)
        {
            d = d * 5;

            if (d > 5 || d < 0)
                throw new Exception("Unhanlded parameter value");

            //Return Black - Red
            if (d <= 1)
                return new Color((int)(d * 255), 0, 0);
            d--;

            //Return Red - Green
            if (d <= 1)
                return new Color((int)((1 - d) * 255), (int)(d * 255), 0);
            d--;

            //Return Green - Blue
            if (d <= 1)
                return new Color(0, (int)((1- d) * 255),(int)(d * 255));
            d--;
            
            //Return Blue - Red
            if (d <= 1)
                return new Color((int)(d * 255), 0, 255);
            d--;

            //Return white
            if (d <= 1)
                return new Color(255, (int)(d * 255), 255);
            d--;

            throw new Exception("unhandled region");
            return Color.White;

        }
        public static Color FromDoubleCircular(double d)
        {
            d -= ((int)d) * 4;
            var r = 0.0;
            var g = 0.0;
            var b = 0.0;

            //REd
            if (d >= 2 && d < 3)
                r = d - 3;
            if ((d >= 0 && d < 1) || (d >= 3 && d < 4))
                r = 1;            
            if (d >= 1 && d < 2)
                r = 2 - d;

            //Green
            if (d >= 0 && d < 1)
                g = d;
            if (d >= 1 && d < 2)
                g = 1;
            if (d >= 2 && d < 3)
                g = 3 - d;

            //Blue
            if (d >= 1 && d < 2)
                b = d - 1;
            if (d >= 2 && d < 3)
                b = 1;
            if (d >= 3 && d < 4)
                b = 4 - d;

            var c = new Color((int)(r * 255), (int)(g * 255), (int)(b * 255));
            c.Limit = 255;

            return c;
        }
        public Color Multiply(double m)
        {
            return new Color((int)(this.MyRed * m), (int)(this.MyGreen * m), (int)(this.MyBlue * m));
        }
        public Color RawAdd(Color c)
        {
            return new Color(this.MyRed + c.MyRed, this.MyGreen + c.MyGreen, this.MyBlue + c.MyBlue, (int)Math.Max(this.Limit, c.Limit));
        }
    }

    //Structures
    public struct Line
    {
        public Point P1, P2;

        public bool Horizontal
        {
            get
            {
                return (P1.X == P2.X);
            }
        }

        public Line(Point p1, Point p2)
        {
            this.P1 = p1;
            this.P2 = p2;

            this = this.OrderPosX();
        }

        public Line OrderPosX()
        {
            if (this.P2.X < this.P1.X)
                return new Line(this.P2, this.P1);

            return this;
        }

        public Point? RealCrossPoint(Line l)
        {
            var t = this;

            t = t.OrderPosX();
            l = l.OrderPosX();

            //Handle horizont Line
            if (l.Horizontal && !t.Horizontal && t.P1.X <= l.P1.X && t.P2.X >= l.P1.X)
            {
                var cy = t.Slope() * l.P1.X + t.Intercept();
                if ((cy <= P1.Y && cy >= P2.Y) || (cy >= P1.Y && cy <= P2.Y))
                    return new Point(l.P1.X, cy);
            }
            //Handle horizontal this
            if (t.Horizontal && !l.Horizontal && l.P1.X <= t.P1.X && l.P2.X >= t.P1.X)
            {
                var cy = l.Slope() * t.P1.X + l.Intercept();
                if ((cy <= P1.Y && cy >= P2.Y) || (cy >= P1.Y && cy <= P2.Y))
                    return new Point(t.P1.X, cy);
            }


            var dx = t.P2.X - t.P1.X;

            if (dx == 0)
                return null;

            var mt = (t.P2.Y - t.P1.Y) / dx;
            var ml = (l.P2.Y - l.P1.Y) / (l.P2.X - l.P1.X);

            var bt = t.P1.Y - (mt * t.P1.X);
            var bl = l.P1.Y - (ml * l.P1.X);

            var xc = (bl - bt) / (mt - ml);
            var cp = new Point(xc, (mt * xc) + bt);

            if (xc >= t.P1.X && xc <= t.P2.X && xc >= l.P1.X && xc <= l.P2.X)
                return cp;

            return null;
        }
        public Point? RealCrossPoint(Line[] ls, out Line? line)
        {
            foreach (Line l in ls)
            {
                var cp = this.RealCrossPoint(l);
                if (cp != null)
                {
                    line = l;
                    return cp;
                }
            }
            line = null;
            return null;
        }
        public double Slope()
        {
            var t = this.OrderPosX();
            var dx = t.P2.X - t.P1.X;
            var mt = (t.P2.Y - t.P1.Y) / dx;
            return mt;
        }
        public double Intercept()
        {
            var t = this.OrderPosX();
            var mt = t.Slope();
            var bt = t.P1.Y - (mt * t.P1.X);
            return bt;
        }

    }


}
