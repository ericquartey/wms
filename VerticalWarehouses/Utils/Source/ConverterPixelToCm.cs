using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Utils.Source
{
    public static class ConverterPixelToCm
    {
        #region Fields

        private const int CURRENT_DEPTH_INDEX = 0;
        private const int CURRENT_LENGHT_INDEX = 0;
        private const int DEPTH_1 = 650;
        private const int DEPTH_2 = 840;
        private const int DEPTH_3 = 1030;
        private const int LENGHT_1 = 1950;
        private const int LENGHT_2 = 2450;
        private const int LENGHT_3 = 3050;
        private const int LENGHT_4 = 3650;
        private const int LENGHT_5 = 4250;
        private static readonly double[] depths = new double[] { DEPTH_1, DEPTH_2, DEPTH_3 };
        private static readonly double[] lenghts = new double[] { LENGHT_1, LENGHT_2, LENGHT_3, LENGHT_4, LENGHT_5, };
        private static double[][] ratio = PopulateRatioMatrix();

        #endregion Fields

        #region Properties

        public static Double[][] Ratio { get => ratio; set => ratio = value; }

        #endregion Properties

        #region Methods

        public static double CompartmentImageHeightPixels(int lenght_pixels)
        {
            return lenght_pixels / ratio[CURRENT_DEPTH_INDEX][CURRENT_LENGHT_INDEX];
        }

        public static double[][] PopulateRatioMatrix()
        {
            double[][] ret_val = new double[depths.Length][];
            for (int i = 0; i < depths.Length; i++)
            {
                ret_val[i] = new double[lenghts.Length];
                for (int j = 0; j < lenghts.Length; j++)
                {
                    ret_val[i][j] = lenghts[j] / depths[i];
                }
            }
            return ret_val;
        }

        #endregion Methods
    }
}
