﻿using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Scomparto
    public sealed class CompartmentType : IDataModel<int>
    {
        #region Properties

        public IEnumerable<Compartment> Compartments { get; set; }

        public IEnumerable<DefaultCompartment> DefaultCompartments { get; set; }

        public double Depth { get; set; }

        public int Id { get; set; }

        public IEnumerable<ItemCompartmentType> ItemsCompartmentTypes { get; set; }

        public double Width { get; set; }

        #endregion
    }
}
