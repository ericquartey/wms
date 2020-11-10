using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Installation.Views
{
    public class HorizontalCalibrationStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumChainCalibration { get; set; }

        public DataTemplate EnumConfirmAdjustment { get; set; }

        public DataTemplate EnumRunningCalibration { get; set; }

        public DataTemplate EnumStartCalibration { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (HorizontalChainCalibrationStep)(item ?? HorizontalChainCalibrationStep.StartCalibration);
            switch (value)
            {
                case HorizontalChainCalibrationStep.StartCalibration:
                    return this.EnumStartCalibration;

                case HorizontalChainCalibrationStep.RunningCalibration:
                    return this.EnumRunningCalibration;

                case HorizontalChainCalibrationStep.ConfirmAdjustment:
                    return this.EnumConfirmAdjustment;

                case HorizontalChainCalibrationStep.ChainCalibration:
                    return this.EnumChainCalibration;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
