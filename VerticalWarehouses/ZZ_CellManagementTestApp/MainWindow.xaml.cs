using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.VW.Utils.Source.CellManagement;

namespace ZZ_CellManagementTestApp
{
    public class LogItem : Border
    {
        #region Constructors

        public LogItem(string text)
        {
            this.BorderBrush = Brushes.White;
            this.Background = Brushes.Transparent;
            this.BorderThickness = new Thickness(2, 2, 2, 2);
            this.Margin = new Thickness(5, 5, 5, 5);
            this.Width = 900;
            this.Height = 35;
            Label l = new Label();
            l.Content = text;
            l.Foreground = Brushes.White;
            l.Background = Brushes.Transparent;
            this.Child = l;
        }

        #endregion Constructors
    }

    public partial class MainWindow : Window
    {
        #region Fields

        private CellsManager cm;
        private int drawerCounter = 0;
        private int extractDrawerDestinationBayID;
        private int extractDrawerID;
        private int insertNewDrawerHeight;
        private int insertNewDrawerID;
        private int machineHeight;
        private int newBayHeight;
        private int newBayHeightFromGround;
        private int newBaySide;
        private int newUnusableCellID;
        private int reInsertDrawerDestinationBayID;

        #endregion Fields

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            this.cm = new CellsManager();
        }

        #endregion Constructors

        #region Properties

        public Int32 ExtractDrawerDestinationBayID { get => this.extractDrawerDestinationBayID; set => this.extractDrawerDestinationBayID = value; }
        public System.Int32 ExtractDrawerID { get => this.extractDrawerID; set => this.extractDrawerID = value; }
        public System.Int32 InsertNewDrawerHeight { get => this.insertNewDrawerHeight; set => this.insertNewDrawerHeight = value; }
        public System.Int32 InsertNewDrawerID { get => this.insertNewDrawerID; set => this.insertNewDrawerID = value; }
        public System.Int32 MachineHeight { get => this.machineHeight; set => this.machineHeight = value; }

        public System.Int32 NewBayHeight { get => this.newBayHeight; set => this.newBayHeight = value; }
        public System.Int32 NewBayHeightFromGround { get => this.newBayHeightFromGround; set => this.newBayHeightFromGround = value; }
        public System.Int32 NewBaySide { get => this.newBaySide; set => this.newBaySide = value; }
        public Int32 NewUnusableCellID { get => this.newUnusableCellID; set => this.newUnusableCellID = value; }
        public Int32 ReInsertDrawerDestinationBayID { get => this.reInsertDrawerDestinationBayID; set => this.reInsertDrawerDestinationBayID = value; }

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

        public void ExtractDrawerButtonMethod(object sender, RoutedEventArgs e)
        {
            this.ExtractDrawer();
        }

        public void InsertMachineHeightDoneButtonMethod(object sender, RoutedEventArgs e)
        {
            this.InsertMachineHeightDone();
        }

        public void InsertNewDrawerButtonMethod(object sender, RoutedEventArgs e)
        {
            this.InsertNewDrawer();
        }

        public void InsertNewUnusableCellButtonMethod(object sender, RoutedEventArgs e)
        {
            this.InsertNewUnusableCell();
        }

        public void OpenExtractDrawerInputMaskButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OpenExtractDrawerInputMask();
        }

        public void OpenInsertNewDrawerInputMaskButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OpenInsertNewDrawerInputMask();
        }

        public void OpenInsertUnusableCellInputMaskButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OpenInsertUnusableCellInputMask();
        }

        public void ReInsertDrawerButtonMethod(object sender, RoutedEventArgs e)
        {
            this.ReInsertDrawer();
        }

        private void AddLogItemToLogStackPanel(string text)
        {
            this.LogStackPanel.Children.Insert(0, new LogItem(text));
        }

        private void DoneInsertingBays()
        {
            CellManagementMethods.InsertBays(this.cm);
            this.InsertBaysPopUp.IsOpen = false;
            string logtxt = "Bay (s) inserted.";
            this.AddLogItemToLogStackPanel(logtxt);
        }

        private void ExtractDrawer()
        {
            try
            {
                this.ExtractDrawerID = int.Parse(this.ExtractDrawerIDTextBox.Text);
                this.ExtractDrawerDestinationBayID = int.Parse(this.ExtractDrawerDestinationBayIDTextBox.Text);
            }
            catch
            {
            }
            this.ExtractDrawerIDTextBox.Text = "";
            this.ExtractDrawerDestinationBayIDTextBox.Text = "";
            if (CellManagementMethods.ExtractDrawer(this.cm, this.ExtractDrawerID, this.ExtractDrawerDestinationBayID))
            {
                string logtxt = "Drawer ID " + this.ExtractDrawerID + "extracted and positioned on Bay ID " + this.ExtractDrawerDestinationBayID + ".";
                this.AddLogItemToLogStackPanel(logtxt);
            }
            else
            {
                string logtxt = "Drawer ID " + this.ExtractDrawerID + " not extracted. Probably there isn't any drawer with selected ID or the selected bay is already occupied.";
                this.AddLogItemToLogStackPanel(logtxt);
            }
        }

        private void InsertMachineHeightDone()
        {
            this.MachineHeight = int.Parse(this.MachineHeightTextbox.Text);
            CellManagementMethods.CreateCellTable(this.cm, this.MachineHeight);
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
            CellManagementMethods.CreateBay(this.cm, initialNewBayCellIndex, finalNewBayCellIndex);
        }

        private void InsertNewDrawer()
        {
            var watch = Stopwatch.StartNew();
            try
            {
                this.InsertNewDrawerID = int.Parse(this.InsertNewDrawerIDTextBox.Text);
            }
            catch
            {
                this.InsertNewDrawerID = ++this.drawerCounter;
            }
            try
            {
                this.InsertNewDrawerHeight = int.Parse(this.InsertNewDrawerHeightTextBox.Text);
            }
            catch
            {
                this.InsertNewDrawerHeight = new Random().Next(1, 11) * 100;
            }
            if (CellManagementMethods.InsertNewDrawer(this.cm, this.InsertNewDrawerID, this.InsertNewDrawerHeight))
            {//ATTENTION: DRAWER ID - 1 NON E' UGUALE A DRAWER INDEX!!!!!
                this.InsertNewDrawerIDTextBox.Text = "";
                this.InsertNewDrawerHeightTextBox.Text = "";
                int tmp = (this.cm.Drawers.Count == 0) ? 0 : this.cm.Drawers.Count - 1;
                Debug.Print("TMP = " + tmp + "\n");
                string logtxt = "Drawer ID " + this.InsertNewDrawerID + ", height " + this.InsertNewDrawerHeight + " inserted in init cell " + this.cm.Drawers[tmp].FirstCellID + ".";
                this.AddLogItemToLogStackPanel(logtxt);
            }
            else
            {
                string logtxt = "Drawer ID " + this.InsertNewDrawerID + ", height " + this.InsertNewDrawerHeight + " not inserted. There could be no more space for this drawer.";
                this.AddLogItemToLogStackPanel(logtxt);
            }
            watch.Stop();
            Debug.Print("It took " + watch.ElapsedMilliseconds + " milliseconds to insert new drawer of height " + this.InsertNewDrawerHeight + "\n");
        }

        private void InsertNewUnusableCell()
        {
            try
            {
                this.NewUnusableCellID = int.Parse(this.InsertNewUnusableCellIDTextBox.Text);
            }
            catch
            {
                do
                {
                    this.NewUnusableCellID = new Random().Next(1, this.cm.Cells.Count);
                } while (this.cm.Cells[this.NewUnusableCellID].Status != Status.Free);
                Debug.Print("MainWindowCodeBehind::InsertNewUnusableCell: insert unusable cell with ID " + this.NewUnusableCellID + ".\n");
            }
            this.InsertNewUnusableCellIDTextBox.Text = "";
            if (CellManagementMethods.InsertUnusableCell(this.cm, this.NewUnusableCellID))
            {
                string logtxt = "Cell ID " + this.NewUnusableCellID + " status is now set to Unusable";
                this.AddLogItemToLogStackPanel(logtxt);
            }
            else
            {
                string logtxt = "Cell ID " + this.NewUnusableCellID + " status not changed. Probably the selected cell is disabled or already occupied.";
                this.AddLogItemToLogStackPanel(logtxt);
            }
        }

        private void OpenExtractDrawerInputMask()
        {
            this.InsertUnusableCellInputMask.Visibility = Visibility.Hidden;
            this.InsertLUInputMask.Visibility = Visibility.Hidden;
            this.ExtractLUInputMask.Visibility = Visibility.Visible;
        }

        private void OpenInsertNewDrawerInputMask()
        {
            this.InsertUnusableCellInputMask.Visibility = Visibility.Hidden;
            this.ExtractLUInputMask.Visibility = Visibility.Hidden;
            this.InsertLUInputMask.Visibility = Visibility.Visible;
        }

        private void OpenInsertUnusableCellInputMask()
        {
            this.InsertLUInputMask.Visibility = Visibility.Hidden;
            this.ExtractLUInputMask.Visibility = Visibility.Hidden;
            this.InsertUnusableCellInputMask.Visibility = Visibility.Visible;
        }

        private void ReInsertDrawer()
        {
            try
            {
                this.ReInsertDrawerDestinationBayID = int.Parse(this.BayIDTofreeTextBox.Text);
            }
            catch
            {
                this.ReInsertDrawerDestinationBayID = 1;
            }
            string logtxt1 = "";
            if (this.ReInsertDrawerDestinationBayID <= this.cm.Bays.Count)
            {
                logtxt1 = "Drawer ID " + this.cm.Bays[this.ReInsertDrawerDestinationBayID - 1].DrawerID + ", height " + this.cm.Drawers[this.cm.Bays[this.ReInsertDrawerDestinationBayID - 1].DrawerID].HeightMillimiters + " inserted in init cell " + this.cm.Drawers[this.cm.Bays[this.ReInsertDrawerDestinationBayID - 1].DrawerID].FirstCellID + ".";
            }
            this.BayIDTofreeTextBox.Text = "";
            if (CellManagementMethods.ReInsertDrawer(this.cm, this.ReInsertDrawerDestinationBayID))
            {
                string logtxt = "Bay ID " + this.ReInsertDrawerDestinationBayID + " freed.";
                this.AddLogItemToLogStackPanel(logtxt);
                this.AddLogItemToLogStackPanel(logtxt1);
            }
            else
            {
                string logtxt = "Error: Bay ID " + this.ReInsertDrawerDestinationBayID + " apparently already free.";
                this.AddLogItemToLogStackPanel(logtxt);
            }
        }

        #endregion Methods
    }
}
