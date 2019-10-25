using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    public class PpcControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items),
            typeof(IEnumerable<IPresentation>),
            typeof(PpcControl),
            new PropertyMetadata(default(IEnumerable<IPresentation>), ItemsChanged));

        public static readonly DependencyProperty PresentationTypeProperty = DependencyProperty.Register(
            nameof(PresentationType),
            typeof(PresentationTypes),
            typeof(PpcControl),
            new PropertyMetadata(default(PresentationTypes), PresentationTypeChanged));

        #endregion

        #region Constructors

        public PpcControl()
        {
            this.Loaded += async (sender, e) => await this.OnViewLoadedAsync(sender, e);
        }

        #endregion

        #region Properties

        public IEnumerable<IPresentation> Items
        {
            get => (IEnumerable<IPresentation>)this.GetValue(ItemsProperty);
            set => this.SetValue(ItemsProperty, value);
        }

        public PresentationTypes PresentationType
        {
            get => (PresentationTypes)this.GetValue(PresentationTypeProperty);
            set => this.SetValue(PresentationTypeProperty, value);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            if (this.DataContext is BasePresentationViewModel)
            {
                return;
            }

            if (this.DataContext is IEnumerable<IPresentation> items)
            {
                this.Items = items;
            }

            if (this.Items != null
                &&
                this.PresentationType != PresentationTypes.None)
            {
                var presentationStateFound = this.Items.FirstOrDefault(i => i.Type == this.PresentationType);
                if (presentationStateFound != null)
                {
                    this.DataContext = presentationStateFound;
                }
            }
        }

        private static void ItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcControl control)
            {
                control.Initialize();
            }
        }

        private static void PresentationTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcControl control)
            {
                control.Initialize();
            }
        }

        private async Task OnViewLoadedAsync(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BasePresentationViewModel basePresentation)
            {
                await basePresentation.OnLoadedAsync();
            }
        }

        #endregion
    }
}
