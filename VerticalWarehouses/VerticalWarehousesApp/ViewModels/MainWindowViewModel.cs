using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ferretto.VW.VerticalWarehousesApp.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool isUserLoggedIn = false;
        private SolidColorBrush userLoggedInRectBrush = new SolidColorBrush(Colors.Red);

        public Boolean IsUserLoggedIn { get => this.isUserLoggedIn; set { this.isUserLoggedIn = value; this.RaisePropertyChanged("IsUserLoggedIn"); } }
        public SolidColorBrush UserLoggedInRectBrush { get => this.userLoggedInRectBrush; set { this.userLoggedInRectBrush = value; this.RaisePropertyChanged("UserLoggedInRectBrush"); } }

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

    }
}
