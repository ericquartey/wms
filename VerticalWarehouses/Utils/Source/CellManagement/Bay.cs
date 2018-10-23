using System;

namespace Ferretto.VW.Utils.Source.CellManagement
{
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
                }
            }
        }

        public Int32 Id { get; set; }
        public Boolean Occupied { get; set; }

        #endregion Properties
    }
}
