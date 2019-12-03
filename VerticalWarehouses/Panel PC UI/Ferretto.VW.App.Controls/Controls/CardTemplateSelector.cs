﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CardTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate EnumAxis { get; set; }

        public DataTemplate EnumBay { get; set; }

        public DataTemplate EnumDrawer { get; set; }

        public DataTemplate EnumPosition { get; set; }

        public DataTemplate EnumShutter { get; set; }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            CardSensor.CardType value = (CardSensor.CardType)(item ?? CardSensor.CardType.Axis);
            switch (value)
            {
                case CardSensor.CardType.Axis:
                    return this.EnumAxis;

                case CardSensor.CardType.Bay:
                    return this.EnumBay;

                case CardSensor.CardType.Drawer:
                    return this.EnumDrawer;

                case CardSensor.CardType.Position:
                    return this.EnumPosition;

                case CardSensor.CardType.Shutter:
                    return this.EnumShutter;
            }

            return base.SelectTemplate(item, container);
        }

        #endregion
    }
}
