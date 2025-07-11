﻿using System.ComponentModel;

#nullable enable

namespace Ferretto.VW.Installer.Services
{
    public interface INotificationService : INotifyPropertyChanged
    {
        #region Methods

        void ClearMessage();

        void SetErrorMessage(string message);

        void SetMessage(string message);

        public bool IsError { get; }

        public string? Message { get; }

        #endregion
    }
}
