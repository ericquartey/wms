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
            DependencyProperty.Register("Content", typeof(object), typeof(PpcButton), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnContentChanged)));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(PpcButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(PpcButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty KindFontAwesomeProperty =
            DependencyProperty.Register(nameof(KindFontAwesome), typeof(PackIconFontAwesomeKind?), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialLightKind?), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty PermissionProperty = DependencyProperty.Register(
            nameof(Permission),
            typeof(UserAccessLevel),
            typeof(PpcButton),
            new PropertyMetadata(UserAccessLevel.NoAccess, new PropertyChangedCallback(PermissionChanged)));

        private IEventAggregator eventAggregator = null;

        private ISessionService sessionService = null;

        private SubscriptionToken userAccessLevelToken;

        private bool? visibleOldStatus;

        #endregion

        #region Constructors

        static PpcButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PpcButton), new FrameworkPropertyMetadata(typeof(PpcButton)));
        }

        public PpcButton()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Initialization();
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

        public void PermissionChanged()
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

                if ((condition && this.Visibility != Visibility.Visible) ||
                    (!condition && this.Visibility == Visibility.Visible))
                {
                    // salvo lo stato precedente
                    if (!this.visibleOldStatus.HasValue && this.Visibility == Visibility.Visible && !condition)
                    {
                        this.visibleOldStatus = this.Visibility == Visibility.Visible;
                    }

                    // setto la visibilità in base alla consizione
                    if (condition)
                    {
                        this.Visibility = !this.visibleOldStatus.HasValue || this.visibleOldStatus.Value ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else if (!condition)
                    {
                        this.Visibility = Visibility.Collapsed;
                    }

                    // relsetto lo stato vecchio
                    if (condition)
                    {
                        this.visibleOldStatus = null;
                    }
                };
            }
        }

        public void VisibilityChange()
        {
            if (!this.visibleOldStatus.HasValue &&
                this.Visibility == Visibility.Visible)
            {
                this.visibleOldStatus = this.Visibility == Visibility.Visible;
            }
            this.Visibility = Visibility.Visible;
        }

        public void VisibilityRestore()
        {
            if (this.Visibility != Visibility.Visible &&
                this.visibleOldStatus.HasValue &&
                this.Permission != UserAccessLevel.NoAccess)
            {
                this.Visibility = this.visibleOldStatus.Value ? Visibility.Visible : Visibility.Collapsed;
                this.visibleOldStatus = null;
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
                ppcButton.PermissionChanged();
            }
        }

        private void Initialization()
        {
            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();

            this.userAccessLevelToken = this.eventAggregator
                    .GetEvent<UserAccessLevelNotificationPubSubEvent>()
                    .Subscribe(
                        (m) => this.PermissionChanged(),
                        ThreadOption.UIThread,
                        false);

            this.Loaded += (s, e) =>
            {
                this.PermissionChanged();
            };

            this.Unloaded += (s, e) =>
            {
                this.userAccessLevelToken?.Dispose();
                this.userAccessLevelToken = null;
            };
        }

        #endregion
    }
}
