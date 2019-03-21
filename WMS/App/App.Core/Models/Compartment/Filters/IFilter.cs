using System;

namespace Ferretto.WMS.App.Core.Models
{
    public interface IFilter
    {
        #region Properties

        Func<ICompartment, ICompartment, string> ColorFunc { get; }

        string Description { get; }

        int Id { get; }

        ICompartment Selected { get; set; }

        #endregion
    }
}
