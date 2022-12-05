using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.Views
{
    public class WeightCalibrationStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumCallUnit { get; set; }

        public DataTemplate EnumEmptyUnitWeighing { get; set; }

        public DataTemplate EnumFullUnitWeighing { get; set; }

        public DataTemplate EnumOptionalWeighing1 { get; set; }

        public DataTemplate EnumSetWeight { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (WeightCalibartionStep)(item ?? WeightCalibartionStep.CallUnit);

            switch (value)
            {
                case WeightCalibartionStep.CallUnit:
                    return this.EnumCallUnit;

                case WeightCalibartionStep.Empty:
                    return this.EnumEmptyUnitWeighing;

                case WeightCalibartionStep.Optional:
                    return this.EnumOptionalWeighing1;

                case WeightCalibartionStep.Full:
                    return this.EnumFullUnitWeighing;

                case WeightCalibartionStep.SetWeight:
                    return this.EnumSetWeight;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
