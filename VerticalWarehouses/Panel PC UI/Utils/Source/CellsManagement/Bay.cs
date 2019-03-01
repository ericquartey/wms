using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Utils.Source.CellsManagement
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

        public int DrawerID { get; set; }
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
        public bool Occupied { get; set; }

        #endregion Properties
    }
}
