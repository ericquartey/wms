using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Ferretto.VW.Installer.Core
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            Application.Current?.Dispatcher.Invoke(() =>
                 this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
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
