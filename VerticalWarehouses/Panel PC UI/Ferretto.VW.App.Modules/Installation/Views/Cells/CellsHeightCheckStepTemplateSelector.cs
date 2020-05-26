using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Installation.Views
{
    public class CellsHeightCheckStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumConfirm { get; set; }

        public DataTemplate EnumInizialize { get; set; }

        public DataTemplate EnumMeasured { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (CellsHeightCheckStep)(item ?? CellsHeightCheckStep.Inizialize);
            switch (value)
            {
                case CellsHeightCheckStep.Inizialize:
                    return this.EnumInizialize;

                case CellsHeightCheckStep.Measured:
                    return this.EnumMeasured;

                case CellsHeightCheckStep.Confirm:
                    return this.EnumConfirm;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
