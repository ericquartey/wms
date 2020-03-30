using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;

namespace Ferretto.VW.App.Modules.Installation.ViewModels.Bays
{
    public class DepositAndPickUpTestTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumCallunit { get; set; }

        public DataTemplate EnumCycleTest { get; set; }

        public DataTemplate EnumEndTest { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DepositAndPickUpStep value = (DepositAndPickUpStep)(item ?? DepositAndPickUpStep.CallUnit);
            switch (value)
            {
                case DepositAndPickUpStep.EndTest:
                    return this.EnumEndTest;

                case DepositAndPickUpStep.CycleTest:
                    return this.EnumCycleTest;

                case DepositAndPickUpStep.CallUnit:
                    return this.EnumCallunit;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
