using System;
using System.Windows;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    internal class ThemeService : IThemeService
    {
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
                    themeDictionary.Source = new Uri(
                        "/Ferretto.VW.App.Controls;Component/Skins/LightSkin.xaml",
                        UriKind.Relative);
                    break;

                case ApplicationTheme.Dark:
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

        #endregion
    }
}
