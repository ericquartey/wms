using System;

namespace Ferretto.Common.BusinessModels
{
    public class ArticleFilter : IFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
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

        public string Description => "Article";

        public int Id => 1;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
