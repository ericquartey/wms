using System;

namespace Ferretto.Common.BusinessModels
{
    public class ArticleFilter : IFilter
    {
        #region Fields

        private static readonly Func<CompartmentDetails, CompartmentDetails, string> colorFunc = delegate (CompartmentDetails compartment, CompartmentDetails selected)
        {
            var color = "Orange";
            if (selected != null)
            {
                if ((compartment.MaterialStatusId != 0 || compartment == selected) && compartment.MaterialStatusId == selected.MaterialStatusId)
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

        public Func<CompartmentDetails, CompartmentDetails, string> ColorFunc => colorFunc;
        public string Description => "Article";
        public int Id => 1;
        public CompartmentDetails Selected { get; set; }

        #endregion Properties
    }
}
