using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class Area
    {
        #region Constructors

        public Area()
        {
        }

        #endregion

        #region Properties

        public IEnumerable<Bay> Bays { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        #endregion
    }
}
