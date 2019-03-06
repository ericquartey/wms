using System;

namespace Ferretto.VW.Utils.Source.CellsManagement
{
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

        public int FirstCellID { get; set; }

        public int HeightMillimiters
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
                }
            }
        }

        public int Id { get; set; }

        #endregion Properties
    }
}
