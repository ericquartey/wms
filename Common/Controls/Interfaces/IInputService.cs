using System;
using System.Windows;
using Ferretto.Common.Controls.Services;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IInputService
    {
        #region Properties

        IInputElement FocusedElement { get; }

        #endregion Properties

        #region Methods

        void BeginMouseNotify(object instance, Action<MouseDownInfo> callback);

        void BeginShortKeyNotify(object instance, Action<ShortKeyInfo> callback);

        void End();

        void EndMouseNotify(object instance);

        void EndShortKeyNotify(object instance);

        void RaiseEvent(System.Windows.Input.Key key);

        void Start();

        #endregion Methods
    }
}
