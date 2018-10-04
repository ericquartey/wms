using System;
using System.ComponentModel;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.VerticalWarehousesApp.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private bool isUserLoggedIn = false;

        #endregion Fields

        #region Constructors

        public MainWindowViewModel()
        {
            CellsManager cm = new CellsManager();
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public Boolean IsUserLoggedIn { get => this.isUserLoggedIn; set { this.isUserLoggedIn = value; this.RaisePropertyChanged("IsUserLoggedIn"); } }

        #endregion Properties

        #region Methods

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public void test()
        {
        }

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Methods
    }
}
