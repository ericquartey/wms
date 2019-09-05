using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationTheme : BasePresentation
    {
        #region Fields

        private readonly IThemeService themeService;

        #endregion

        #region Constructors

        public PresentationTheme(IThemeService themeService)
            : base(PresentationTypes.Theme)
        {
            if (themeService is null)
            {
                throw new System.ArgumentNullException(nameof(themeService));
            }

            this.themeService = themeService;
        }

        #endregion

        #region Properties

        public bool IsDarkThemeActive => this.themeService.ActiveTheme == ApplicationTheme.Dark;

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.themeService.ApplyTheme(
                this.themeService.ActiveTheme == ApplicationTheme.Light
                    ? ApplicationTheme.Dark
                    : ApplicationTheme.Light);

            this.RaisePropertyChanged(nameof(this.IsDarkThemeActive));

            return Task.CompletedTask;
        }

        #endregion
    }
}
