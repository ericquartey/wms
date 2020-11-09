/********************************************************************************************
                                  Class LstSquQuadRegr
             A C#  Class for Least Squares Regression for Quadratic Curve Fitting
                                  Alex Etchells  2010
 ********************************************************************************************/

using System;
using System.Collections;

// https://www.codeproject.com/Articles/63170/Least-Squares-Regression-for-Quadratic-Curve-Fitti

namespace Ferretto.VW.Utils.Maths
{
    public class LstSquQuadRegr
    {
        /* instance variables */

        #region Fields

        private int numOfEntries;

        private ArrayList pointArray = new ArrayList();

        private double[] pointpair;

        #endregion

        /*constructor */

        #region Constructors

        public LstSquQuadRegr()
        {
            this.numOfEntries = 0;
            this.pointpair = new double[2];
        }

        #endregion

        /*instance methods */

        #region Methods

        /// <summary>
        /// add point pairs
        /// </summary>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        public void AddPoints(double x, double y)
        {
            this.pointpair = new double[2];
            this.numOfEntries += 1;
            this.pointpair[0] = x;
            this.pointpair[1] = y;
            this.pointArray.Add(this.pointpair);
        }

        /// <summary>
        /// returns the a term of the equation ax^2 + bx + c
        /// </summary>
        /// <returns>a term</returns>
        public double aTerm()
        {
            if (this.numOfEntries < 3)
            {
                throw new InvalidOperationException("Insufficient pairs of co-ordinates");
            }
            //notation sjk to mean the sum of x_i^j*y_i^k.
            double s40 = this.getSx4(); //sum of x^4
            double s30 = this.getSx3(); //sum of x^3
            double s20 = this.getSx2(); //sum of x^2
            double s10 = this.getSx();  //sum of x
            double s00 = this.numOfEntries;  //sum of x^0 * y^0  ie 1 * number of entries

            double s21 = this.getSx2y(); //sum of x^2*y
            double s11 = this.getSxy();  //sum of x*y
            double s01 = this.getSy();   //sum of y

            //a = Da/D
            return (s21 * (s20 * s00 - s10 * s10) - s11 * (s30 * s00 - s10 * s20) + s01 * (s30 * s10 - s20 * s20))
                    /
                    (s40 * (s20 * s00 - s10 * s10) - s30 * (s30 * s00 - s10 * s20) + s20 * (s30 * s10 - s20 * s20));
        }

        /// <summary>
        /// returns the b term of the equation ax^2 + bx + c
        /// </summary>
        /// <returns>b term</returns>
        public double bTerm()
        {
            if (this.numOfEntries < 3)
            {
                throw new InvalidOperationException("Insufficient pairs of co-ordinates");
            }
            //notation sjk to mean the sum of x_i^j*y_i^k.
            double s40 = this.getSx4(); //sum of x^4
            double s30 = this.getSx3(); //sum of x^3
            double s20 = this.getSx2(); //sum of x^2
            double s10 = this.getSx();  //sum of x
            double s00 = this.numOfEntries;  //sum of x^0 * y^0  ie 1 * number of entries

            double s21 = this.getSx2y(); //sum of x^2*y
            double s11 = this.getSxy();  //sum of x*y
            double s01 = this.getSy();   //sum of y

            //b = Db/D
            return (s40 * (s11 * s00 - s01 * s10) - s30 * (s21 * s00 - s01 * s20) + s20 * (s21 * s10 - s11 * s20))
                    /
                    (s40 * (s20 * s00 - s10 * s10) - s30 * (s30 * s00 - s10 * s20) + s20 * (s30 * s10 - s20 * s20));
        }

