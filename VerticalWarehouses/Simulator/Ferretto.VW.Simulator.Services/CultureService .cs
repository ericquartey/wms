using System;
using Ferretto.VW.Simulator.Services.Interfaces;
using Ferretto.VW.Simulator.Services.Models;

namespace Ferretto.VW.Simulator.Services
{
    internal class CultureService : ICultureService
    {
        #region Properties

        public ApplicationCulture ActiveCulture { get; private set; } = ApplicationCulture.Eng;

        #endregion

        //[System.Diagnostics.CodeAnalysis.SuppressMessage(
        //    "Minor Code Smell",
        //    "S1075:URIs should not be hardcoded",
        //    Justification = "Extract URIs")]

        #region Methods

        public void ApplyCulture(ApplicationCulture culture)
        {
            if (this.ActiveCulture == culture)
            {
                return;
            }

            switch (culture)
            {
                case ApplicationCulture.Eng:
                    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                    System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                    break;

                case ApplicationCulture.Ita:
                    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
                    System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
                    break;

                default:
                    throw new NotSupportedException("The specified culture is not supported.");
            }

            this.ActiveCulture = culture;
        }

        #endregion
    }
}
