using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Keyboards;
using Microsoft.Xaml.Behaviors;

namespace Ferretto.VW.App.Controls.Behaviors
{
    public abstract class KeyboardPopupBehaviorBase<T> : Behavior<T>
        where T : Control
    {
        #region Fields

        public static readonly DependencyProperty InactiveTimeoutProperty =
                            DependencyProperty.Register(nameof(InactiveTimeout), typeof(TimeSpan), typeof(KeyboardPopupBehaviorBase<T>));

        public static readonly DependencyProperty IsDoubleClickTriggerEnabledProperty
                            = DependencyProperty.RegisterAttached(nameof(IsDoubleClickTriggerEnabled), typeof(bool), typeof(KeyboardPopupBehaviorBase<T>), new PropertyMetadata(true));

        public static readonly DependencyProperty KeyboardLabelProperty =
                    DependencyProperty.Register(nameof(KeyboardLabel), typeof(string), typeof(KeyboardPopupBehaviorBase<T>));

        public static readonly DependencyProperty KeyboardLayoutCodeProperty =
                            DependencyProperty.Register(nameof(KeyboardLayoutCode), typeof(string), typeof(KeyboardPopupBehaviorBase<T>), new PropertyMetadata("lowercase"));

        public static readonly DependencyProperty ValueTypeProperty =
                            DependencyProperty.Register(nameof(ValueType), typeof(Type), typeof(KeyboardPopupBehaviorBase<T>));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the dependency property to be handled.
        /// </summary>
        public DependencyProperty DependencyProperty
        {
            get; set;
        }

        public TimeSpan InactiveTimeout
        {
            get => (TimeSpan)this.GetValue(InactiveTimeoutProperty);
            set => this.SetValue(InactiveTimeoutProperty, value);
        }

        public bool IsDoubleClickTriggerEnabled
        {
            get => (bool)this.GetValue(IsDoubleClickTriggerEnabledProperty);
            set => this.SetValue(IsDoubleClickTriggerEnabledProperty, value);
        }

        public string KeyboardLabel
        {
            get => (string)this.GetValue(KeyboardLabelProperty);
            set => this.SetValue(KeyboardLabelProperty, value);
        }

        public string KeyboardLayoutCode
        {
            get => (string)this.GetValue(KeyboardLayoutCodeProperty);
            set => this.SetValue(KeyboardLayoutCodeProperty, value);
        }

        /// <summary>
        /// Gets or sets the
        /// </summary>
        public Type ValueType
        {
            get => (Type)this.GetValue(ValueTypeProperty);
            set => this.SetValue(ValueTypeProperty, value);
        }

        protected virtual Control AssociatedObjectHandle => this.AssociatedObject;

        #endregion

        #region Methods

        protected virtual bool IsKeyboardEnabled(Control ctrl) => ctrl?.IsEnabled == true;

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObjectHandle.TouchUp += this.Control_TouchUp;
            this.AssociatedObjectHandle.MouseDoubleClick += this.Control_MouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObjectHandle.MouseDoubleClick -= this.Control_MouseDoubleClick;
            this.AssociatedObjectHandle.TouchUp -= this.Control_TouchUp;
            base.OnDetaching();
        }

        protected virtual void OpenKeyboard()
        {
            // show keyboard
            this.AssociatedObjectHandle.PopupKeyboard(this.DependencyProperty, this.ValueType ?? typeof(object), this.KeyboardLayoutCode, this.KeyboardLabel, this.InactiveTimeout);
        }

        private void Control_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.IsDoubleClickTriggerEnabled)
            {
                this.OpenKeyboardConditional();
            }
        }

        private void Control_TouchUp(object sender, TouchEventArgs e)
        {
            // e.Handled = true;
            this.Dispatcher.BeginInvoke(new Action(this.OpenKeyboardConditional));
        }

        private void OpenKeyboardConditional()
        {
            if (this.IsKeyboardEnabled(this.AssociatedObjectHandle))
            {
                this.OpenKeyboard();
            }
        }

        #endregion
    }
}
