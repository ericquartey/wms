using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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

        public static readonly string JSON_BLOCK_PATH = Environment.CurrentDirectory + "/blockstable.json";
        public static readonly string JSON_CELL_PATH = Environment.CurrentDirectory + "/cellstable.json";

        #endregion Fields

        #region Methods

        public static void CompactCells(CellsManager cm)
        {
        }

        public static void CompactCells(CellsManager cm, Side side)
        {
        }

        public static void CompactCells(CellsManager cm, int height)
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
            cm.Bays.Add(new Bay(++cm.BayCounter, ((lastCellID - firstCellID) / 2) * 25, firstCellID));
            return true;
        }

        public static bool CreateBlocks(CellsManager cm)
        {
            var watch = Stopwatch.StartNew();
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
            watch.Stop();
            var elapsedMs = watch.ElapsedTicks;
            Debug.Print("Create Block in RAM took " + elapsedMs + " timerticks to complete (" + (elapsedMs * ((double)Stopwatch.Frequency / 1000000D) + " microseconds). It does " + Stopwatch.Frequency + " ticks every second. " + ((double)Stopwatch.Frequency / 1000000D) + " microsecond per tick.\n"));
            watch = Stopwatch.StartNew();
            UpdateBlocksFile(cm);
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            Debug.Print("Create Block write File I/O took " + elapsedMs + " ms to complete.\n");
            return true;
        }

        public static void CreateCellTable(CellsManager cm, int machineHeight)
        {
            int cells = CalculateCellQuantityFromMachineHeight(machineHeight);
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
            FreeCells(cm, d.FirstCellID - 1, d.Height);
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

        public static bool InsertNewDrawer(CellsManager cm, int newDrawerID, int newDrawerHeight)
        {
            var watch = Stopwatch.StartNew();
            var initCellID = FindFirstFreeCellIDForDrawerInsert(cm, newDrawerHeight);
            if (initCellID < 0)
            {
                return false;
            }
            var d = new Drawer(newDrawerID, newDrawerHeight, initCellID);
            cm.Drawers.Add(d);
            OccupyCells(cm, initCellID - 1, newDrawerHeight);
            UpdateCellsFile(cm);
            CreateBlocks(cm);
            UpdateBlocksFile(cm);
            watch.Stop();
            Debug.Print("It took " + watch.ElapsedMilliseconds + " millisecond to complete InsertNewDrawer method in CellManager.\n");
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
            InsertNewDrawer(cm, cm.Bays[bayID - 1].DrawerID, cm.Drawers[cm.Bays[bayID - 1].DrawerID - 1].Height);
            cm.Bays[bayID - 1].Occupied = false;
            cm.Bays[bayID - 1].DrawerID = -1;
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
                File.WriteAllText(JSON_BLOCK_PATH, json);
            }
            else
            {
                File.WriteAllText(JSON_BLOCK_PATH, json);
            }
        }

        public static void UpdateCellsFile(CellsManager cm)
        {
            var watch = Stopwatch.StartNew();
            var json = JsonConvert.SerializeObject(cm.Cells, Formatting.Indented);

            if (File.Exists(JSON_CELL_PATH))
            {
                File.Delete(JSON_CELL_PATH);
                File.WriteAllText(JSON_CELL_PATH, json);
            }
            else
            {
                File.WriteAllText(JSON_CELL_PATH, json);
            }
            watch.Stop();
            Debug.Print("Update Cells file took: " + watch.ElapsedMilliseconds + " milliseconds. \n");
        }

        private static int CalculateCellQuantityFromMachineHeight(int machineHeight)
        {
            int cells = machineHeight / 25;
            return cells * 2;
        }

        private static void ChangeCellStatus(CellsManager cm, int cellIndex, Status newStatus)
        {
            if (cellIndex >= cm.Cells.Count || cellIndex < 0)
            {
                throw new ArgumentException("CellsManagement Exception: cellID does not point to any cell in memory.", "cellID");
            }
            cm.Cells[cellIndex].Status = newStatus;
        }

        private static int FindFirstFreeCellIDForDrawerInsert(CellsManager cm, int drawerHeight)
        {
            var watch = Stopwatch.StartNew();
            var tempCellBlockIenumerable = cm.Blocks.Where(x => x.BlockHeight > drawerHeight);
            tempCellBlockIenumerable = tempCellBlockIenumerable.OrderBy(x => x.Priority);
            var tempCellBlockList = tempCellBlockIenumerable.ToList();

            if (tempCellBlockList.Count > 0)
            {
                watch.Stop();
                Debug.Print("Took " + watch.ElapsedMilliseconds + " ms to find first free cell id for new drawer of height " + drawerHeight + "\n");
                return tempCellBlockList[0].InitialIDCell;
            }
            else
            {
                Debug.Print("There are NO block with enough height to host this drawer.\n");
                return -1;
            }
        }

        private static void FreeCells(CellsManager cm, int firstCellIndex, int drawerHeight)
        {
            var cellsToFree = drawerHeight / 25;
            for (int index = firstCellIndex; index <= firstCellIndex + cellsToFree * 2; index += 2)
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

        private static void OccupyCells(CellsManager cm, int firstCellindex, int drawerHeight)
        {
            int cellsToOccupy = (drawerHeight / 25) + 1;
            for (int index = firstCellindex; index <= firstCellindex + cellsToOccupy * 2; index += 2)
            {
                ChangeCellStatus(cm, index, Status.Occupied);
            }
        }

        #endregion Methods
    }

    public class Bay
    {
        #region Fields

        private int drawerID = -1;
        private int firstCellID;
        private int height;
        private int id;
        private bool occupied = false;

        #endregion Fields

        #region Constructors

        public Bay(int newBayID, int newBayHeight, int newBayFirstCellID)
        {
            this.FirstCellID = newBayFirstCellID;
            this.Height = newBayHeight;
            this.Id = newBayID;
        }

        #endregion Constructors

        #region Properties

        public Int32 DrawerID { get => this.drawerID; set => this.drawerID = value; }
        public Int32 FirstCellID { get => this.firstCellID; set => this.firstCellID = value; }
        public Int32 Height { get => this.height; set => this.height = value; }
        public Int32 Id { get => this.id; set => this.id = value; }
        public Boolean Occupied { get => this.occupied; set => this.occupied = value; }

        #endregion Properties
    }

    public class Cell
    {
        #region Fields

        private int coord;
        private int idCell;
        private int priority;
        private Side side;
        private Status status;

        #endregion Fields

        #region Constructors

        public Cell(int id)
        {
            this.IdCell = id;
            this.Priority = id;
            if (id % 2 == 0) //if id is even
            {
                this.Coord = (id == 2) ? 25 : 25 * (id / 2);
                this.Side = Side.FrontEven;
            }
            else //if id is odd
            {
                this.Coord = (id == 1) ? 25 : 25 * ((id / 2) + 1);
                this.Side = Side.BackOdd;
            }
            this.Status = Status.Free;
        }

        #endregion Constructors

        #region Properties

        public Int32 Coord { get => this.coord; set => this.coord = value; }
        public Int32 IdCell { get => this.idCell; set => this.idCell = value; }
        public Int32 Priority { get => this.priority; set => this.priority = value; }
        public Side Side { get => this.side; set => this.side = value; }
        public Status Status { get => this.status; set => this.status = value; }

        #endregion Properties

        // status code: 0 = free; 1 = disabled; 2 = occupied; 3 = unusable
    }

    public class CellBlock
    {
        #region Fields

        private int area;
        private int blockHeight;
        private int finalIDCell;
        private int idGroup;
        private int initialIDCell;
        private int machine;
        private int priority;
        private Side side;

        #endregion Fields

        #region Constructors

        public CellBlock(int firstCellID, int lastCellID, int blockID)
        {
            if ((firstCellID % 2 == 0 && lastCellID % 2 != 0) || (firstCellID % 2 != 0 && lastCellID % 2 == 0))
            {
                throw new ArgumentException("Cells' Management Exception: final cell not on the same side of initial cell.", "lastCell");
            }
            this.Area = 0;
            this.Machine = 0;
            this.InitialIDCell = firstCellID;
            this.FinalIDCell = lastCellID;
            this.Priority = this.InitialIDCell;
            this.BlockHeight = ((lastCellID - firstCellID) / 2) * 25;
            this.Side = (firstCellID % 2 == 0) ? Side.FrontEven : Side.BackOdd;
            this.IdGroup = blockID;
        }

        #endregion Constructors

        #region Properties

        public Int32 Area { get => this.area; set => this.area = value; }
        public Int32 BlockHeight { get => this.blockHeight; set => this.blockHeight = value; }
        public Int32 FinalIDCell { get => this.finalIDCell; set => this.finalIDCell = value; }
        public Int32 IdGroup { get => this.idGroup; set => this.idGroup = value; }
        public Int32 InitialIDCell { get => this.initialIDCell; set => this.initialIDCell = value; }
        public Int32 Machine { get => this.machine; set => this.machine = value; }
        public Int32 Priority { get => this.priority; set => this.priority = value; }
        public Side Side { get => this.side; set => this.side = value; }

        #endregion Properties
    }

    public class CellsManager
    {
        #region Fields

        private int bayCounter = 0;
        private List<Bay> bays = new List<Bay>();
        private List<CellBlock> blocks = new List<CellBlock>();
        private List<Cell> cells = new List<Cell>();
        private List<Drawer> drawers = new List<Drawer>();

        #endregion Fields

        #region Constructors

        public CellsManager()
        {
        }

        #endregion Constructors

        #region Properties

        public Int32 BayCounter { get => this.bayCounter; set => this.bayCounter = value; }

        public List<Bay> Bays { get => this.bays; set => this.bays = value; }
        public List<CellBlock> Blocks { get => this.blocks; set => this.blocks = value; }
        public List<Cell> Cells { get => this.cells; set => this.cells = value; }
        public List<Drawer> Drawers { get => this.drawers; set => this.drawers = value; }

        #endregion Properties
    }

    public class Drawer
    {
        #region Fields

        private int firstCellID;
        private int height;
        private int id;

        #endregion Fields

        #region Constructors

        public Drawer(int newID, int newHeight, int newFirstCellID)
        {
            this.Id = newID;
            this.Height = newHeight;
            this.FirstCellID = newFirstCellID;
        }

        #endregion Constructors

        #region Properties

        public Int32 FirstCellID { get => this.firstCellID; set => this.firstCellID = value; }
        public Int32 Height { get => this.height; set => this.height = value; }
        public Int32 Id { get => this.id; set => this.id = value; }

        #endregion Properties

        #region Methods

        public void ChangeFirstCellID(int newFirstCellID)
        {
            this.FirstCellID = newFirstCellID;
        }

        #endregion Methods
    }
}
