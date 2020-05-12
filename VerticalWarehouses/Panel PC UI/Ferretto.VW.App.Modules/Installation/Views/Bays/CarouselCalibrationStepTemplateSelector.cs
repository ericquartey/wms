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
    public class CarouselCalibrationStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumConfirmAdjustment { get; set; }

        public DataTemplate EnumRunningCalibration { get; set; }

        public DataTemplate EnumStartCalibration { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            CarouselCalibrationStep value = (CarouselCalibrationStep)(item ?? CarouselCalibrationStep.StartCalibration);
            switch (value)
            {
                case CarouselCalibrationStep.StartCalibration:
                    return this.EnumStartCalibration;

                case CarouselCalibrationStep.RunningCalibration:
                    return this.EnumRunningCalibration;

                case CarouselCalibrationStep.ConfirmAdjustment:
                    return this.EnumConfirmAdjustment;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
