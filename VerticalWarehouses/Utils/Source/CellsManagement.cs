using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            for (int i = 1; i < cellQuantity + 1; i++)
            {
                var tmp = new Cell(i);
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

        public void ChangeCellStatus(int cellID, int newStatus)
        {
            if (cellID >= this.Cells.Count || cellID < 0)
            {
                throw new ArgumentException("CellsManagement Exception: cellID does not point to any cell in memory.", "cellID");
            }
            this.Cells[cellID].Status = newStatus;
            this.UpdateCellsFile();
        }

        public void CreateBay(int firstCell, int lastCell)
        {
            if ((firstCell % 2 == 0 && lastCell % 2 != 0) || (firstCell % 2 != 0 && lastCell % 2 == 0))
            {
                throw new ArgumentException("Cells' Management Exception: final cell not on the same side of initial cell.", "lastCell");
            }
            for (int i = firstCell; i < lastCell + 1; i += 2)
            {
                this.ChangeCellStatus(i, 1);
            }
        }

        public void CreateCellTable(int machineHeight)
        {
            int cells = this.CalculateCellQuantityFromMachineHeight(machineHeight);
            CreateAndPopulateTables.CreateCellJsonFile(cells);
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

        private void CreateBlocks()
        {
            int counter = 1;
            for (int i = 0; i < this.Cells.Count; i += 2) //odd cell's index
            {
                if (this.Cells[i].Status == 0)
                {
                    int tmp = this.GetLastUpperNotDisabledCellIndex(i);
                    CellBlock cb = new CellBlock(i + 1, tmp + 1, counter);
                    this.Blocks.Add(cb);
                    counter++;
                    i = tmp;
                }
            }
            for (int i = 1; i < this.Cells.Count; i += 2)//even cell's index
            {
                if (this.Cells[i].Status == 0)
                {
                    int tmp = this.GetLastUpperNotDisabledCellIndex(i);
                    CellBlock cb = new CellBlock(i + 1, tmp + 1, counter);
                    this.Blocks.Add(cb);
                    counter++;
                    i = tmp;
                }
            }
        }

        private int GetLastUpperNotDisabledCellIndex(int cellIndex)
        {
            for (int i = cellIndex + 2; i < this.Cells.Count; i += 2)
            {
                if (this.Cells[i].Status != 1)
                {
                }
                else
                {
                    return i - 2;
                }
            }
            return (cellIndex % 2 == 0) ? this.Cells.Count - 2 : this.Cells.Count - 1;
        }

        #endregion Methods
    }

    internal static class CellManagementMethods
    {
        #region Methods

        private static void ChangeCellStatus(CellsManagement cm, int cellID, int newStatus)
        {
            cm.Cells[cellID - 1].Status = newStatus;
            cm.UpdateCellsFile();
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

        public CellBlock(int firstCell, int lastCell, int blockID)
        {
            if ((firstCell % 2 == 0 && lastCell % 2 != 0) || (firstCell % 2 != 0 && lastCell % 2 == 0))
            {
                throw new ArgumentException("Cells' Management Exception: final cell not on the same side of initial cell.", "lastCell");
            }
            this.Area = 0;
            this.Machine = 0;
            this.InitialIDCell = firstCell;
            this.FinalIDCell = lastCell;
            this.Priority = this.InitialIDCell;
            this.BlockHeight = ((lastCell - firstCell) / 2) * 25;
            this.Side = (firstCell % 2 == 0) ? 0 : 1;
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
