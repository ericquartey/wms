using System;
using System.Diagnostics;
using System.Windows;
using Ferretto.VW.Utils.Source;

namespace ZZ_CellManagementTestApp
{
    public partial class MainWindow : Window
    {
        #region Fields

        private CellsManager cm;
        private int extractDrawerID;
        private int insertNewDrawerHeight;
        private int insertNewDrawerID;
        private int machineHeight;
        private int newBayHeight;
        private int newBayHeightFromGround;
        private int newBaySide;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            this.cm = new CellsManager();
        }

        #endregion Constructors

        #region Properties

        public System.Int32 ExtractDrawerID { get => this.extractDrawerID; set => this.extractDrawerID = value; }
        public System.Int32 InsertNewDrawerHeight { get => this.insertNewDrawerHeight; set => this.insertNewDrawerHeight = value; }
        public System.Int32 InsertNewDrawerID { get => this.insertNewDrawerID; set => this.insertNewDrawerID = value; }
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

        public void InsertNewDrawerButtonMethod(object sender, RoutedEventArgs e)
        {
            this.InsertNewDrawer();
        }

        public void OpenExtractDrawerInputMaskButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OpenExtractDrawerInputMask();
        }

        public void OpenInsertNewDrawerInputMaskButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OpenInsertNewDrawerInputMask();
        }

        private void DoneInsertingBays()
        {
            this.cm.CreateBlocks();
            this.cm.UpdateCellsFile();
            this.cm.UpdateBlocksFile();
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
            int initialNewBayCellIndex = (this.NewBaySide != 0) ? ((2 * this.NewBayHeightFromGround) / 25) - 1 : (2 * this.NewBayHeightFromGround) / 25;
            int finalNewBayCellIndex = (this.NewBaySide != 0) ? ((2 * (this.NewBayHeightFromGround + this.NewBayHeight)) / 25) - 1 : (2 * (this.NewBayHeightFromGround + this.NewBayHeight)) / 25;
            this.cm.CreateBay(initialNewBayCellIndex, finalNewBayCellIndex);
        }

        private void InsertNewDrawer()
        {
            var watch = Stopwatch.StartNew();
            var watch1 = Stopwatch.StartNew();
            try
            {
                this.InsertNewDrawerID = int.Parse(this.InsertNewDrawerIDTextBox.Text);
            }
            catch
            {
                this.InsertNewDrawerID = 0;
            }
            watch1.Stop();
            Debug.Print("Phase 1 took " + watch1.ElapsedMilliseconds + " milliseconds.\n");
            var watch2 = Stopwatch.StartNew();
            try
            {
                this.InsertNewDrawerHeight = int.Parse(this.InsertNewDrawerHeightTextBox.Text);
            }
            catch
            {
                this.InsertNewDrawerHeight = new Random().Next(1, 11) * 100;
                Debug.Print("New Drawer Height: " + this.InsertNewDrawerHeight + "\n");
            }
            watch2.Stop();
            Debug.Print("Phase 2 took " + watch2.ElapsedMilliseconds + " milliseconds.\n");
            var watch3 = Stopwatch.StartNew();
            if (this.cm.InsertNewDrawer(this.InsertNewDrawerID, this.InsertNewDrawerHeight))
            {
                this.InsertNewDrawerIDTextBox.Text = "";
                this.InsertNewDrawerHeightTextBox.Text = "";
            }
            else
            {
                Debug.Print("Failed inserting new drawer with height " + this.InsertNewDrawerHeight + "\n");
            }
            watch3.Stop();
            Debug.Print("Phase 3 took " + watch3.ElapsedMilliseconds + " milliseconds.\n");
            watch.Stop();
            Debug.Print("It took " + watch.ElapsedMilliseconds + " milliseconds to insert new drawer of height " + this.InsertNewDrawerHeight + "\n");
        }

        private void OpenExtractDrawerInputMask()
        {
            this.InsertLUInputMask.Visibility = Visibility.Hidden;
            this.ExtractLUInputMask.Visibility = Visibility.Visible;
        }

        private void OpenInsertNewDrawerInputMask()
        {
            this.ExtractLUInputMask.Visibility = Visibility.Hidden;
            this.InsertLUInputMask.Visibility = Visibility.Visible;
        }

        #endregion Methods
    }
}
