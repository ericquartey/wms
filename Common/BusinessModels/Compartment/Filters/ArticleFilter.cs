using System;

namespace Ferretto.Common.BusinessModels
{
    public class ArticleFilter : IFilter
    {
        #region Fields

        private static readonly Func<ICompartment, ICompartment, string> colorFunc = delegate (ICompartment compartment, ICompartment selected)
        {
            var compartmentDetails = compartment as CompartmentDetails;
            var selectedDetails = selected as CompartmentDetails;
            var color = "Orange";
            if (selectedDetails != null && compartmentDetails != null)
            {
                if ((compartmentDetails.MaterialStatusId != 0 || compartment == selected)
                    &&
                    compartmentDetails.MaterialStatusId == selectedDetails.MaterialStatusId)
                {
                    color = "#76FF03";
                }
                else
                {
                    color = "#90A4AE";
                }
            }
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => colorFunc;

        public string Description => "Article";

        public int Id => 1;

        public ICompartment Selected { get; set; }

        #endregion Properties
    }
}
