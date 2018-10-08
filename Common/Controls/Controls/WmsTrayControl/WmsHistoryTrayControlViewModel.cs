using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Ferretto.Common.Modules.BLL.Models;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class WmsHistoryTrayControlViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly string ColorFill = ((SolidColorBrush)System.Windows.Application.Current.Resources["Red"])
                .Color.ToString();

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public LoadingUnitDetails LoadingUnitProperty { get; set; }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            LoadingUnitDetails lu = new LoadingUnitDetails();
            CompartmentDetails cd;
            var listWmsCompartment = new List<WmsCompartment>();
            //listWmsCompartment.Add(new WmsCompartment(150, 150, 0, 0));
            //listWmsCompartment.Add(new WmsCompartment(150, 150, 0, 150));
            //listWmsCompartment.Add(new WmsCompartment(150, 150, 0, 300));
            //listWmsCompartment.Add(new WmsCompartment(150, 150, 0, 450));
            ////
            //listWmsCompartment.Add(new WmsCompartment(50, 150, 150, 0));
            //listWmsCompartment.Add(new WmsCompartment(50, 150, 150, 150));
            //listWmsCompartment.Add(new WmsCompartment(50, 150, 150, 300));
            //listWmsCompartment.Add(new WmsCompartment(50, 150, 150, 450));
            ////
            //listWmsCompartment.Add(new WmsCompartment(300, 300, 200, 0));
            //listWmsCompartment.Add(new WmsCompartment(300, 300, 200, 300));
            //listci.Add(new CompartmentInput(300, 300, 300, 600));
            //
            //listci.Add(new CompartmentInput(300, 300, 300, 0));
            //listci.Add(new CompartmentInput(300, 300, 300, 300));
            //listci.Add(new CompartmentInput(300, 300, 300, 600));

            //int pixel50 = (int)this.ConvertMillimetersToPixel(50);

            //DRAW DYNAMIC SCOMPARTMENT
            //listCompartmentDetails.AddRange(this.SplitAreaDynamical(500, 0, 600, 300, this.pixelSquare, this.pixelSquare));

            foreach (var wmsCompartment in listWmsCompartment)
            {
                this.DrawNewCompartment(wmsCompartment);
            }
        }

        public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        {
            this.LoadingUnitProperty = loadingUnitDetails;
            //TODO
        }

        private void DrawNewCompartment(WmsCompartment ci)
        {
            throw new NotImplementedException();
        }

        private void TransformDataInput()
        {
            var listWmsCompartment = new List<WmsCompartment>();
            var compartments = this.LoadingUnitProperty.Compartments;
            foreach (var compartment in compartments)
            {
                listWmsCompartment.Add(new WmsCompartment((int)compartment.Width, (int)compartment.Height, (int)compartment.XPosition, (int)compartment.YPosition, this.ColorFill));
            }

            var
        }

        #endregion Methods
    }
}
