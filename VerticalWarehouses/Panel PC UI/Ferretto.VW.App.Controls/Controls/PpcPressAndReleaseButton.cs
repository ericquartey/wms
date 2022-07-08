using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.IconPacks;

namespace Ferretto.VW.App.Controls.Controls
{
    public class PpcPressAndReleaseButton : PpcButton
    {
        #region Fields

        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialKind?), typeof(PpcPressAndReleaseButton), new PropertyMetadata(null));

        public static readonly DependencyProperty PressCommandProperty =
            DependencyProperty.Register(nameof(PressCommand), typeof(ICommand), typeof(PpcPressAndReleaseButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ReleaseCommandProperty =
            DependencyProperty.Register(nameof(ReleaseCommand), typeof(ICommand), typeof(PpcPressAndReleaseButton), new PropertyMetadata(null));

        #endregion

        #region Constructors

        public PpcPressAndReleaseButton()
        {
            this.PreviewMouseLeftButtonUp += (o, a) =>
            {
                this.OnButtonUp();
            };
            this.TouchLeave += (o, a) =>
            {
                this.OnButtonUp();
            };
            this.TouchUp += (o, a) =>
            {
                this.OnButtonUp();
            };

            this.PreviewMouseLeftButtonDown += (o, a) =>
            {
                this.OnButtonDown();
            };
            this.TouchEnter += (o, a) =>
            {
                this.OnButtonDown();
            };

            this.Style = Application.Current.FindResource("PpcPressAndReleaseButtonStyle") as Style;
        }

        #endregion

        #region Properties

        public bool HasKind => !(this.Kind is null);

        public PackIconMaterialKind? Kind
        {
            get => (PackIconMaterialKind?)this.GetValue(KindProperty);
            set => this.SetValue(KindProperty, value);
        }

        public ICommand PressCommand
        {
            get => (ICommand)this.GetValue(PressCommandProperty);
            set => this.SetValue(PressCommandProperty, value);
        }

        public ICommand ReleaseCommand
        {
            get => (ICommand)this.GetValue(ReleaseCommandProperty);
            set => this.SetValue(ReleaseCommandProperty, value);
        }

        #endregion

        #region Methods

        private void OnButtonDown()
        {
            Debug.WriteLine($"Pressed");

            if (this.PressCommand?.CanExecute(null) == true)
            {
                this.PressCommand.Execute(null);
            }
        }

        private void OnButtonUp()
        {
            Debug.WriteLine($"Release");

            this.ReleaseCommand.Execute(null);
        }

        #endregion
    }
}
