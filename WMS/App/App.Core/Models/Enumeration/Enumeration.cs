using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Core.Models
{
    public class Enumeration : BindableBase, IModel<int>
    {
        #region Constructors

        public Enumeration(int id, string description)
        {
            this.Description = description;
            this.Id = id;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public int Id { get; set; }

        #endregion
    }
}
