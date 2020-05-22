using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Ferretto.VW.App.Scaffolding
{
    /// <summary>
    /// Interaction logic for ScaffolderWindow.xaml
    /// </summary>
    public partial class ScaffolderWindow : Window
    {
        #region Fields

        public static readonly DependencyProperty CurrentStructureProperty
            = DependencyProperty.Register("CurrentStructure", typeof(Models.ScaffoldedStructure), typeof(ScaffolderWindow), new PropertyMetadata(OnCurrentStructurePropertyChanged));

        public static readonly DependencyProperty EntitiesProperty
            = DependencyProperty.Register("Entities", typeof(ObservableCollection<Models.ScaffoldedEntity>), typeof(ScaffolderWindow));

        public static readonly DependencyProperty StructuresProperty
            = DependencyProperty.Register("Structures", typeof(ObservableCollection<Models.ScaffoldedStructure>), typeof(ScaffolderWindow));

        public static readonly DependencyProperty VertimagProperty = DependencyProperty.Register("Vertimag", typeof(Models.VertimagModel), typeof(ScaffolderWindow));

        private static readonly Models.VertimagModel _vertimag = new Models.VertimagModel
        {
            Bays = new[]
             {
                 new Models.Bay
                 {
                      Number = Models.BayNumber.BayOne,
                       ChainOffset = 10,
                        Inverter = new Models.Inverter
                        {
                             IpAddress = new System.Net.IPAddress(new byte[]{ 127,0,0,1}),
                              TcpPort = 3456
                        },
                         IoDevice = new Models.IoDevice{
                          IpAddress = new System.Net.IPAddress(new byte[]{192, 168, 0, 1 }),
                           TcpPort = 3457,
                            Index = Models.IoIndex.IoDevice1
                         }                         ,  IsExternal = true
                 },
                 new Models.Bay
                 {
                      Number = Models.BayNumber.BayTwo,
                       ChainOffset = 8,
                        Inverter = new Models.Inverter
                        {
                             IpAddress = new System.Net.IPAddress(new byte[]{ 192,168,0,2}),
                              TcpPort = 3458
                        },
                         IoDevice = new Models.IoDevice{
                          IpAddress = new System.Net.IPAddress(new byte[]{192, 168, 0, 3 }),
                           TcpPort = 3459,
                            Index = Models.IoIndex.IoDevice1
                         }                         ,  IsExternal = false
                 }
             },
            Key = "abcdefghijkl"
        };

        #endregion

        #region Constructors

        public ScaffolderWindow()
        {
            this.InitializeComponent();
            this.SetValue(VertimagProperty, _vertimag);
        }

        #endregion

        #region Properties

        public Models.ScaffoldedStructure CurrentStructure
        {
            get => (Models.ScaffoldedStructure)this.GetValue(CurrentStructureProperty);
            set => this.SetValue(CurrentStructureProperty, value);
        }

        public ObservableCollection<Models.ScaffoldedEntity> Entities
        {
            get => (ObservableCollection<Models.ScaffoldedEntity>)this.GetValue(EntitiesProperty);
            set => this.SetValue(EntitiesProperty, value);
        }

        public ObservableCollection<Models.ScaffoldedStructure> Structures
        {
            get => (ObservableCollection<Models.ScaffoldedStructure>)this.GetValue(StructuresProperty);
            set => this.SetValue(StructuresProperty, value);
        }

        #endregion

        #region Methods

        public void SelectCategory(object sender, EventArgs e)
        {
            var context = ((FrameworkElement)sender).DataContext as Models.ScaffoldedStructure;
            this.SetValue(CurrentStructureProperty, context);
        }

        private static void OnCurrentStructurePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var current = e.NewValue as Models.ScaffoldedStructure;
            var window = d as ScaffolderWindow;
            window.Entities = new ObservableCollection<Models.ScaffoldedEntity>(current?.Entities.AsEnumerable() ?? new Models.ScaffoldedEntity[0]);
            window.Structures = new ObservableCollection<Models.ScaffoldedStructure>(current?.Children.AsEnumerable() ?? new Models.ScaffoldedStructure[0]);
        }

        #endregion
    }
}
