using System.Diagnostics;
using System.Windows;
using Ferretto.VW.Utils.Source;

namespace ZZ_CellManagementTestApp
{
    public partial class MainWindow : Window
    {
        #region Fields

        private CellsManagement cm;
        private int machineHeight;
        private int newBayHeight;
        private int newBayHeightFromGround;
        private int newBaySide;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            this.cm = new CellsManagement();
        }

        #endregion Constructors

        #region Properties

        public System.Int32 MachineHeight { get => this.machineHeight; set => this.machineHeight = value; }

        public System.Int32 NewBayHeight { get => this.newBayHeight; set => this.newBayHeight = value; }
        public System.Int32 NewBayHeightFromGround { get => this.newBayHeightFromGround; set => this.newBayHeightFromGround = value; }
        public System.Int32 NewBaySide { get => this.newBaySide; set => this.newBaySide = value; }

        #endregion Properties

        #region Methods

        public void CloseInsertNewBayPopUpButtonMethod(object sender, RoutedEventArgs e)
        {
            this.DoneInsertingBays();
        }

        public void CreateNewBayButtonMethod(object sender, RoutedEventArgs e)
        {
            this.InsertNewBay();
        }

        public void InsertMachineHeightDoneButtonMethod(object sender, RoutedEventArgs e)
        {
            this.InsertMachineHeightDone();
        }

        private void DoneInsertingBays()
        {
            this.cm.CreateBlocks();
            this.cm.UpdateBlocksFile();
            this.cm.UpdateCellsFile();
            this.InsertBaysPopUp.IsOpen = false;
        }

        private void InsertMachineHeightDone()
        {
            this.MachineHeight = int.Parse(this.MachineHeightTextbox.Text);
            this.cm.CreateCellTable(this.MachineHeight);
            this.MachineHeightPopUp.IsOpen = false;
            this.InsertBaysPopUp.IsOpen = true;
        }

        private void InsertNewBay()
        {
            this.NewBayHeight = int.Parse(this.NewBayHeightTextBox.Text);
            this.NewBayHeightFromGround = int.Parse(this.NewBayHeightFromGroundTextBox.Text);
            this.NewBaySide = int.Parse(this.NewBaySideTextBox.Text);
            this.NewBayHeightTextBox.Text = "";
            this.NewBayHeightFromGroundTextBox.Text = "";
            this.NewBaySideTextBox.Text = "";
            int initialNewBayCell = (this.NewBaySide == 0) ? (2 * this.NewBayHeightFromGround) / 25 : ((2 * this.NewBayHeightFromGround) / 25) + 1;
            int finalNewBayCell = (this.NewBaySide == 0) ? (2 * (this.NewBayHeightFromGround + this.NewBayHeight)) / 25 : ((2 * (this.NewBayHeightFromGround + this.NewBayHeight)) / 25) + 1;
            this.cm.CreateBay(initialNewBayCell, finalNewBayCell);
        }

        #endregion Methods
    }
}
