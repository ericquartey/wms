using System.ComponentModel.DataAnnotations;

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
