using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Installation.Views
{
    public class BEDTestStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumConfirmAdjustment { get; set; }

        public DataTemplate EnumRunningCalibration { get; set; }

        public DataTemplate EnumStartCalibration { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (BEDTestStep)(item ?? BEDTestStep.StartCalibration);
            switch (value)
            {
                case BEDTestStep.StartCalibration:
                    return this.EnumStartCalibration;

                case BEDTestStep.RunningCalibration:
                    return this.EnumRunningCalibration;

                case BEDTestStep.ConfirmAdjustment:
                    return this.EnumConfirmAdjustment;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
