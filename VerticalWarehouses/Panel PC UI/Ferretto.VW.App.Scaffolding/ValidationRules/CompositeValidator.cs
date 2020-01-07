using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class CompositeValidator : DependencyObject
    {
        public static readonly DependencyProperty RulesProperty = DependencyProperty.Register("Rules", typeof(ObservableCollection<ValidationRule>), typeof(CompositeValidator));

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
