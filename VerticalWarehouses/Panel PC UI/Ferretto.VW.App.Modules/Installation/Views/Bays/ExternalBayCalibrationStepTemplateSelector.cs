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
    public class ExternalBayCalibrationStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumConfirmAdjustment { get; set; }

        public DataTemplate EnumRunningCalibration { get; set; }

        public DataTemplate EnumStartCalibration { get; set; }

        public DataTemplate EnumCallUnit { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ExternalBayCalibrationStep value = (ExternalBayCalibrationStep)(item ?? ExternalBayCalibrationStep.StartCalibration);
            switch (value)
            {
                case ExternalBayCalibrationStep.StartCalibration:
                    return this.EnumStartCalibration;

                case ExternalBayCalibrationStep.RunningCalibration:
                    return this.EnumRunningCalibration;

                case ExternalBayCalibrationStep.ConfirmAdjustment:
                    return this.EnumConfirmAdjustment;

                case ExternalBayCalibrationStep.CallUnit:
                    return this.EnumCallUnit;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
