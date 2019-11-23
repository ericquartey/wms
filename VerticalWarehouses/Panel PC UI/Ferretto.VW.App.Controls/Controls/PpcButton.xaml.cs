using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcButton : Button
    {
        #region Fields

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(PpcButton));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(PpcButton));

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(PpcButton));

        public static readonly DependencyProperty PermitionProperty = DependencyProperty.Register(
            nameof(Permition),
            typeof(UserAccessLevel),
            typeof(PpcButton),
            new PropertyMetadata(UserAccessLevel.Operator, new PropertyChangedCallback(OnPermitionChanged)));

        private readonly IEventAggregator eventAggregator = CommonServiceLocator.ServiceLocator.Current.GetInstance<IEventAggregator>();

        private readonly ISessionService sessionService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISessionService>();

        private readonly SubscriptionToken userAccessLevelToken;

        #endregion

        #region Constructors

        public PpcButton()
        {
            this.InitializeComponent();

            this.userAccessLevelToken = this.eventAggregator
                    .GetEvent<UserAccessLevelNotificationPubSubEvent>()
                    .Subscribe(
                        ((m) => this.PermitionChanged()),
                        ThreadOption.UIThread,
                        false);
        }

        #endregion

        #region Properties

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

        protected bool NoAccess => this.sessionService.UserAccessLevel == UserAccessLevel.NoAccess;

        public UserAccessLevel Permition
        {
            get => (UserAccessLevel)this.GetValue(PermitionProperty);
            set => this.SetValue(PermitionProperty, value);
        }

        protected bool IsAdmin => this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        protected bool IsInstaller => this.sessionService.UserAccessLevel == UserAccessLevel.Installer ||
                                      this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        protected bool IsOperator => this.sessionService.UserAccessLevel == UserAccessLevel.Operator ||
                                                 this.sessionService.UserAccessLevel == UserAccessLevel.Installer ||
                                         this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        #endregion

        #region Methods

        public void PermitionChanged()
        {
            if (!(this.sessionService?.UserAccessLevel is null))
            {
                switch (this.Permition)
                {
                    case UserAccessLevel.Installer:
                        this.Visibility = this.Visibility == Visibility.Visible && this.IsInstaller ? Visibility.Visible : Visibility.Collapsed;
                        break;

                    case UserAccessLevel.Admin:
                        this.Visibility = this.Visibility == Visibility.Visible && this.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
                        break;

                    case UserAccessLevel.Operator:
                        this.Visibility = this.Visibility == Visibility.Visible && this.IsOperator ? Visibility.Visible : Visibility.Collapsed;
                        break;

                    case UserAccessLevel.NoAccess:
                        break;

                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }
            }
        }

        private static void OnPermitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcButton ppcButton)
            {
                ppcButton.PermitionChanged();
            }
        }

        #endregion
    }
}
