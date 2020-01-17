using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ferretto.VW.Installer
{
    internal abstract class BindableBase : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        protected void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (field == null && value != null || field?.Equals(value) == false)
            {
                field = value;
                this.RaisePropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        #endregion
    }
}
