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
    public class BayCheckStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumConfirm { get; set; }

        public DataTemplate EnumPositionDown { get; set; }

        public DataTemplate EnumPositionUp { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var value = (BayCheckStep)(item ?? BayCheckStep.PositionUp);
            switch (value)
            {
                case BayCheckStep.PositionUp:
                    return this.EnumPositionUp;

                case BayCheckStep.PositionDown:
                    return this.EnumPositionDown;

                case BayCheckStep.Confirm:
                    return this.EnumConfirm;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
