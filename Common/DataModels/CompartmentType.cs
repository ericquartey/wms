using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Scomparto
    public sealed class CompartmentType : IDataModel
    {
        #region Properties

        public IEnumerable<Compartment> Compartments { get; set; }

        public IEnumerable<DefaultCompartment> DefaultCompartments { get; set; }

        public int? Height { get; set; }

        public int Id { get; set; }

        public IEnumerable<ItemCompartmentType> ItemsCompartmentTypes { get; set; }

        public int? Width { get; set; }

        #endregion Properties
    }
}
