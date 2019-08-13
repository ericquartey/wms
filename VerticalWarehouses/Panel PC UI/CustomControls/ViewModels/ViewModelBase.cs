using System;
using System.Threading.Tasks;
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

        #region Destructors

        // Use C# destructor syntax for finalization code.
        ~ViewModelBase()
        {
            // Simply call Dispose(false).
            this.Dispose(false);
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
        }

        public virtual bool CanClose()
        {
            return true;
        }

        public virtual void Disappear()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual Task OnNavigatedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Delete the object created from the object.
        /// <para>Free resources</para>
        /// </summary>
        /// <param name="disposing">If true the delete of managed object is required</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed == false)
            {
                this.IsDisposed = true;

                if (disposing)
                {
                    // Free other state (managed objects).
                    this.OnDispose();
                }

                // Free your own state (unmanaged objects).
                this.OnFinalize();
            }
        }

        protected virtual void OnDispose()
        {
        }

        protected virtual void OnFinalize()
        {
        }

        #endregion
    }
}
