using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace Ferretto.VW.App.Controls.Controls
{
    public class BaseMenuButton : Button
    {
        #region Fields

        public static readonly DependencyProperty AbbreviationProperty =
                    DependencyProperty.Register(
                nameof(Abbreviation),
                typeof(string),
                typeof(BaseMenuButton),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                nameof(ImageSource),
                typeof(ImageSource),
                typeof(BaseMenuButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(BaseMenuButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(
                nameof(IsBusy),
                typeof(bool),
                typeof(BaseMenuButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(
                nameof(Kind),
                typeof(PackIconMaterialKind),
                typeof(BaseMenuButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MenuBrushProperty =
                                                    DependencyProperty.Register(
                nameof(MenuBrush),
                typeof(Brush),
                typeof(BaseMenuButton),
                new PropertyMetadata(Brushes.Green));

        #endregion

        #region Constructors

        public BaseMenuButton()
        {
        }

        #endregion

        #region Properties

        public string Abbreviation
        {
            get { return (string)this.GetValue(AbbreviationProperty); }
            set { this.SetValue(AbbreviationProperty, value); }
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public bool IsActive
        {
            get => (bool)this.GetValue(IsActiveProperty);
            set => this.SetValue(IsActiveProperty, value);
        }

        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }

        public PackIconMaterialKind Kind
        {
            get => (PackIconMaterialKind)this.GetValue(KindProperty);
            set => this.SetValue(KindProperty, value);
        }

        public Brush MenuBrush
        {
            get { return (Brush)this.GetValue(MenuBrushProperty); }
            set { this.SetValue(MenuBrushProperty, value); }
        }

        #endregion
    }
}
