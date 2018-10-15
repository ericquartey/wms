using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Ferretto.VW.Navigation;

namespace Ferretto.VW.InstallationApp.Views
{
    public partial class VerticalAxisCalibrationView : UserControl
    {
        #region Fields

        private string lowerBound;
        private string offset;
        private string resolution;
        private string upperBound;

        #endregion Fields

        #region Constructors

        public VerticalAxisCalibrationView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public String LowerBound { get => this.lowerBound; set => this.lowerBound = value; }
        public String Offset { get => this.offset; set => this.offset = value; }
        public String Resolution { get => this.resolution; set => this.resolution = value; }
        public String UpperBound { get => this.upperBound; set => this.upperBound = value; }

        #endregion Properties

        #region Methods

        public void StartButtonMethod(object sender, EventArgs e)
        {
            this.StartMethod();
        }

        private void StartMethod()
        {
        }

        #endregion Methods
    }
}
