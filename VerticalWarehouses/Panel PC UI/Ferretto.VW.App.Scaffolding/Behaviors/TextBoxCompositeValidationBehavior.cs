using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ferretto.VW.App.Scaffolding.ValidationRules;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public class TextBoxCompositeValidationBehavior : TextBoxValidationBehavior
    {

        public static readonly DependencyProperty CompositeValidatorProperty
            = DependencyProperty.Register(nameof(CompositeValidator), typeof(CompositeValidator), typeof(TextBoxCompositeValidationBehavior), new PropertyMetadata(OnCompositeValidatorPropertyChanged));

        private static void OnCompositeValidatorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextBoxCompositeValidationBehavior)d).OnCompositeValidatorChanged(e);
        }

        private void OnCompositeValidatorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CompositeValidator old)
            {
                old.RulesChanged -= this.CompositeValidator_RulesChanged;
                if (old.Rules != null)
                {
                    this.RemoveRules(old.Rules);
                    old.Rules.CollectionChanged -= this.Rules_CollectionChanged;
                }
            }
            if (e.NewValue is CompositeValidator val)
            {
                val.RulesChanged += this.CompositeValidator_RulesChanged;
                if (val.Rules != null)
                {
                    this.AddRules(val.Rules);
                    val.Rules.CollectionChanged += this.Rules_CollectionChanged;
                }
            }
        }

        private Binding GetTextBinding()
            => this.AssociatedObject.GetBindingExpression(TextBox.TextProperty)?.ParentBinding;

        private void CompositeValidator_RulesChanged(object sender, RulesChangedEventArgs e)
        {
            if (e.OldRules != null)
            {
                e.OldRules.CollectionChanged -= this.Rules_CollectionChanged;
            }
            if (e.NewRules != null)
            {
                e.NewRules.CollectionChanged += this.Rules_CollectionChanged;
            }
        }

        private void Rules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                this.RemoveRules(e.OldItems.OfType<ValidationRule>());
            }
            if (e.NewItems?.Count > 0)
            {
                this.AddRules(e.NewItems.OfType<ValidationRule>());
            }
        }

        private void RemoveRules(IEnumerable<ValidationRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }
            var bindingRules = this.GetTextBinding()?.ValidationRules;
            if (bindingRules != null)
            {
                foreach (var rule in rules)
                {
                    if (bindingRules.IndexOf(rule) >= 0)
                    {
                        bindingRules.Remove(rule);
                    }
                }
            }
        }

        private void AddRules(IEnumerable<ValidationRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }
            var bindingRules = this.GetTextBinding()?.ValidationRules;
            if (bindingRules != null)
            {
                foreach (var rule in rules)
                {
                    if (bindingRules.IndexOf(rule) == -1)
                    {
                        bindingRules.Add(rule);
                    }
                }
            }
        }

        public CompositeValidator CompositeValidator
        {
            get => (CompositeValidator)this.GetValue(CompositeValidatorProperty);
            set => this.SetValue(CompositeValidatorProperty, value);
        }

        public override bool Validate(string text)
        {
            var rules = this.CompositeValidator?.Rules;
            if (rules != null)
            {
                CultureInfo culture = this.GetTextBinding()?.ConverterCulture ?? CultureInfo.CurrentCulture;
                foreach (var rule in rules)
                {
                    if (!rule.Validate(text, culture).IsValid)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
