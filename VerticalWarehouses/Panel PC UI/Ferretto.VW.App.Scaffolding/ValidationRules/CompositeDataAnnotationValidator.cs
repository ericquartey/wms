using System.Collections.ObjectModel;
using System.Windows;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class CompositeDataAnnotationValidator : DependencyObject
    {
        public static readonly DependencyProperty AttributesProperty = DependencyProperty.Register("Attributes", typeof(ObservableCollection<AttributeValidationRule>), typeof(CompositeDataAnnotationValidator));

        public ObservableCollection<AttributeValidationRule> Attributes
        {
            get => (ObservableCollection<AttributeValidationRule>)this.GetValue(AttributesProperty);
            set => this.SetValue(AttributesProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(CompositeDataAnnotationValidator));

        public object Value
        {
            get => this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}
