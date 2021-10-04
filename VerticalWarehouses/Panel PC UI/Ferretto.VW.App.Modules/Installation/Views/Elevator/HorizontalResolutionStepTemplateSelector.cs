using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Installation.Views
{
    public class HorizontalResolutionStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumChainCalibration { get; set; }

        public DataTemplate EnumConfirmAdjustment { get; set; }

        public DataTemplate EnumEndChainCalibration { get; set; }

        public DataTemplate EnumRunningCalibration { get; set; }

        public DataTemplate EnumStartCalibration { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (HorizontalResolutionCalibrationStep)(item ?? HorizontalResolutionCalibrationStep.StartCalibration);
            switch (value)
            {
                case HorizontalResolutionCalibrationStep.StartCalibration:
                    return this.EnumStartCalibration;

                case HorizontalResolutionCalibrationStep.RunningCalibration:
                    return this.EnumRunningCalibration;

                case HorizontalResolutionCalibrationStep.ConfirmAdjustment:
                    return this.EnumConfirmAdjustment;

                case HorizontalResolutionCalibrationStep.ChainCalibration:
                    return this.EnumChainCalibration;

                case HorizontalResolutionCalibrationStep.EndChainCalibration:
                    return this.EnumEndChainCalibration;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
