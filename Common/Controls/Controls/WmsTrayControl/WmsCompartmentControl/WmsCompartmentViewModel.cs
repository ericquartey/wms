using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    public class WmsCompartmentViewModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                ((INotifyPropertyChanged)this.CompartmentDetailsProperty).PropertyChanged += value;
            }

            remove
            {
                ((INotifyPropertyChanged)this.CompartmentDetailsProperty).PropertyChanged -= value;
            }
        }

        #endregion Events

        #region Properties

        public CompartmentDetails CompartmentDetailsProperty { get; set; }

        #endregion Properties

        #region Methods

        public void Refresh(CompartmentDetails compartmentDetails)
        {
        }

        #endregion Methods
    }
}
