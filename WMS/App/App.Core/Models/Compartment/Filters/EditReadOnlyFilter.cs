﻿using System;

namespace Ferretto.WMS.App.Core.Models
{
    public class EditReadOnlyFilter : ICompartmentColorFilter
    {
        #region Properties

        public Func<ICompartment, ICompartment, string> ColorFunc => (compartment, selected) =>
        {
            return "#e6e6e6";
        };

        public string Description => "EditReadOnly";

        public int Id => 2;

        public ICompartment Selected { get; set; }

        #endregion
    }
}
