using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Installation.Views
{
    public class ProfileResolutionStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumConfirmAdjustment { get; set; }

        public DataTemplate EnumRunningCalibration { get; set; }

        public DataTemplate EnumStartCalibration { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (ProfileResolutionCalibrationStep)(item ?? ProfileResolutionCalibrationStep.StartCalibration);
            switch (value)
            {
                case ProfileResolutionCalibrationStep.StartCalibration:
                    return this.EnumStartCalibration;

                case ProfileResolutionCalibrationStep.RunningCalibration:
                    return this.EnumRunningCalibration;

                case ProfileResolutionCalibrationStep.ConfirmAdjustment:
                    return this.EnumConfirmAdjustment;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
