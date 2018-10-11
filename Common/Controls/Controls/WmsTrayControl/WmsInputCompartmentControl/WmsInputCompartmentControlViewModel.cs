using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.CodeParser;
using DevExpress.Mvvm;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    class WmsInputCompartmentControlViewModel : INotifyPropertyChanged, IEventUI
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private CompartmentDetails compartmentInput;
        private ICommand createNewCompartmentCommand;
        public ICommand CreateNewCompartmentCommand => this.createNewCompartmentCommand ??
                 (this.createNewCompartmentCommand = new DelegateCommand(this.ExecuteNewCreateCompartmentCommand));


        public WmsInputCompartmentControlViewModel()
        {
            this.compartmentInput = new CompartmentDetails();
            this.compartmentInput.Width = 1;
            this.compartmentInput.Height = 1;
            this.compartmentInput.XPosition = 0;
            this.compartmentInput.YPosition = 0;
            //this.compartmentInput.MaxCapacity = 0;
            this.compartmentInput.Stock = 0;
            this.compartmentInput.ItemCode = "Item";
            this.CompartmentInput = this.compartmentInput;
        }

        public CompartmentDetails CompartmentInput
        {
            get
            {
                return this.compartmentInput;
            }
            set
            {
                this.compartmentInput = value;
                this.NotifyPropertyChanged(nameof(this.compartmentInput));
            }
        }
        /*private WmsCompartmentViewModel compartmentInput;

        public WmsInputCompartmentControlViewModel()
        {
            this.compartmentInput = new WmsCompartmentViewModel();
            this.compartmentInput.Width = 0;
            this.compartmentInput.Height = 0;
            this.compartmentInput.Top = 0;
            this.compartmentInput.Left = 0;
            this.compartmentInput.Capacity = 0;
            this.compartmentInput.Quantity = 0;
            this.compartmentInput.Article = "Item";
            this.CompartmentInput = this.compartmentInput;
        }

        public WmsCompartmentViewModel CompartmentInput
        {
            get
            {
                return this.compartmentInput;
            }
            set
            {
                this.compartmentInput = value;
                this.NotifyPropertyChanged(nameof(this.compartmentInput));
            }
        }*/
        private void ExecuteNewCreateCompartmentCommand()
        {

        }

            protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

       

        public void CreateButtonForm(EventUI newEvent)
        {
            if(newEvent.data is CompartmentDetails compartment)
            {

            }
        }
    }
}
