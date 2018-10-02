using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Ferretto.VW.Utils.Source
{
    public static class CreateAndPopulateTestTables
    {
        #region Fields

        public const string JSON_PATH = "C:/Users/npadovani/Desktop/cellsfile.json";

        #endregion Fields

        #region Methods

        public static void CreateJsonFile()
        {
            List<Cell> cellsToJson = new List<Cell>();
            for (int i = 1; i < 4001; i++)
            {
                var tmp = new Cell(i);
                cellsToJson.Add(tmp);
            }

            var json = JsonConvert.SerializeObject(cellsToJson, Formatting.Indented);

            if (File.Exists(JSON_PATH))
            {
                File.Delete(JSON_PATH);
                File.WriteAllText(JSON_PATH, json);
            }
            else
            {
                File.WriteAllText(JSON_PATH, json);
            }
        }

        #endregion Methods
    }

    public class CellsManagement
    {
        #region Fields

        private List<CellBlock> blocks;
        private List<Cell> cells;

        #endregion Fields

        #region Constructors

        public CellsManagement()
        {
            string json = File.ReadAllText(CreateAndPopulateTestTables.JSON_PATH);
            this.cells = JsonConvert.DeserializeObject<List<Cell>>(json);
        }

        #endregion Constructors

        #region Properties

        internal List<CellBlock> Blocks { get => this.blocks; set => this.blocks = value; }
        internal List<Cell> Cells { get => this.cells; set => this.cells = value; }

        #endregion Properties
    }

    internal static class CellManagementMethods
    {
    }

    internal class Cell
    {
        #region Fields

        private int coord;
        private int idCell;
        private int priority;
        private int side;
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

        public CellBlock()
        {
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
