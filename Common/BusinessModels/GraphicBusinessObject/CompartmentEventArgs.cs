using System;

namespace Ferretto.Common.BusinessModels
{
    public class CompartmentEventArgs : EventArgs
    {
        #region Fields

        private readonly CompartmentDetails compartment;

        #endregion

        #region Constructors

        public CompartmentEventArgs(CompartmentDetails compartment)
        {
            this.compartment = compartment;
        }

        #endregion

        #region Properties

        public CompartmentDetails Compartment => this.compartment;

        #endregion
    }
}
