using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// File for cells' management. Data stored in CellsManager instance, methods stored in CellsManagementMethods
// ATTENTION: MAKE SURE TO CHECK WHEN METHODS REQUIRE CELL ID OR CELL INDEX (WHERE CELL INDEX = CELL ID - 1)

namespace Ferretto.VW.Utils.Source
{
    public enum Side
    {
        FrontEven,
        BackOdd
    }

    public enum Status
    {
        Free,
        Disabled,
        Occupied,
        Unusable
    }

    // ATTENTION: MAKE SURE TO CHECK WHEN METHODS REQUIRE CELL ID OR CELL INDEX (WHERE CELL INDEX = CELL ID - 1)
    public static class CellManagementMethods
    {
        #region Fields

        public const int AISLE_SIDES_COUNT = 2;
        public const int CELL_HEIGHT_MILLIMETERS = 25;

        public static readonly string JSON_BLOCK_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["BlocksFilePath"]);
        public static readonly string JSON_CELL_PATH = string.Concat(Environment.CurrentDirectory, ConfigurationManager.AppSettings["CellsFilePath"]);

        #endregion Fields

        #region Methods

        public static void CompactCells(CellsManager cm)
        {
        }

        public static void CompactCells(CellsManager cm, Side side)
        {
        }

        public static void CompactCells(CellsManager cm, int heightMillimiters)
        {
        }

        public static bool CreateBay(CellsManager cm, int firstCellID, int lastCellID)
        {
            if ((firstCellID % 2 == 0 && lastCellID % 2 != 0) || (firstCellID % 2 != 0 && lastCellID % 2 == 0))
            {
                Debug.Print("CellManagementMethods::CreateBay Error: inserted cell not on same side of the machine.\n");
                return false;
            }
            if (cm.BayCounter > 3)
            {
                Debug.Print("CellManagementMethods::CreateBay Error: it's not possible to insert more than 3 bays.\n");
                return false;
            }
            for (int id = firstCellID; id <= lastCellID; id += 2)
            {
                ChangeCellStatus(cm, id - 1, Status.Disabled);
            }
            cm.Bays.Add(new Bay(++cm.BayCounter, ((lastCellID - firstCellID) / AISLE_SIDES_COUNT) * CELL_HEIGHT_MILLIMETERS, firstCellID));
            return true;
        }

        public static bool CreateBlocks(CellsManager cm)
        {
            cm.Blocks = null;
            cm.Blocks = new List<CellBlock>();
            int counter = 1;
            for (int index = 0; index < cm.Cells.Count; index += 2) //odd ID cell's index
            {
                if (cm.Cells[index].Status == 0)
                {
                    int tmp = GetLastUpperNotDisabledCellIndex(cm, index);
                    var cb = new CellBlock(index + 1, tmp + 1, counter);
                    cm.Blocks.Add(cb);
                    counter++;
                    index = tmp;
                }
            }
            for (int index = 1; index < cm.Cells.Count; index += 2)//even ID cell's index
            {
                if (cm.Cells[index].Status == 0)
                {
                    int tmp = GetLastUpperNotDisabledCellIndex(cm, index);
                    var cb = new CellBlock(index + 1, tmp + 1, counter);
                    cm.Blocks.Add(cb);
                    counter++;
                    index = tmp;
                }
            }
            UpdateBlocksFile(cm);
            return true;
        }

        public static void CreateCellTable(CellsManager cm, int machineHeightMillimiters)
        {
            int cells = CalculateCellQuantityFromMachineHeight(machineHeightMillimiters);
            for (int id = 1; id <= cells; id++)
            {
                var c = new Cell(id);
                cm.Cells.Add(c);
            }
            UpdateCellsFile(cm);
        }

