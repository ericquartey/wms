using System;
using System.Windows;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls.Interfaces
{
    public interface IInputService
    {
        #region Properties

        IInputElement FocusedElement { get; }

        #endregion

        #region Methods

        void BeginMouseNotify(object instance, Action<MouseDownInfo> callback);

        void BeginShortKeyNotify(object instance, Action<ShortKeyInfo> callback);

        void EndMouseNotify(object instance);

        void EndShortKeyNotify(object instance);

        void Start();

        #endregion
    }
}
