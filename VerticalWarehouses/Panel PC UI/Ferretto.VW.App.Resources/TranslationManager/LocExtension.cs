using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace Ferretto.VW.App.Resources
{
    public class LocExtension : MarkupExtension

    {
        #region Constructors

        public LocExtension(string stringName)
        {
            this.StringName = stringName;
        }

        #endregion

        #region Properties

        public string StringName { get; }

        #endregion

        #region Methods

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Binding binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath($"[{this.StringName}]"),
                Source = LocRes.Instance,
                FallbackValue = StringName
            };

            return binding.ProvideValue(serviceProvider);
        }

        #endregion
    }
}
