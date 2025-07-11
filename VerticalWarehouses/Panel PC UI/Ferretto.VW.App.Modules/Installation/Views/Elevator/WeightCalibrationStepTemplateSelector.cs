﻿using System.Windows;
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
            var value = (WeightCalibrationStep)(item ?? WeightCalibrationStep.CallUnit);

            switch (value)
            {
                case WeightCalibrationStep.CallUnit:
                    return this.EnumCallUnit;

                case WeightCalibrationStep.Empty:
                    return this.EnumEmptyUnitWeighing;

                case WeightCalibrationStep.Optional:
                    return this.EnumOptionalWeighing1;

                case WeightCalibrationStep.Full:
                    return this.EnumFullUnitWeighing;

                case WeightCalibrationStep.SetWeight:
                    return this.EnumSetWeight;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
