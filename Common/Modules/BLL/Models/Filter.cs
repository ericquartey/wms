using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class Filter : IFilter
    {
        #region Properties

        public int Count { get; set; }
        public int Id { get; set; }

        public string Name { get; set; }

        #endregion Properties
    }
}
