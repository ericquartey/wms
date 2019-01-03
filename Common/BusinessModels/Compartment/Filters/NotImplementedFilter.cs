using System;

namespace Ferretto.Common.BusinessModels
{
    public class NotImplementdFilter : IFilter
    {
        #region Fields

        private static readonly Func<ICompartment, ICompartment, string> colorFunc = delegate (ICompartment compartment, ICompartment selected)
        {
            return "Gray";
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
