using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Ferretto.VW.App.Controls.Behaviors
{
    public abstract class TogglableBehavior<T> : Behavior<T> where T : DependencyObject
    {
        #region Fields

        public static readonly DependencyProperty IsEnabledProperty
                    = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TogglableBehavior<T>), new PropertyMetadata(true));

        #endregion

        #region Properties

        public bool IsEnabled
        {
            get => (bool)this.GetValue(IsEnabledProperty);
            set => this.SetValue(IsEnabledProperty, value);
        }

        #endregion
    }
}