        /// <summary>
        /// returns the c term of the equation ax^2 + bx + c
        /// </summary>
        /// <returns>c term</returns>
        public double cTerm()
        {
            if (this.numOfEntries < 3)
            {
                throw new InvalidOperationException("Insufficient pairs of co-ordinates");
            }
            //notation sjk to mean the sum of x_i^j*y_i^k.
            double s40 = this.getSx4(); //sum of x^4
            double s30 = this.getSx3(); //sum of x^3
            double s20 = this.getSx2(); //sum of x^2
            double s10 = this.getSx();  //sum of x
            double s00 = this.numOfEntries;  //sum of x^0 * y^0  ie 1 * number of entries

            double s21 = this.getSx2y(); //sum of x^2*y
            double s11 = this.getSxy();  //sum of x*y
            double s01 = this.getSy();   //sum of y

            //c = Dc/D
            return (s40 * (s20 * s01 - s10 * s11) - s30 * (s30 * s01 - s10 * s21) + s20 * (s30 * s11 - s20 * s21))
                    /
                    (s40 * (s20 * s00 - s10 * s10) - s30 * (s30 * s00 - s10 * s20) + s20 * (s30 * s10 - s20 * s20));
        }

        public double rSquare() // get r-squared
        {
            if (this.numOfEntries < 3)
            {
                throw new InvalidOperationException("Insufficient pairs of co-ordinates");
            }
            // 1 - (total sum of squares / residual sum of squares)
            return 1 - this.getSSerr() / this.getSStot();
        }

        /*helper methods*/

        private double getPredictedY(double x)
        {
            //returns value of y predicted by the equation for a given value of x
            return this.aTerm() * Math.Pow(x, 2) + this.bTerm() * x + this.cTerm();
        }

        private double getSSerr() // residual sum of squares
        {
            //the sum of the squares of te difference between
            //the measured y values and the values of y predicted by the equation
            double ss_err = 0;
            foreach (double[] ppair in this.pointArray)
            {
                ss_err += Math.Pow(ppair[1] - this.getPredictedY(ppair[0]), 2);
            }
            return ss_err;
        }

        private double getSStot() // total sum of squares
        {
            //the sum of the squares of the differences between
            //the measured y values and the mean y value
            double ss_tot = 0;
            foreach (double[] ppair in this.pointArray)
            {
                ss_tot += Math.Pow(ppair[1] - this.getYMean(), 2);
            }
            return ss_tot;
        }

        private double getSx() // get sum of x
        {
            double Sx = 0;
            foreach (double[] ppair in this.pointArray)
            {
                Sx += ppair[0];
            }
            return Sx;
        }

        private double getSx2() // get sum of x^2
        {
            double Sx2 = 0;
            foreach (double[] ppair in this.pointArray)
            {
                Sx2 += Math.Pow(ppair[0], 2); // sum of x^2
            }
            return Sx2;
        }

        private double getSx2y() // get sum of x^2*y
        {
            double Sx2y = 0;
            foreach (double[] ppair in this.pointArray)
            {
                Sx2y += Math.Pow(ppair[0], 2) * ppair[1]; // sum of x^2*y
            }
            return Sx2y;
        }

        private double getSx3() // get sum of x^3
        {
            double Sx3 = 0;
            foreach (double[] ppair in this.pointArray)
            {
                Sx3 += Math.Pow(ppair[0], 3); // sum of x^3
            }
            return Sx3;
        }

        private double getSx4() // get sum of x^4
        {
            double Sx4 = 0;
            foreach (double[] ppair in this.pointArray)
            {
                Sx4 += Math.Pow(ppair[0], 4); // sum of x^4
            }
            return Sx4;
        }

        private double getSxy() // get sum of x*y
        {
            double Sxy = 0;
            foreach (double[] ppair in this.pointArray)
            {
                Sxy += ppair[0] * ppair[1]; // sum of x*y
            }
            return Sxy;
        }

        private double getSy() // get sum of y
        {
            double Sy = 0;
            foreach (double[] ppair in this.pointArray)
            {
                Sy += ppair[1];
            }
            return Sy;
        }

        private double getYMean() // mean value of y
        {
            double y_tot = 0;
            foreach (double[] ppair in this.pointArray)
            {
                y_tot += ppair[1];
            }
            return y_tot / this.numOfEntries;
        }

        #endregion
    }
}
