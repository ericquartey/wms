using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using System;
using Ferretto.VW.App.Resources;
using CommonServiceLocator;
using System.ComponentModel;
using MahApps.Metro.IconPacks;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcButton : Button
    {
        #region Fields

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(PpcButton), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnContentChanged)));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(PpcButton), new PropertyMetadata(false));

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy), typeof(bool), typeof(PpcButton), new PropertyMetadata(false));

        public static readonly DependencyProperty KindFontAwesomeProperty =
            DependencyProperty.Register(nameof(KindFontAwesome), typeof(PackIconFontAwesomeKind?), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialLightKind?), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty PermissionProperty = DependencyProperty.Register(
            nameof(Permission), typeof(UserAccessLevel), typeof(PpcButton), new PropertyMetadata(UserAccessLevel.NoAccess, new PropertyChangedCallback(PermissionChanged)));

        public bool PermissionValue = true;

        private IEventAggregator eventAggregator = null;

        private ISessionService sessionService = null;

        private SubscriptionToken userAccessLevelToken;

        #endregion

        #region Constructors

        static PpcButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PpcButton), new FrameworkPropertyMetadata(typeof(PpcButton)));

            UIElement.VisibilityProperty.AddOwner(typeof(PpcButton), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceVisibilityValue)));
        }

        public PpcButton()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public object Content
        {
            get { return (object)this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        public bool HasKind => !(this.Kind is null);

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

        public PackIconMaterialLightKind? Kind
        {
            get => (PackIconMaterialLightKind?)this.GetValue(KindProperty);
            set => this.SetValue(KindProperty, value);
        }

        public PackIconFontAwesomeKind? KindFontAwesome
        {
            get => (PackIconFontAwesomeKind?)this.GetValue(KindFontAwesomeProperty);
            set => this.SetValue(KindFontAwesomeProperty, value);
        }

        protected bool NoAccess => this.sessionService.UserAccessLevel == UserAccessLevel.NoAccess;

        public UserAccessLevel Permission
        {
            get => (UserAccessLevel)this.GetValue(PermissionProperty);
            set => this.SetValue(PermissionProperty, value);
        }

        protected bool IsAdmin => this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        protected bool IsInstaller => this.sessionService.UserAccessLevel == UserAccessLevel.Installer ||
                                      this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        protected bool IsOperator => this.sessionService.UserAccessLevel == UserAccessLevel.Operator ||
                                                 this.sessionService.UserAccessLevel == UserAccessLevel.Installer ||
                                         this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        #endregion

        #region Methods

        public void OnPermissionChanged()
        {
            if (!(this.sessionService?.UserAccessLevel is null))
            {
                bool condition = false;
                switch (this.Permission)
                {
                    case UserAccessLevel.Operator:
                        condition = this.IsOperator;
                        break;

                    case UserAccessLevel.Installer:
                        condition = this.IsInstaller;
                        break;

                    case UserAccessLevel.Admin:
                        condition = this.IsAdmin;
                        break;

                    case UserAccessLevel.NoAccess:
                        condition = true;
                        break;

                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }

                this.PermissionValue = condition;

                this.OnPropertyChanged(new DependencyPropertyChangedEventArgs(VisibilityProperty, null, this.Visibility));
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();

            this.userAccessLevelToken = this.eventAggregator
                    .GetEvent<UserAccessLevelNotificationPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnPermissionChanged(),
                        ThreadOption.UIThread,
                        false);

            this.Loaded += (s, ev) =>
            {
                this.OnPermissionChanged();
            };

            this.Unloaded += (s, ev) =>
            {
                this.userAccessLevelToken?.Dispose();
                this.userAccessLevelToken = null;
            };
        }

        private static object CoerceVisibilityValue(DependencyObject d, object value)
        {
            if ((Visibility)value != Visibility.Visible)
            {
                return value;
            }

            if (d is PpcButton button
                && button.PermissionValue)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcButton button
                && button.Content is string text
                && !string.IsNullOrEmpty(text)
                && text.Equals(InstallationApp.Stop))
            {
                button.Style = Application.Current.Resources["PpcButtonStopStyle"] as Style;
            }
        }

        private static void PermissionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcButton ppcButton)
            {
                ppcButton.OnPermissionChanged();
            }
        }

        #endregion
    }
}
