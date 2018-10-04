using Ferretto.Common.Controls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class BaseNavigationViewModel : BindableBase, INavigableViewModel
    {
        #region Constructors

        protected BaseNavigationViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public object Data { get; set; }
        public string MapId { get; set; }
        public string StateId { get; set; }
        public string Token { get; set; }

        #endregion Properties

        #region Methods

        public void Appear()
        {
            this.OnAppear();
        }

        public void Disappear()
        {
            this.OnDisappear();
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

        #endregion Methods
    }
}
