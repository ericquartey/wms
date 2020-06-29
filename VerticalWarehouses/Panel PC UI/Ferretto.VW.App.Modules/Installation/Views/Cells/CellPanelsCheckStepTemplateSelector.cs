using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Installation.Views
{
    public class CellPanelsCheckStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumInizialize { get; set; }

        public DataTemplate EnumMeasuredBack { get; set; }

        public DataTemplate EnumMeasuredFront { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (CellPanelsCheckStep)(item ?? CellPanelsCheckStep.Inizialize);
            switch (value)
            {
                case CellPanelsCheckStep.Inizialize:
                    return this.EnumInizialize;

                case CellPanelsCheckStep.MeasuredFront:
                    return this.EnumMeasuredFront;

                case CellPanelsCheckStep.MeasuredBack:
                    return this.EnumMeasuredBack;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
