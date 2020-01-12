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
    public class ProfileCheckStepTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumDrawerPosition { get; set; }

        public DataTemplate EnumElevatorPosition { get; set; }

        public DataTemplate EnumInitialize { get; set; }

        public DataTemplate EnumResultCheck { get; set; }

        public DataTemplate EnumShapePosition { get; set; }

        public DataTemplate EnumTaraturaCatena { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ProfileCheckStep value = (ProfileCheckStep)(item ?? ProfileCheckStep.Initialize);
            switch (value)
            {
                case ProfileCheckStep.Initialize:
                    return this.EnumInitialize;

                case ProfileCheckStep.ElevatorPosition:
                    return this.EnumElevatorPosition;

                case ProfileCheckStep.DrawerPosition:
                    return this.EnumDrawerPosition;

                case ProfileCheckStep.ShapePosition:
                    return this.EnumShapePosition;

                case ProfileCheckStep.TaraturaCatena:
                    return this.EnumTaraturaCatena;

                case ProfileCheckStep.ResultCheck:
                    return this.EnumResultCheck;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
