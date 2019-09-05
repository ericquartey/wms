﻿using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Mvvm;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public class ViewModelBase : BindableBase, INavigableViewModel, INavigationAware
    {
        #region Constructors

        protected ViewModelBase()
        {
        }

        #endregion

        #region Properties

        public object Data { get; set; }

        public bool IsDisposed
        {
            get;
            private set;
        }

        public string MapId { get; set; }

        #endregion

        #region Methods

        public virtual void Appear()
        {
            // do nothing
        }

        public virtual bool CanClose()
        {
            return true;
        }

        public virtual void Disappear()
        {
            // do nothing
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual Task OnNavigatedAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // do nothing
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            // do nothing
        }

        protected virtual void OnDispose()
        {
            // do nothing
            // let the derived classes implement the dispose behaviour
        }

        /// <summary>
        /// Delete the object created from the object.
        /// <para>Free resources</para>
        /// </summary>
        /// <param name="disposing">If true the delete of managed object is required</param>
        private void Dispose(bool disposing)
        {
            if (this.IsDisposed == false)
            {
                this.IsDisposed = true;

                if (disposing)
                {
                    // Free other state (managed objects).
                    this.OnDispose();
                }
            }
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
