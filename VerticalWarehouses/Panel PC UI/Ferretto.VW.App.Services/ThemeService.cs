using System;
using System.Windows;
using Ferretto.VW.App.Services.Models;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class ThemeService : IThemeService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public ThemeService(
            IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));

            this.ApplyDXTheme(this.ActiveTheme);
        }

        #endregion

        #region Properties

        public ApplicationTheme ActiveTheme { get; private set; } = ApplicationTheme.Dark;

        #endregion

        #region Methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Minor Code Smell",
            "S1075:URIs should not be hardcoded",
            Justification = "Extract URIs")]
        public void ApplyTheme(ApplicationTheme theme)
        {
            if (this.ActiveTheme == theme)
            {
                return;
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            var themeDictionary = new ResourceDictionary();

            switch (theme)
            {
                case ApplicationTheme.Light:
                    this.ApplyDXTheme(ApplicationTheme.Light);
                    themeDictionary.Source = new Uri(
                        "/Ferretto.VW.App.Controls;Component/Skins/Light/LightSkin.xaml",
                        UriKind.Relative);
                    break;

                case ApplicationTheme.Dark:
                    this.ApplyDXTheme(ApplicationTheme.Dark);
                    themeDictionary.Source = new Uri(
                         "/Ferretto.VW.App.Controls;Component/Skins/Dark/DarkSkin.xaml",
                         UriKind.Relative);
                    break;

                default:
                    throw new NotSupportedException("The specified theme is not supported.");
            }

            this.ActiveTheme = theme;
            Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
        }

        private void ApplyDXTheme(ApplicationTheme theme)
        {
            switch (theme)
            {
                case ApplicationTheme.Light:
                    DevExpress.Xpf.Core.ApplicationThemeHelper.ApplicationThemeName = VW.Utils.Common.DX_THEME_LIGHT;
                    break;

                case ApplicationTheme.Dark:
                    DevExpress.Xpf.Core.ApplicationThemeHelper.ApplicationThemeName = VW.Utils.Common.DX_THEME_DARK;
                    break;

                default:
                    throw new NotSupportedException("The specified theme is not supported.");
            }

            this.eventAggregator
                .GetEvent<ThemeChangedPubSubEvent>()
                .Publish(new ThemeChangedMessage(this.ActiveTheme));
        }

        #endregion
    }
}
