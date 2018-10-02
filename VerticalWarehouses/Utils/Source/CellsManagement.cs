using System;
using System.IO;
using Newtonsoft.Json;

namespace Ferretto.VW.Utils.Source
{
    public static class CreateAndPopulateTestTables
    {
        #region Fields

        private static readonly string jsonPath = Environment.CurrentDirectory;

        #endregion Fields

        #region Methods

        public static void CreateJsonFile()
        {
            string json = "";
            for (int i = 0; i < 4000; i++)
            {
                var tmp = new Cell(i);
                json += JsonConvert.SerializeObject(tmp);
            }
            File.WriteAllText(jsonPath, json);
        }

        #endregion Methods
    }

    public class CellsManagement
    {
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
            if (id % 2 == 0)
            {
                this.Coord = (id == 2) ? 25 : 25 * (id / 2);
                this.Side = 0;
            }
            else
            {
                this.Coord = (id == 2) ? 25 : 25 * ((id / 2) + 1);
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
