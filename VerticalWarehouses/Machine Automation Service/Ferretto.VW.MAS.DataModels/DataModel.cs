using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.MAS.DataModels
{
    public class DataModel
    {
        #region Constructors

        protected DataModel()
        {
        }

        #endregion

        #region Properties

        [Key]
        public int Id { get; set; }

        #endregion
    }
}
