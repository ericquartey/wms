﻿using System;
using System.ComponentModel;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces.Models;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public abstract class SidePanelDetailsViewModel<TModel> : DetailsViewModel<TModel>, ICompletable
        where TModel : class, ICloneable, IModel<int>, INotifyPropertyChanged, IDataErrorInfo, IPolicyDescriptor<IPolicy>
    {
        #region Fields

        private ICommand cancelCommand;

        private string title;

        #endregion

        #region Events

        public event EventHandler<OperationEventArgs> OperationComplete;

        #endregion

        #region Properties

        public ICommand CancelCommand => this.cancelCommand ??
                                  (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand));

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        #endregion

        #region Methods

        protected virtual void CompleteOperation(bool isCanceled = false)
        {
            this.OperationComplete?.Invoke(this, new OperationEventArgs(this.Model, isCanceled));
        }

        private void ExecuteCancelCommand()
        {
            this.CompleteOperation(isCanceled: true);
        }

        #endregion
    }
}
