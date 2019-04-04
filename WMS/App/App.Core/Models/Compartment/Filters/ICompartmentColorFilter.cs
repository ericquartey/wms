using System;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.App.Core.Models
{
    public interface ICompartmentColorFilter
    {
        #region Properties

        Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc { get; }

        string Description { get; }

        int Id { get; }

        IDrawableCompartment Selected { get; set; }

        #endregion
    }
}
