using System;
using System.ComponentModel;

namespace Ferretto.VW.VerticalWarehousesApp.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool isUserLoggedIn = false;

        public Boolean IsUserLoggedIn { get => this.isUserLoggedIn; set { this.isUserLoggedIn = value; this.RaisePropertyChanged("IsUserLoggedIn"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if(PropertyChanged != null) {
                PropertyChanged(this, e);
            }
        }

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void test() { }
    }
}
