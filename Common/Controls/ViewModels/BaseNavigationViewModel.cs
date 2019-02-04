using System;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class BaseNavigationViewModel : BindableBase, INavigableViewModel, IShortKey, IDisposable
    {
        #region Constructors

        protected BaseNavigationViewModel()
        {
        }

        #endregion

        #region Destructors

        // Use C# destructor syntax for finalization code.
        ~BaseNavigationViewModel()
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

        public string StateId { get; set; }

        public string Token { get; set; }

        #endregion

        #region Methods

        public void Appear()
        {
            this.OnAppear();
        }

        public virtual bool CanDisappear()
        {
            return true;
        }

        public virtual void Disappear()
        {
            this.OnDisappear();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual bool KeyPress(ShortKeyInfo shortKeyInfo)
        {
            return false;
        }

        protected virtual void OnAppear()
        {
            // Nothing to do here.
            // Derived classes can implement custom logic overriding this method.
        }

        protected virtual void OnDisappear()
        {
            // Nothing to do here.
            // Derived classes can implement custom logic overriding this method.
        }

        protected virtual void OnDispose()
        {
        }

        protected virtual void OnFinalize()
        {
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

                // Free your own state (unmanaged objects).
                this.OnFinalize();
            }
        }

        #endregion
    }
}
