using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class RulesChangedEventArgs
    {
        internal RulesChangedEventArgs(ObservableCollection<ValidationRule> oldRules, ObservableCollection<ValidationRule> currentRules)
        {
            this.OldRules = oldRules;
            this.NewRules = currentRules;
        }

        public ObservableCollection<ValidationRule> OldRules { get; }
        public ObservableCollection<ValidationRule> NewRules { get; }
    }

    public class CompositeValidator : DependencyObject
    {
        public static readonly DependencyProperty RulesProperty
            = DependencyProperty.Register("Rules", typeof(ObservableCollection<ValidationRule>), typeof(CompositeValidator), new PropertyMetadata(OnRulesPropertyChanged));

        private static void OnRulesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CompositeValidator)d).OnRulesChanged(new RulesChangedEventArgs(e.OldValue as ObservableCollection<ValidationRule>, e.NewValue as ObservableCollection<ValidationRule>));
        }

        protected virtual void OnRulesChanged(RulesChangedEventArgs e)
        {
            this.RulesChanged?.Invoke(this, e);
        }

        public event EventHandler<RulesChangedEventArgs> RulesChanged;

        public ObservableCollection<ValidationRule> Rules
        {
            get => (ObservableCollection<ValidationRule>)this.GetValue(RulesProperty);
            set => this.SetValue(RulesProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(CompositeValidator));

        public object Value
        {
            get => this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}
