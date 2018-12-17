using System;

namespace Ferretto.Common.BusinessModels
{
    public class FillingFilter : IFilter
    {
        #region Fields

        private static readonly Func<ICompartment, ICompartment, string> colorFunc =
            delegate (ICompartment compartment, ICompartment selected)
            {
                var compartmentDetails = compartment as CompartmentDetails;

                var stock = compartmentDetails.Stock;
                var max = compartmentDetails.MaxCapacity;
                string color;

                if (max == null || max.Value == 0)
                {
                    color = "#00000000";
                }
                else
                {
                    var filling = (double)stock / max.Value * 100.0;

                    if (stock == 0)
                    {
                        color = "#FF63BE7B";
                    }
                    else if (filling < 10)
                    {
                        color = "#FF80C77D";
                    }
                    else if (filling < 20)
                    {
                        color = "#FF9CCF7F";
                    }
                    else if (filling < 30)
                    {
                        color = "#FFB9D780";
                    }
                    else if (filling < 40)
                    {
                        color = "#FFD5DF82";
                    }
                    else if (filling < 50)
                    {
                        color = "#FFF1E784";
                    }
                    else if (filling < 60)
                    {
                        color = "#FFFEDF81";
                    }
                    else if (filling < 70)
                    {
                        color = "#FFFDC77D";
                    }
                    else if (filling < 80)
                    {
                        color = "#FFFBAF78";
                    }
                    else if (filling < 90)
                    {
                        color = "#FFFA9874";
                    }
                    else if (filling < 100)
                    {
                        color = "#FFF9806F";
                    }
                    else
                    {
                        color = "#FFF8696B";
                    }
                }

                return color;
            };

        #endregion Fields

        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => colorFunc;

        public string Description => "Compartment";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion Properties
    }
}
