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
    public class CalibrationStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumConfirm { get; set; }

        public DataTemplate EnumFirstMisuration { get; set; }

        public DataTemplate EnumLastMisuration { get; set; }

        public DataTemplate EnumPositionMeter { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            CalibrationStep value = (CalibrationStep)(item ?? CalibrationStep.PositionMeter);
            switch (value)
            {
                case CalibrationStep.PositionMeter:
                    return this.EnumPositionMeter;

                case CalibrationStep.FirstMisuration:
                    return this.EnumFirstMisuration;

                case CalibrationStep.LastMisuration:
                    return this.EnumLastMisuration;

                case CalibrationStep.Confirm:
                    return this.EnumConfirm;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
