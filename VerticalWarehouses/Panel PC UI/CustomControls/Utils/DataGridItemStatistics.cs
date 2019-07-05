using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Utils
{
    public class DataGridItemStatistics : BindableBase
    {
        #region Fields

        private string itemClass;

        private string itemPercentage;

        private string itemQuantity;

        private string movementeQuantity;

        private string movementPercentage;

        #endregion

        #region Properties

        public string ItemClass { get => this.itemClass; set => this.SetProperty(ref this.itemClass, value); }

        public string ItemPercentage { get => this.itemPercentage; set => this.SetProperty(ref this.itemPercentage, value); }

        public string ItemQuantity { get => this.itemQuantity; set => this.SetProperty(ref this.itemQuantity, value); }

        public string MovementeQuantity { get => this.movementeQuantity; set => this.SetProperty(ref this.movementeQuantity, value); }

        public string MovementPercentage { get => this.movementPercentage; set => this.SetProperty(ref this.movementPercentage, value); }

        #endregion
    }
}
