using System;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Controls.Controls;
using Microsoft.Xaml.Behaviors;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public abstract class PpcTextBoxBehavior : Behavior<PpcTextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            if (!this.AssociatedObject.IsLoaded)
            {
                this.AssociatedObject.Loaded += this.AssociatedObject_Loaded;
            }
            else
            {
                this.FindTextBox();
            }
        }

        private void FindTextBox()
        {
            var box = this.TextBox = this.AssociatedObject.FindTextBox();
            if (box == null)
            {
                throw new ApplicationException($"Cannot find {typeof(TextBox)} in {typeof(PpcTextBox)}. A name-change might have occurred.");
            }
            this.OnTextBoxAttached();
        }

        protected virtual void OnTextBoxAttached()
        {
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            this.AssociatedObject.Loaded -= this.AssociatedObject_Loaded;
            this.FindTextBox();
        }

        protected TextBox TextBox { get; private set; }

        public static readonly DependencyProperty IsEnabledProperty
                    = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(PpcTextBoxFilterBehavior), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets whether this behavior is enabled or not.
        /// </summary>
        public bool IsEnabled
        {
            get => (bool)this.GetValue(IsEnabledProperty);
            set => this.SetValue(IsEnabledProperty, value);
        }
    }
}
