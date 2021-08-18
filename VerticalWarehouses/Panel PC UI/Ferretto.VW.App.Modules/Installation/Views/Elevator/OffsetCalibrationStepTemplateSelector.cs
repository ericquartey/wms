using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Installation.Views
{
    public class OffsetCalibrationStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumCellMeasured { get; set; }

        public DataTemplate EnumConfirm { get; set; }

        public DataTemplate EnumOriginCalibration { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (VerticalOffsetCalibrationStep)(item ?? VerticalOffsetCalibrationStep.CellMeasured);
            switch (value)
            {
                case VerticalOffsetCalibrationStep.CellMeasured:
                    return this.EnumCellMeasured;

                case VerticalOffsetCalibrationStep.Confirm:
                    return this.EnumConfirm;

                case VerticalOffsetCalibrationStep.OriginCalibration:
                    return this.EnumOriginCalibration;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
