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

        public DataTemplate EnumFirstMeasured { get; set; }

        public DataTemplate EnumLastMeasured { get; set; }

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

                case CalibrationStep.FirstMeasured:
                    return this.EnumFirstMeasured;

                case CalibrationStep.LastMeasured:
                    return this.EnumLastMeasured;

                case CalibrationStep.Confirm:
                    return this.EnumConfirm;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
