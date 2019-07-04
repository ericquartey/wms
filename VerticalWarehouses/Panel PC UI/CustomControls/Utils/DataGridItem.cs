using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Utils
{
    public class DataGridItem : BindableBase
    {
        #region Fields

        private string article;

        private double availableQuantity;

        private string description;

        private string imageCode;

        private string machine;

        #endregion

        #region Constructors

        public DataGridItem(string article, string description, string machine, string imageCode)
        {
            this.article = article;
            this.description = description;
            this.machine = machine;
            this.imageCode = imageCode;
        }

        public DataGridItem()
        {
        }

        #endregion

        #region Properties

        public string Article { get => this.article; set => this.SetProperty(ref this.article, value); }

        public double AvailableQuantity { get => this.availableQuantity; set => this.SetProperty(ref this.availableQuantity, value); }

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public string ImageCode { get => this.imageCode; set => this.SetProperty(ref this.imageCode, value); }

        public string Machine { get => this.machine; set => this.SetProperty(ref this.machine, value); }

        #endregion
    }
}
