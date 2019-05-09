using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Core.Models
{
    public class EnumerationString : BindableBase, IModel<string>
    {
        #region Constructors

        public EnumerationString(string id, string description)
        {
            this.Description = description;
            this.Id = id;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public string Id { get; set; }

        #endregion
    }
}
