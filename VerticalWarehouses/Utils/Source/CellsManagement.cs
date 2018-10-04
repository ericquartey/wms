using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Ferretto.VW.Utils.Source
{
    public static class CreateAndPopulateTables
    {
        #region Fields

        public static readonly string JSON_BLOCK_PATH = Environment.CurrentDirectory + "/blockstable.json";
        public static readonly string JSON_CELL_PATH = Environment.CurrentDirectory + "/cellstable.json";

        #endregion Fields

        #region Methods

        public static void CreateCellJsonFile(int cellQuantity)
        {
            List<Cell> cellsToJson = new List<Cell>();
            for (int id = 1; id <= cellQuantity; id++)
            {
                var tmp = new Cell(id);
                cellsToJson.Add(tmp);
            }
            var json = JsonConvert.SerializeObject(cellsToJson, Formatting.Indented);

            if (File.Exists(JSON_CELL_PATH))
            {
                File.Delete(JSON_CELL_PATH);
                File.WriteAllText(JSON_CELL_PATH, json);
            }
            else
            {
                File.WriteAllText(JSON_CELL_PATH, json);
            }
        }

        #endregion Methods
    }

    public class CellsManagement
    {
        #region Fields

        private List<CellBlock> blocks = new List<CellBlock>();
        private List<Cell> cells = new List<Cell>();

        #endregion Fields

        #region Constructors

        public CellsManagement()
        {
            //if (File.Exists(CreateAndPopulateTables.JSON_CELL_PATH))
            //{
            //    string jsonCells = File.ReadAllText(CreateAndPopulateTables.JSON_CELL_PATH);
            //    this.Cells = JsonConvert.DeserializeObject<List<Cell>>(jsonCells);
            //}
            //if (File.Exists(CreateAndPopulateTables.JSON_BLOCK_PATH))
            //{
            //    string jsonBlocks = File.ReadAllText(CreateAndPopulateTables.JSON_BLOCK_PATH);
            //    this.Blocks = JsonConvert.DeserializeObject<List<CellBlock>>(jsonBlocks);
            //}
            //else
            //{
            //    this.CreateBlocks();
            //    this.UpdateBlocksFile();
            //}
        }

        #endregion Constructors

        #region Properties

        internal List<CellBlock> Blocks { get => this.blocks; set => this.blocks = value; }

        internal List<Cell> Cells { get => this.cells; set => this.cells = value; }

        #endregion Properties

        #region Methods

        public int CalculateCellQuantityFromMachineHeight(int machineHeight)
        {
            int cells = machineHeight / 25;
            return cells * 2;
        }

        public void ChangeCellStatus(int cellIndex, int newStatus)
        {
            if (cellIndex >= this.Cells.Count || cellIndex < 0)
            {
                throw new ArgumentException("CellsManagement Exception: cellID does not point to any cell in memory.", "cellID");
            }
            this.Cells[cellIndex].Status = newStatus;
            this.UpdateCellsFile();
        }

        public void CreateBay(int firstCellID, int lastCellID)
        {
            Debug.Print("firstCell: " + firstCellID + ", lastCell: " + lastCellID + "\n");
            if ((firstCellID % 2 == 0 && lastCellID % 2 != 0) || (firstCellID % 2 != 0 && lastCellID % 2 == 0))
            {
                throw new ArgumentException("Cells' Management Exception: final cell not on the same side of initial cell.", "lastCell");
            }
            for (int id = firstCellID; id <= lastCellID; id += 2)
            {
                this.ChangeCellStatus(id - 1, 1);
            }
        }

        public void CreateBlocks()
        {
            this.Blocks = null;
            this.Blocks = new List<CellBlock>();
            int counter = 1;
            for (int index = 0; index < this.Cells.Count; index += 2) //odd cell's index
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
            for (int index = 1; index < this.Cells.Count; index += 2)//even cell's index
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

        public void InsertNewDrawer(int newDrawerID, int newDrawerHeight)
        {
            int initCellID = CellManagementMethods.FindFirstFreeCellIDForDrawerInsert(this, newDrawerHeight);
            CellManagementMethods.OccupyCells(this, initCellID - 1, newDrawerHeight);
            this.UpdateCellsFile();
            this.CreateBlocks();
            this.UpdateBlocksFile();
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
        }

        private int GetLastUpperNotDisabledCellIndex(int cellIndex)
        {
            for (int index = cellIndex + 2; index < this.Cells.Count; index += 2)
            {
                if (this.Cells[index].Status != 1 && this.Cells[index].Status != 2)
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

        public static void CreateBlocks(CellsManagement cm)
        {
            cm.CreateBlocks();
        }

        public static int FindFirstFreeCellIDForDrawerInsert(CellsManagement cm, int drawerHeight)
        {
            var tempCellBlockIenumerable = cm.Blocks.Where(x => x.BlockHeight > drawerHeight);
            tempCellBlockIenumerable = tempCellBlockIenumerable.OrderBy(x => x.Priority);
            var tempCellBlockList = tempCellBlockIenumerable.ToList();

            Debug.Print("First cellblock free: " + tempCellBlockList[0].InitialIDCell + "\n");
            //TODO!!!!!
            //cm.Blocks.OrderBy(x => x.Priority);
            //List<CellBlock> cb = cm.Blocks.FindAll(x => x.BlockHeight > drawerHeight);
            //return cb[0].InitialIDCell;
            return tempCellBlockList[0].InitialIDCell;
        }

        public static void FreeCells(CellsManagement cm, int firstCellIndex, int drawerHeight)
        {
            int cellsToFree = drawerHeight / 25;
            for (int index = firstCellIndex; index <= firstCellIndex + cellsToFree * 2; index += 2)
            {
                cm.ChangeCellStatus(index, 0);
            }
        }

        public static int GetFreeCellQuantityInMachine(CellsManagement cm)
        {
            int counter = 0;
            for (int index = 0; index < cm.Blocks.Count; index++)
            {
                counter += cm.Blocks[index].FinalIDCell - cm.Blocks[index].InitialIDCell;
            }
            return counter;
        }

        public static int GetFreeCellQuantityInMachineSide(CellsManagement cm, int side)
        {
            int counter = 0;
            if (side == 0)
            {
                for (int index = 0; index < cm.Blocks.Count; index++)
                {
                    if (cm.Blocks[index].Side == 1)
                    {
                        continue;
                    }
                    counter += cm.Blocks[index].FinalIDCell - cm.Blocks[index].InitialIDCell;
                }
            }
            else
            {
                for (int index = 0; index < cm.Blocks.Count; index++)
                {
                    if (cm.Blocks[index].Side == 0)
                    {
                        continue;
                    }
                    counter += cm.Blocks[index].FinalIDCell - cm.Blocks[index].InitialIDCell;
                }
            }
            return counter;
        }

        public static void OccupyCells(CellsManagement cm, int firstCellindex, int drawerHeight)
        {
            int cellsToOccupy = drawerHeight / 25;
            for (int index = firstCellindex; index <= firstCellindex + cellsToOccupy * 2; index += 2)
            {
                cm.ChangeCellStatus(index, 2);
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
        private int side; // 0 = even ; 1 = odd
        private int status;

        #endregion Fields

        // status code: 0 = free; 1 = disabled; 2 = occupied; 3 = unusable

        #region Constructors

        public Cell(int id)
        {
            this.IdCell = id;
            this.Priority = id;
            if (id % 2 == 0) //if id is even
            {
                this.Coord = (id == 2) ? 25 : 25 * (id / 2);
                this.Side = 0;
            }
            else //if id is odd
            {
                this.Coord = (id == 1) ? 25 : 25 * ((id / 2) + 1);
                this.Side = 1;
            }
            this.Status = 0;
        }

        #endregion Constructors

        #region Properties

        public Int32 Coord { get => this.coord; set => this.coord = value; }
        public Int32 IdCell { get => this.idCell; set => this.idCell = value; }
        public Int32 Priority { get => this.priority; set => this.priority = value; }
        public Int32 Side { get => this.side; set => this.side = value; }
        public Int32 Status { get => this.status; set => this.status = value; }

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
        private int side;

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
            this.Side = (firstCellID % 2 == 0) ? 0 : 1;
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
        public Int32 Side { get => this.side; set => this.side = value; }

        #endregion Properties
    }
}
