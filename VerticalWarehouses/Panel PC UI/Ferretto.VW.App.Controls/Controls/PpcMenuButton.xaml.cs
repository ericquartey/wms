using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            DependencyProperty.Register(
                nameof(Abbreviation),
                typeof(string),
                typeof(PpcMenuButton),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(PpcMenuButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(
                nameof(Kind),
                typeof(PackIconMaterialKind),
                typeof(PpcMenuButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MenuBrushProperty =
            DependencyProperty.Register(
                nameof(MenuBrush),
                typeof(Brush),
                typeof(PpcMenuButton),
                new PropertyMetadata(Brushes.Green));

        public static readonly DependencyProperty PermitionProperty =
            DependencyProperty.Register(
                nameof(Permition),
                typeof(UserAccessLevel),
                typeof(PpcMenuButton),
                new PropertyMetadata(UserAccessLevel.Operator, new PropertyChangedCallback(OnPermitionChanged)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(PpcMenuButton),
                new PropertyMetadata(string.Empty));

        private readonly IEventAggregator eventAggregator = CommonServiceLocator.ServiceLocator.Current.GetInstance<IEventAggregator>();

        private readonly ISessionService sessionService = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISessionService>();

        private SubscriptionToken userAccessLevelToken;

        private bool? visibleOldStatus;

        #endregion

        #region Constructors

        public PpcMenuButton()
        {
            this.InitializeComponent();
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

        public UserAccessLevel Permition
        {
            get => (UserAccessLevel)this.GetValue(PermitionProperty);
            set => this.SetValue(PermitionProperty, value);
        }

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
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
                bool condition = false;
                switch (this.Permition)
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
                        throw new ArgumentException(nameof(this.Permition));

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

        private static void OnPermitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcMenuButton button)
            {
                button.PermitionChanged();
            }
        }

        private void Initialization()
        {
            this.userAccessLevelToken = this.eventAggregator
                    .GetEvent<UserAccessLevelNotificationPubSubEvent>()
                    .Subscribe(
                        ((m) => this.PermitionChanged()),
                        ThreadOption.UIThread,
                        false);

            this.Unloaded += (s, e) =>
            {
                this.userAccessLevelToken?.Dispose();
                this.userAccessLevelToken = null;
            };
        }

        #endregion
    }
}