        public static bool ExtractDrawer(CellsManager cm, int drawerID, int destinationBayID)
        {
            if (cm.Bays[destinationBayID - 1].Occupied)
            {
                Debug.Print("CellManagementMethods::ExtractDrawer Error: destination bay is already occupied. Destination Bay ID = " + destinationBayID + ".\n");
                return false;
            }
            if (cm.Drawers.Find(x => x.Id == drawerID) == null)
            {
                Debug.Print("CellManagementMethods::ExtractDrawer Error: did not found drawer with this ID: " + drawerID + ".\n");
                return false;
            }
            cm.Bays[destinationBayID - 1].Occupied = true;
            cm.Bays[destinationBayID - 1].DrawerID = drawerID;
            var d = (from ret_d in cm.Drawers where ret_d.Id == drawerID select ret_d).First();
            FreeCells(cm, d.FirstCellID - 1, d.HeightMillimiters);
            UpdateCellsFile(cm);
            CreateBlocks(cm);
            UpdateBlocksFile(cm);
            Debug.Print("CellManagementMethods::ExtractDrawer output: Drawer with ID " + drawerID + " successfully extracted.\n");
            return true;
        }

        public static int GetFreeCellQuantity(CellsManager cm)
        {
            return cm.Blocks.Sum(x => x.FinalIDCell - x.InitialIDCell);
        }

        public static int GetFreeCellQuantity(CellsManager cm, Side side)
        {
            return cm.Blocks.Where(x => x.Side == side).Sum(x => x.FinalIDCell - x.InitialIDCell);
        }

        public static void InsertBays(CellsManager cm)
        {
            CreateBlocks(cm);
            UpdateCellsFile(cm);
            UpdateBlocksFile(cm);
        }

        public static bool InsertNewDrawer(CellsManager cm, int newDrawerID, int newDrawerHeightMillimiters)
        {
            var initCellID = FindFirstFreeCellIDForDrawerInsert(cm, newDrawerHeightMillimiters);
            if (initCellID < 0)
            {
                return false;
            }
            var d = new Drawer(newDrawerID, newDrawerHeightMillimiters, initCellID);
            cm.Drawers.Add(d);
            OccupyCells(cm, initCellID - 1, newDrawerHeightMillimiters);
            UpdateCellsFile(cm);
            CreateBlocks(cm);
            UpdateBlocksFile(cm);
            return true;
        }

        public static bool InsertUnusableCell(CellsManager cm, int cellID)
        {
            if (cm.Cells[cellID - 1].Status != Status.Free)
            {
                Debug.Print("CellManagementMethods::InsertUnusableCell Error: selected Cell ID " + cellID + " is not free.\n");
                return false;
            }
            ChangeCellStatus(cm, cellID - 1, Status.Unusable);
            CreateBlocks(cm);
            UpdateCellsFile(cm);
            UpdateBlocksFile(cm);
            return true;
        }

        public static bool ReInsertDrawer(CellsManager cm, int bayID)
        {
            if (cm.Bays.Count < bayID || !cm.Bays[bayID - 1].Occupied)
            {
                Debug.Print("CellManagementMethods::ReInsertDrawer Error: selected bay ID " + bayID + " is not occupied or specified ID is not present.\n");
                return false;
            }
            InsertNewDrawer(cm, cm.Bays[(int)bayID - 1].DrawerID, cm.Drawers[cm.Bays[bayID - 1].DrawerID - 1].HeightMillimiters);
            cm.Bays[(int)bayID - 1].Occupied = false;
            cm.Bays[(int)bayID - 1].DrawerID = -1;
            CreateBlocks(cm);
            UpdateCellsFile(cm);
            UpdateBlocksFile(cm);
            return true;
        }

        public static void UpdateBlocksFile(CellsManager cm)
        {
            var json = JsonConvert.SerializeObject(cm.Blocks, Formatting.Indented);

            if (File.Exists(JSON_BLOCK_PATH))
            {
                File.Delete(JSON_BLOCK_PATH);
            }
            File.WriteAllText(JSON_BLOCK_PATH, json);
        }

