using Ferretto.Common.BLL.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.BusinessModels
{
    public class Enumeration : BindableBase, IBusinessObject
    {
        #region Constructors

        public Enumeration(int id, string description)
        {
            this.Description = description;
            this.Id = id;
        }

        #endregion Constructors

        #region Properties

        public string Description { get; }
        public int Id { get; set; }

        #endregion Properties
    }
}
