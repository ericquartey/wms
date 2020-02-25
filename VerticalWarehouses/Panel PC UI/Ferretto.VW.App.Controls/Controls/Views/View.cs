using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    public class View : UserControl, INavigableView
    {
        #region Fields

        public static readonly DependencyProperty ParentModuleNameProperty = DependencyProperty.Register(nameof(ParentModuleName), typeof(string), typeof(View));

        public static readonly DependencyProperty ParentViewNameProperty = DependencyProperty.Register(nameof(ParentViewName), typeof(string), typeof(View));

        #endregion

        #region Constructors

        public View()
        {
            this.Loaded += async (sender, e) => await this.View_Loaded(sender, e);
        }

        #endregion

        #region Properties

        public string ParentModuleName
        {
            get => (string)this.GetValue(ParentModuleNameProperty);
            set => this.SetValue(ParentModuleNameProperty, value);
        }

        public string ParentViewName
        {
            get => (string)this.GetValue(ParentViewNameProperty);
            set => this.SetValue(ParentViewNameProperty, value);
        }

        #endregion

        #region Methods

        private async Task View_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is IActivationViewModel activationViewModel)
            {
                this.SetBinding(
                    IsEnabledProperty,
                    new Binding(nameof(IActivationViewModel.IsEnabled)) { Source = activationViewModel });

                this.SetBinding(
                    VisibilityProperty,
                    new Binding(nameof(IActivationViewModel.IsVisible)) { Source = activationViewModel, Converter = new BooleanToVisibilityConverter() });
            }

            if (this.DataContext is INavigableViewModel viewModel)
            {
                try
                {
                    await viewModel.OnAppearedAsync();
                }
                catch (System.Exception ex)
                {
                    NLog.LogManager
                        .GetCurrentClassLogger()
                        .Error(ex, $"An error occurred while opening view '{this.GetType().Name}'.");
                }
            }
        }

        #endregion
    }
}
