using System;

namespace Ferretto.Common.BusinessModels
{
    public class EditFilter : IFilter
    {
        #region Fields

        private static readonly Func<ICompartment, ICompartment, string> colorFunc =
            delegate (ICompartment compartment, ICompartment selected)
            {
                var color = "#57A639";
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

    public class EditReadOnlyFilter : IFilter
    {
        #region Fields

        private static readonly Func<ICompartment, ICompartment, string> colorFunc =
            delegate (ICompartment compartment, ICompartment selected)
            {
                var color = "#e6e6e6";
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
