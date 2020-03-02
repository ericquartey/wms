using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Installation.Views
{
    public class CellPanelsCheckStepTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EnumConfirm { get; set; }

        public DataTemplate EnumInizialize { get; set; }

        public DataTemplate EnumMeasured { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            CellPanelsCheckStep value = (CellPanelsCheckStep)(item ?? CellPanelsCheckStep.Inizialize);
            switch (value)
            {
                case CellPanelsCheckStep.Inizialize:
                    return this.EnumInizialize;

                case CellPanelsCheckStep.Measured:
                    return this.EnumMeasured;

                case CellPanelsCheckStep.Confirm:
                    return this.EnumConfirm;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
