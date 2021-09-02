using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommonServiceLocator;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using MahApps.Metro.IconPacks;
using Prism.Events;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcButton : Button
    {
        #region Fields

        public static readonly DependencyProperty AbbrevationProperty =
            DependencyProperty.Register(nameof(Abbrevation), typeof(string), typeof(PpcButton), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnAbbrevationChanged)));

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(PpcButton), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnContentChanged)));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(PpcButton), new PropertyMetadata(false));

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy), typeof(bool), typeof(PpcButton), new PropertyMetadata(false));

        public static readonly DependencyProperty IsBypassedProperty = DependencyProperty.Register(
           nameof(IsBypassed), typeof(bool), typeof(PpcButton), new PropertyMetadata(false));

        public static readonly DependencyProperty IsCompletedProperty = DependencyProperty.Register(
                    nameof(IsCompleted), typeof(bool), typeof(PpcButton), new PropertyMetadata(false));

        public static readonly DependencyProperty KindFontAwesomeProperty =
            DependencyProperty.Register(nameof(KindFontAwesome), typeof(PackIconFontAwesomeKind?), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty KindMaterialProperty =
            DependencyProperty.Register(nameof(KindMaterial), typeof(PackIconMaterialKind?), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty KindModernProperty =
            DependencyProperty.Register(nameof(KindModern), typeof(PackIconModernKind?), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty KindProperty =
                    DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialLightKind?), typeof(PpcButton), new PropertyMetadata(null));

        public static readonly DependencyProperty PermissionProperty = DependencyProperty.Register(
            nameof(Permission), typeof(UserAccessLevel), typeof(PpcButton), new PropertyMetadata(UserAccessLevel.NoAccess, new PropertyChangedCallback(PermissionChanged)));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PpcButton), new PropertyMetadata(string.Empty));

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty VisibilityPermissionProperty =
            DependencyProperty.Register(nameof(VisibilityPermission), typeof(Visibility), typeof(PpcButton), new PropertyMetadata(Visibility.Visible));

        public bool PermissionValue = true;

        //public static readonly DependencyProperty VisibilityProperty =
        //    DependencyProperty.Register(nameof(Visibility), typeof(Visibility), typeof(PpcButton), new FrameworkPropertyMetadata(Visibility.Visible));
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

        public string Abbrevation
        {
            get { return (string)this.GetValue(AbbrevationProperty); }
            set { this.SetValue(AbbrevationProperty, value); }
        }

        public object Content
        {
            get { return (object)this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        public bool HasKind => !(this.Kind is null);

        public bool HasKindFontAwesome => !(this.KindFontAwesome is null);

        public bool HasKindMaterial => !(this.KindMaterial is null);

        public bool HasKindModern => !(this.KindModern is null);

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

        public bool IsBypassed
        {
            get => (bool)this.GetValue(IsBypassedProperty);
            set => this.SetValue(IsBypassedProperty, value);
        }

        public bool IsCompleted
        {
            get => (bool)this.GetValue(IsCompletedProperty);
            set => this.SetValue(IsCompletedProperty, value);
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

        public PackIconMaterialKind? KindMaterial
        {
            get => (PackIconMaterialKind?)this.GetValue(KindMaterialProperty);
            set => this.SetValue(KindMaterialProperty, value);
        }

        public PackIconModernKind? KindModern
        {
            get => (PackIconModernKind?)this.GetValue(KindModernProperty);
            set => this.SetValue(KindModernProperty, value);
        }

        protected bool NoAccess => this.sessionService.UserAccessLevel == UserAccessLevel.NoAccess;

        public UserAccessLevel Permission
        {
            get => (UserAccessLevel)this.GetValue(PermissionProperty);
            set => this.SetValue(PermissionProperty, value);
        }

        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        public Visibility VisibilityPermission
        {
            get { return (Visibility)this.GetValue(VisibilityPermissionProperty); }
            set { this.SetValue(VisibilityPermissionProperty, value); }
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
                var condition = false;
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

                if (this.Visibility == Visibility.Visible
                    && this.PermissionValue)
                {
                    this.VisibilityPermission = Visibility.Visible;
                }
                else
                {
                    this.VisibilityPermission = Visibility.Collapsed;
                }
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
            var button = d as PpcButton;

            if (((Visibility)value == Visibility.Visible)
                && button.PermissionValue)
            {
                button.VisibilityPermission = Visibility.Visible;
            }
            else
            {
                button.VisibilityPermission = Visibility.Collapsed;
            }

            return value;
        }

        private static void OnAbbrevationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
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
