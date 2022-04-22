using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using MahApps.Metro.IconPacks;
using Prism.Events;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for PpcMenuButton.xaml
    /// </summary>
    public partial class PpcMenuButton : UserControl
    {
        #region Fields

        public static readonly DependencyProperty AbbreviationProperty =
            DependencyProperty.Register(nameof(Abbreviation), typeof(string), typeof(PpcMenuButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(PpcMenuButton), new PropertyMetadata(null));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(PpcMenuButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialKind), typeof(PpcMenuButton), new PropertyMetadata(null));

        public static readonly DependencyProperty MenuBrushProperty =
            DependencyProperty.Register(nameof(MenuBrush), typeof(Brush), typeof(PpcMenuButton), new PropertyMetadata(Brushes.Green));

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register(nameof(Number), typeof(string), typeof(PpcMenuButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PermissionProperty =
            DependencyProperty.Register(nameof(Permission), typeof(UserAccessLevel), typeof(PpcMenuButton), new PropertyMetadata(UserAccessLevel.Operator, new PropertyChangedCallback(PermissionChanged)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(PpcMenuButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PpcMenuButton), new PropertyMetadata(string.Empty));

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty VisibilityPermissionProperty =
            DependencyProperty.Register(nameof(VisibilityPermission), typeof(Visibility), typeof(PpcMenuButton), new PropertyMetadata(Visibility.Visible));

        public bool PermissionValue = true;

        private readonly IEventAggregator eventAggregator = null;

        private readonly ISessionService sessionService = null;

        private SubscriptionToken userAccessLevelToken;

        #endregion

        #region Constructors

        static PpcMenuButton()
        {
            UIElement.VisibilityProperty.AddOwner(typeof(PpcMenuButton), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceVisibilityValue)));
        }

        public PpcMenuButton()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.eventAggregator = CommonServiceLocator.ServiceLocator.Current.GetInstance<IEventAggregator>();

            this.sessionService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISessionService>();

            this.Initialization();
        }

        #endregion

        #region Properties

        public string Abbreviation
        {
            get { return (string)this.GetValue(AbbreviationProperty); }
            set { this.SetValue(AbbreviationProperty, value); }
        }

        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public string Description
        {
            get { return (string)this.GetValue(DescriptionProperty); }
            set { this.SetValue(DescriptionProperty, value); }
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

        public string Number
        {
            get { return (string)this.GetValue(NumberProperty); }
            set { this.SetValue(NumberProperty, value); }
        }

        public UserAccessLevel Permission
        {
            get => (UserAccessLevel)this.GetValue(PermissionProperty);
            set => this.SetValue(PermissionProperty, value);
        }

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
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

        private static object CoerceVisibilityValue(DependencyObject d, object value)
        {
            var button = d as PpcMenuButton;

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

        private static void PermissionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcMenuButton button)
            {
                button.OnPermissionChanged();
            }
        }

        private void Initialization()
        {
            this.userAccessLevelToken = this.eventAggregator
                    .GetEvent<UserAccessLevelNotificationPubSubEvent>()
                    .Subscribe(
                        ((m) => this.OnPermissionChanged()),
                        ThreadOption.UIThread,
                        false);

            this.Loaded += (s, e) =>
            {
                this.OnPermissionChanged();
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
