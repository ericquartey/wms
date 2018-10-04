using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

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

    public static class CreateAndPopulateTables
    {
        #region Fields

        public static readonly string JSON_BLOCK_PATH = Environment.CurrentDirectory + "/blockstable.json";
        public static readonly string JSON_CELL_PATH = Environment.CurrentDirectory + "/cellstable.json";

        #endregion Fields
    }

    public class CellsManager
    {
        #region Fields

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

        internal List<CellBlock> Blocks { get => this.blocks; set => this.blocks = value; }
        internal List<Cell> Cells { get => this.cells; set => this.cells = value; }
        internal List<Drawer> Drawers { get => this.drawers; set => this.drawers = value; }

        #endregion Properties

        #region Methods

        public int CalculateCellQuantityFromMachineHeight(int machineHeight)
        {
            int cells = machineHeight / 25;
            return cells * 2;
        }

        public void ChangeCellStatus(int cellIndex, Status newStatus)
        {
            if (cellIndex >= this.Cells.Count || cellIndex < 0)
            {
                throw new ArgumentException("CellsManagement Exception: cellID does not point to any cell in memory.", "cellID");
            }
            this.Cells[cellIndex].Status = newStatus;
        }

        public void CreateBay(int firstCellID, int lastCellID)
        {
            if ((firstCellID % 2 == 0 && lastCellID % 2 != 0) || (firstCellID % 2 != 0 && lastCellID % 2 == 0))
            {
                throw new ArgumentException("Cells' Management Exception: final cell not on the same side of initial cell.", "lastCell");
            }
            for (int id = firstCellID; id <= lastCellID; id += 2)
            {
                this.ChangeCellStatus(id - 1, Status.Disabled);
            }
        }

        public void CreateBlocks()
        {
            var watch = Stopwatch.StartNew();
            this.Blocks = null;
            this.Blocks = new List<CellBlock>();
            int counter = 1;
            for (int index = 0; index < this.Cells.Count; index += 2) //odd ID cell's index
            {
                if (this.Cells[index].Status == 0)
                {
                    int tmp = this.GetLastUpperNotDisabledCellIndex(index);
                    CellBlock cb = new CellBlock(index + 1, tmp + 1, counter);
                    this.Blocks.Add(cb);
                    counter++;
                    index = tmp;
                }
            }
            for (int index = 1; index < this.Cells.Count; index += 2)//even ID cell's index
            {
                if (this.Cells[index].Status == 0)
                {
                    int tmp = this.GetLastUpperNotDisabledCellIndex(index);
                    CellBlock cb = new CellBlock(index + 1, tmp + 1, counter);
                    this.Blocks.Add(cb);
                    counter++;
                    index = tmp;
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedTicks;
            Debug.Print("Create Block in RAM took " + elapsedMs + " timerticks to complete (" + (elapsedMs * ((double)Stopwatch.Frequency / 1000000D) + " microseconds). It does " + Stopwatch.Frequency + " ticks every second. " + ((double)Stopwatch.Frequency / 1000000D) + " microsecond per tick.\n"));
            watch = Stopwatch.StartNew();
            this.UpdateBlocksFile();
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            Debug.Print("Create Block write File I/O took " + elapsedMs + " ms to complete.\n");
        }

        public void CreateCellTable(int machineHeight)
        {
            int cells = this.CalculateCellQuantityFromMachineHeight(machineHeight);
            for (int id = 1; id <= cells; id++)
            {
                Cell c = new Cell(id);
                this.Cells.Add(c);
            }
            this.UpdateCellsFile();
        }

        public void ExtractDrawer(int drawerID)
        {
            Drawer d = (from ret_d in this.Drawers where ret_d.Id == drawerID select ret_d).First();
            CellManagementMethods.FreeCells(this, d.FirstCellID - 1, d.Height);
            this.UpdateCellsFile();
            this.UpdateBlocksFile();
        }

        public bool InsertNewDrawer(int newDrawerID, int newDrawerHeight)
        {
            var watch = Stopwatch.StartNew();
            int initCellID = CellManagementMethods.FindFirstFreeCellIDForDrawerInsert(this, newDrawerHeight);
            if (initCellID < 0)
            {
                return false;
            }
            var d = new Drawer(newDrawerID, newDrawerHeight, initCellID);
            this.Drawers.Add(d);
            CellManagementMethods.OccupyCells(this, initCellID - 1, newDrawerHeight);
            this.UpdateCellsFile();
            this.CreateBlocks();
            this.UpdateBlocksFile();
            watch.Stop();
            Debug.Print("It took " + watch.ElapsedMilliseconds + " millisecond to complete InsertNewDrawer method in CellManager.\n");
            return true;
        }

        public void UpdateBlocksFile()
        {
            var json = JsonConvert.SerializeObject(this.Blocks, Formatting.Indented);

            if (File.Exists(CreateAndPopulateTables.JSON_BLOCK_PATH))
            {
                File.Delete(CreateAndPopulateTables.JSON_BLOCK_PATH);
                File.WriteAllText(CreateAndPopulateTables.JSON_BLOCK_PATH, json);
            }
            else
            {
                File.WriteAllText(CreateAndPopulateTables.JSON_BLOCK_PATH, json);
            }
        }

        public void UpdateCellsFile()
        {
            var watch = Stopwatch.StartNew();
            var json = JsonConvert.SerializeObject(this.Cells, Formatting.Indented);

            if (File.Exists(CreateAndPopulateTables.JSON_CELL_PATH))
            {
                File.Delete(CreateAndPopulateTables.JSON_CELL_PATH);
                File.WriteAllText(CreateAndPopulateTables.JSON_CELL_PATH, json);
            }
            else
            {
                File.WriteAllText(CreateAndPopulateTables.JSON_CELL_PATH, json);
            }
            watch.Stop();
            Debug.Print("Update Cells file took: " + watch.ElapsedMilliseconds + " milliseconds. \n");
        }

        private int GetLastUpperNotDisabledCellIndex(int cellIndex)
        {
            for (int index = cellIndex + 2; index < this.Cells.Count; index += 2)
            {
                if (this.Cells[index].Status != Status.Disabled && this.Cells[index].Status != Status.Occupied)
                {
                }
                else
                {
                    return index - 2;
                }
            }
            return (cellIndex % 2 == 0) ? this.Cells.Count - 2 : this.Cells.Count - 1;
        }

        #endregion Methods
    }

    internal static class CellManagementMethods
    {
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

        public static void CreateBlocks(CellsManager cm)
        {
            cm.CreateBlocks();
        }

        public static int FindFirstFreeCellIDForDrawerInsert(CellsManager cm, int drawerHeight)
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

        public static void FreeCells(CellsManager cm, int firstCellIndex, int drawerHeight)
        {
            int cellsToFree = drawerHeight / 25;
            for (int index = firstCellIndex; index <= firstCellIndex + cellsToFree * 2; index += 2)
            {
                cm.ChangeCellStatus(index, 0);
            }
        }

        public static int GetFreeCellQuantity(CellsManager cm)
        {
            int counter = 0;
            for (int index = 0; index < cm.Blocks.Count; index++)
            {
                counter += cm.Blocks[index].FinalIDCell - cm.Blocks[index].InitialIDCell;
            }
            return counter;
        }

        public static int GetFreeCellQuantity(CellsManager cm, Side side)
        {
            int counter = 0;
            if (side == Side.FrontEven)
            {
                for (int index = 0; index < cm.Blocks.Count; index++)
                {
                    if (cm.Blocks[index].InitialIDCell % 2 == 0)
                    {
                        counter += cm.Blocks[index].FinalIDCell - cm.Blocks[index].InitialIDCell;
                    }
                }
            }
            else
            {
                for (int index = 0; index < cm.Blocks.Count; index++)
                {
                    if (cm.Blocks[index].InitialIDCell % 2 != 0)
                    {
                        counter += cm.Blocks[index].FinalIDCell - cm.Blocks[index].InitialIDCell;
                    }
                }
            }
            return counter;
        }

        public static void OccupyCells(CellsManager cm, int firstCellindex, int drawerHeight)
        {
            int cellsToOccupy = drawerHeight / 25;
            for (int index = firstCellindex; index <= firstCellindex + cellsToOccupy * 2; index += 2)
            {
                cm.ChangeCellStatus(index, Status.Occupied);
            }
        }

        #endregion Methods
    }

    internal class Cell
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
            if (id == 1 || id == 2)
            {
                this.Status = Status.Unusable;
            }
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

    internal class CellBlock
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

    internal class Drawer
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