        public static void UpdateCellsFile(CellsManager cm)
        {
            var json = JsonConvert.SerializeObject(cm.Cells, Formatting.Indented);

            if (File.Exists(JSON_CELL_PATH))
            {
                File.Delete(JSON_CELL_PATH);
            }
            File.WriteAllText(JSON_CELL_PATH, json);
        }

        private static int CalculateCellQuantityFromMachineHeight(int machineHeightMillimiters)
        {
            int cells = machineHeightMillimiters / CELL_HEIGHT_MILLIMETERS;
            return cells * AISLE_SIDES_COUNT;
        }

        private static void ChangeCellStatus(CellsManager cm, int cellIndex, Status newStatus)
        {
            if (cellIndex >= cm.Cells.Count || cellIndex < 0)
            {
                throw new ArgumentException("CellsManagement Exception: cellID does not point to any cell in memory.", "cellID");
            }
            cm.Cells[cellIndex].Status = newStatus;
        }

        private static int FindFirstFreeCellIDForDrawerInsert(CellsManager cm, int drawerHeightMillimiters)
        {
            var tempCellBlock = cm.Blocks.Where(x => x.BlockHeightMillimiters > drawerHeightMillimiters).OrderBy(x => x.Priority).ToList();

            if (tempCellBlock.Count > 0)
            {
                return tempCellBlock[0].InitialIDCell;
            }
            else
            {
                Debug.Print("There are NO block with enough height to host this drawer.\n");
                return -1;
            }
        }

        private static void FreeCells(CellsManager cm, int firstCellIndex, int drawerHeightMillimiters)
        {
            var cellsToFree = drawerHeightMillimiters / CELL_HEIGHT_MILLIMETERS;
            for (int index = firstCellIndex; index <= firstCellIndex + cellsToFree * AISLE_SIDES_COUNT; index += 2)
            {
                ChangeCellStatus(cm, index, 0);
            }
        }

        private static int GetLastUpperNotDisabledCellIndex(CellsManager cm, int cellIndex)
        {
            for (int index = cellIndex + 2; index < cm.Cells.Count; index += 2)
            {
                if (cm.Cells[index].Status != Status.Disabled && cm.Cells[index].Status != Status.Occupied)
                {
                }
                else
                {
                    return index - 2;
                }
            }
            return (cellIndex % 2 == 0) ? cm.Cells.Count - 2 : cm.Cells.Count - 1;
        }

        private static void OccupyCells(CellsManager cm, int firstCellindex, int drawerHeightMillimiters)
        {
            int cellsToOccupy = (drawerHeightMillimiters / CELL_HEIGHT_MILLIMETERS) + 1;
            for (int index = firstCellindex; index <= firstCellindex + cellsToOccupy * AISLE_SIDES_COUNT; index += 2)
            {
                ChangeCellStatus(cm, index, Status.Occupied);
            }
        }

