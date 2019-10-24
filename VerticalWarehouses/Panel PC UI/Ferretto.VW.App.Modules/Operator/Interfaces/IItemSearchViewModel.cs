﻿using Ferretto.VW.Utils.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Interfaces
{
    public interface IItemSearchViewModel
    {
        #region Properties

        Item SelectedItem { get; set; }

        #endregion
    }
}
