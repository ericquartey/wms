using System;
using System.Windows.Data;
using System.Windows.Markup;
using Ferretto.VW.App.Controls.Converters;

namespace Ferretto.VW.App.Controls
{
    public class ConverterBindableBinding : MarkupExtension
    {
        #region Properties

        public Binding Binding { get; set; }

        public IValueConverter Converter { get; set; }

        public Binding ConverterParameterBinding { get; set; }

        #endregion

        #region Methods

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(this.Binding);
            multiBinding.Bindings.Add(this.ConverterParameterBinding);
            var adapter = new MultiValueConverterAdapter();
            adapter.Converter = this.Converter;
            multiBinding.Converter = adapter;
            return multiBinding.ProvideValue(serviceProvider);
        }

        #endregion
    }
}