        #endregion Methods
    }

    public class Bay
    {
        #region Fields

        private int heightMillimiters;

        #endregion Fields

        #region Constructors

        public Bay(int newBayID, int newBayHeight, int newBayFirstCellID)
        {
            this.FirstCellID = newBayFirstCellID;
            this.HeightMillimiters = newBayHeight;
            this.Id = newBayID;
        }

        #endregion Constructors

        #region Properties

        public Int32 DrawerID { get; set; }
        public Int32 FirstCellID { get; set; }

        public Int32 HeightMillimiters
        {
            get => this.heightMillimiters;
            set
            {
                if (value >= 0)
                {
                    this.heightMillimiters = value;
                }
                else
                {
                    this.heightMillimiters = 0;
                    Debug.Print("BayConstructor:: input height is less than 0. Height set to 0.");
                }
            }
        }

        public Int32 Id { get; set; }
        public Boolean Occupied { get; set; }

        #endregion Properties
    }

    public class Cell
    {
        #region Constructors

        public Cell(int id)
        {
            this.IdCell = id;
            this.Priority = id;
            if (id % 2 == 0) //if id is even
            {
                this.Coord = (id == 2) ? CellManagementMethods.CELL_HEIGHT_MILLIMETERS : CellManagementMethods.CELL_HEIGHT_MILLIMETERS * (id / CellManagementMethods.AISLE_SIDES_COUNT);
                this.Side = Side.FrontEven;
            }
            else //if id is odd
            {
                this.Coord = (id == 1) ? CellManagementMethods.CELL_HEIGHT_MILLIMETERS : CellManagementMethods.CELL_HEIGHT_MILLIMETERS * ((id / CellManagementMethods.AISLE_SIDES_COUNT) + 1);
                this.Side = Side.BackOdd;
            }
            this.Status = Status.Free;
        }

        #endregion Constructors

        #region Properties

        public Int32 Coord { get; set; }
        public Int32 IdCell { get; set; }
        public Int32 Priority { get; set; }
        public Side Side { get; set; }
        public Status Status { get; set; }

        #endregion Properties

        // status code: 0 = free; 1 = disabled; 2 = occupied; 3 = unusable
    }

    public class CellBlock
    {
        #region Fields

        private int blockHeightMillimiters;

        #endregion Fields

        #region Constructors

        public CellBlock(int firstCellID, int lastCellID, int blockID)
        {
            if ((firstCellID % 2 == 0 && lastCellID % 2 != 0) || (firstCellID % 2 != 0 && lastCellID % 2 == 0))
            {
                throw new ArgumentException("Cells' Management Exception: final cell not on the same side of initial cell.", "lastCell");
            }
            this.InitialIDCell = firstCellID;
            this.FinalIDCell = lastCellID;
            this.Priority = this.InitialIDCell;
            this.BlockHeightMillimiters = ((lastCellID - firstCellID) / 2) * CellManagementMethods.CELL_HEIGHT_MILLIMETERS;
            this.Side = (firstCellID % 2 == 0) ? Side.FrontEven : Side.BackOdd;
            this.IdGroup = blockID;
        }

        #endregion Constructors

        #region Properties

        public Int32 Area { get; set; }

        public Int32 BlockHeightMillimiters
        {
            get => this.blockHeightMillimiters;
            set
            {
                if (value >= 0)
                {
                    this.blockHeightMillimiters = value;
                }
                else
                {
                    this.blockHeightMillimiters = 0;
                    Debug.Print("CellBlockConstructor:: input height is less than 0. Height set to 0.");
                }
            }
        }

        public Int32 FinalIDCell { get; set; }
        public Int32 IdGroup { get; set; }
        public Int32 InitialIDCell { get; set; }
        public Int32 Machine { get; set; }
        public Int32 Priority { get; set; }
        public Side Side { get; set; }

        #endregion Properties
    }

    public class CellsManager
    {
        #region Constructors

        public CellsManager()
        {
            this.Bays = new List<Bay>();
            this.Blocks = new List<CellBlock>();
            this.Cells = new List<Cell>();
            this.Drawers = new List<Drawer>();
        }

        #endregion Constructors

        #region Properties

        public Int32 BayCounter { get; set; }

        public List<Bay> Bays { get; set; }
        public List<CellBlock> Blocks { get; set; }
        public List<Cell> Cells { get; set; }
        public List<Drawer> Drawers { get; set; }

        #endregion Properties
    }

    public class Drawer
    {
        #region Fields

        private int heightMillimiters;

        #endregion Fields

        #region Constructors

        public Drawer(int newID, int newHeight, int newFirstCellID)
        {
            this.Id = newID;
            this.HeightMillimiters = newHeight;
            this.FirstCellID = newFirstCellID;
        }

        #endregion Constructors

        #region Properties

        public Int32 FirstCellID { get; set; }

        public Int32 HeightMillimiters
        {
            get => this.heightMillimiters;
            set
            {
                if (value >= 0)
                {
                    this.heightMillimiters = value;
                }
                else
                {
                    this.heightMillimiters = 0;
                    Debug.Print("DrawerConstructor:: input height is less than 0. Height set to 0.");
                }
            }
        }

        public Int32 Id { get; set; }

        #endregion Properties
    }
}
