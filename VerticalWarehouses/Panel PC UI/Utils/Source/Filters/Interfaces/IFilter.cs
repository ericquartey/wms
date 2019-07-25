using System;
using Ferretto.Common.Controls.WPF;

namespace Ferretto.VW.Utils.Source.Filters.Interfaces
{
    public interface IFilter
    {
        #region Properties

        Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc { get; }

        string Description { get; }

        int Id { get; }

        IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
