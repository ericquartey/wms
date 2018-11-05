using System;

namespace Ferretto.Common.BusinessModels
{
    public class FillingFilter : IFilter
    {
        #region Fields

        private static readonly Func<CompartmentDetails, CompartmentDetails, string> colorFunc = delegate (CompartmentDetails compartment, CompartmentDetails selected)
        {
            var stock = compartment.Stock;
            var max = compartment.MaxCapacity;
            var color = "GreenYellow";

            if (max == null)
            {
                color = "#BDBDBD";
            }
            else
            {
                if (selected == null)
                {
                    var filling = (double)stock / max.Value * 100.0;
                    if (stock == 0 || filling < 40)
                    {
                        color = "#76FF03";
                    }
                    else if (filling < 60)
                    {
                        color = "#D4E157";
                    }
                    else if (filling < 80)
                    {
                        color = "#FF9800";
                    }
                    else if (filling <= 99)
                    {
                        color = "#F44336";
                    }
                    else
                    {
                        color = "#D50000";
                    }
                }
                else
                {
                    var stockSelected = selected.Stock;
                    var maxSelected = selected.MaxCapacity;
                    var fillingSelected = (double)stockSelected / (int)maxSelected * 100;
                    var filling = (double)stock / max.Value * 100;

                    color = filling == fillingSelected ? "#76FF03" : "#90A4AE";
                }
            }

            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, CompartmentDetails, string> ColorFunc => colorFunc;
        public string Description => "Compartment";
        public int Id => 2;
        public CompartmentDetails Selected { get; set; }

        #endregion Properties
    }
}
