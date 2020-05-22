using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.InvertersParametersGenerator.Models;

namespace Ferretto.VW.InvertersParametersGenerator.Interfaces
{
    public interface IParentActionChanged
    {
        #region Methods

        void Notify(Exception exception, NotificationSeverity notificationSeverity);

        void Notify(string message, NotificationSeverity notificationSeverity);

        void RaiseCanExecuteChanged();

        #endregion
    }
}
