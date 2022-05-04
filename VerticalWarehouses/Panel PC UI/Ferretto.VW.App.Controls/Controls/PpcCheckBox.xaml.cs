using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for PpcCheckBox.xaml
    /// </summary>
    public partial class PpcCheckBox : UserControl
    {
        #region Fields

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool?), typeof(PpcCheckBox), new PropertyMetadata(false));

        public static readonly DependencyProperty IsThreeStateProperty =
            DependencyProperty.Register(nameof(IsThreeState), typeof(bool), typeof(PpcCheckBox), new PropertyMetadata(false));

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register(nameof(LabelText), typeof(string), typeof(PpcCheckBox), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PermissionProperty =
            DependencyProperty.Register(nameof(Permission), typeof(UserAccessLevel), typeof(PpcCheckBox), new PropertyMetadata(UserAccessLevel.Operator, new PropertyChangedCallback(PermissionChanged)));

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty VisibilityPermissionProperty =
            DependencyProperty.Register(nameof(VisibilityPermission), typeof(Visibility), typeof(PpcCheckBox), new PropertyMetadata(Visibility.Visible));

        public bool PermissionValue = true;

        private IEventAggregator eventAggregator = null;

        private ISessionService sessionService = null;

        private SubscriptionToken userAccessLevelToken;

        #endregion

        #region Constructors

        static PpcCheckBox()
        {
            UIElement.VisibilityProperty.AddOwner(typeof(PpcCheckBox), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceVisibilityValue)));
        }

        public PpcCheckBox()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public bool? IsChecked
        {
            get => (bool?)this.GetValue(IsCheckedProperty);
            set => this.SetValue(IsCheckedProperty, value);
        }

        public bool IsThreeState
        {
            get => (bool)this.GetValue(IsThreeStateProperty);
            set => this.SetValue(IsThreeStateProperty, value);
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set => this.SetValue(LabelTextProperty, value);
        }

        public UserAccessLevel Permission
        {
            get => (UserAccessLevel)this.GetValue(PermissionProperty);
            set => this.SetValue(PermissionProperty, value);
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
                                     this.sessionService.UserAccessLevel == UserAccessLevel.Movement ||
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

                    case UserAccessLevel.Movement:
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

                //refresh
                this.Visibility = this.Visibility;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.eventAggregator = CommonServiceLocator.ServiceLocator.Current.GetInstance<IEventAggregator>();

            this.sessionService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISessionService>();

            this.userAccessLevelToken = this.eventAggregator
                   .GetEvent<UserAccessLevelNotificationPubSubEvent>()
                   .Subscribe(
                       ((m) => this.OnPermissionChanged()),
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
            var checkBox = d as PpcCheckBox;

            if (((Visibility)value == Visibility.Visible)
                && checkBox.PermissionValue)
            {
                checkBox.VisibilityPermission = Visibility.Visible;
            }
            else
            {
                checkBox.VisibilityPermission = Visibility.Collapsed;
            }

            return value;
        }

        private static void PermissionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcCheckBox checkBox)
            {
                checkBox.OnPermissionChanged();
            }
        }

        #endregion
    }
}
