using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.App.Scaffolding.Controls
{
    /// <summary>
    /// Interaction logic for PropertyEditor.xaml
    /// </summary>
    public partial class PropertyEditor : UserControl
    {
        public PropertyEditor()
        {
            this.InitializeComponent();
        }

        #region Plain fields

        private IEnumerable<ValidationRule> _validationRules = Array.Empty<ValidationRule>();

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty EntityProperty
            = DependencyProperty.Register("Entity", typeof(Models.ScaffoldedEntity), typeof(PropertyEditor), new PropertyMetadata(OnEntityPropertyChanged));

        private static void OnEntityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyEditor)d).OnEntityChanged(e);
        }

        private void OnEntityChanged(DependencyPropertyChangedEventArgs e)
        {
            var entity = (Models.ScaffoldedEntity)e.NewValue;
            if (entity == null)
            {
                return;
            }

            // 1. validation rules
            this._validationRules = entity.ExtractValidationRules();

            // 2. other assignments
            object originalValue = null;
            if (entity.Instance != null)
            {
                originalValue = entity.Property.GetValue(entity.Instance);
            }
            this.OriginalValue = originalValue;
            var type = entity.Property.PropertyType;

            // assign value
            if (type.IsValueType)
            {
                // just copy
                this.Value = this.OriginalValue;
            }
            else if (type.IsSerializable)
            {
                if (originalValue != null)
                {
                    // deep copy
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(ms, originalValue);
                        ms.Position = 0;
                        this.Value = formatter.Deserialize(ms);
                    }
                }
                else
                {
                    this.Value = null;
                }
            }
            else
            {
                throw new ArgumentException($"{type} is not a value type neither a serializable type.");
            }
        }

        public Models.ScaffoldedEntity Entity
        {
            get => (Models.ScaffoldedEntity)this.GetValue(EntityProperty);
            set => this.SetValue(EntityProperty, value);
        }

        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register("Value", typeof(object), typeof(PropertyEditor), new PropertyMetadata(OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyEditor)d).OnValueChanged(e);
        }

        private void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            this.IsDirty = e.NewValue != this.OriginalValue;

            // validation
            var currentCulture = System.Globalization.CultureInfo.CurrentCulture;
            var results = this._validationRules.Select(r => r.Validate(e.NewValue, currentCulture)).Where(r => r.IsValid == false);
            this.IsValid = !results.Any();
            this.ValidationMessage = results.Select(r => r.ErrorContent).FirstOrDefault();
        }

        public object Value
        {
            get => this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty OriginalValueProperty
            = DependencyProperty.Register("OriginalValue", typeof(object), typeof(PropertyEditor));

        public object OriginalValue
        {
            get => this.GetValue(OriginalValueProperty);
            set => this.SetValue(OriginalValueProperty, value);
        }

        public static readonly DependencyProperty IsDirtyProperty
            = DependencyProperty.Register("IsDirty", typeof(bool), typeof(PropertyEditor));

        public bool IsDirty
        {
            get => (bool)this.GetValue(IsDirtyProperty);
            set => this.SetValue(IsDirtyProperty, value);
        }

        public static readonly DependencyProperty IsValidProperty
            = DependencyProperty.Register("IsValid", typeof(bool), typeof(PropertyEditor));

        public bool IsValid
        {
            get => (bool)this.GetValue(IsValidProperty);
            set => this.SetValue(IsValidProperty, value);
        }

        public static readonly DependencyProperty ValidationMessageProperty
            = DependencyProperty.Register("ValidationMessage", typeof(object), typeof(PropertyEditor));
        
        public object ValidationMessage
        {
            get => this.GetValue(ValidationMessageProperty);
            set => this.SetValue(ValidationMessageProperty, value);
        }

        #endregion

        #region Events + Handlers

        public event EventHandler<CommitEventArgs> Commit;

        protected void OnCommit(CommitEventArgs e)
        {
            this.Commit?.Invoke(this, e);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.OnCommit(new CommitEventArgs(this.OriginalValue));
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.OnCommit(new CommitEventArgs(this.Value));
        }

        private void CompositeValidationRule_Validated(object sender, ValidationRules.ValidationEventArgs e)
        {
            this.IsValid = e.IsValid;
            this.ValidationMessage = e.ErrorContent;
        }

        #endregion
    }
}
