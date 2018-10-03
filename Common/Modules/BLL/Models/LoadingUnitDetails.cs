using System.Collections.Generic;
using Ferretto.Common.DataModels;
using Ferretto.Common.Utils;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class LoadingUnitDetails : BusinessObject, IEntity<int>
    {
        #region Fields

        private int length;
        private int width;

        #endregion Fields

        #region Properties

        public IEnumerable<Enumeration<string>> AbcClassChoices { get; set; }

        public string AbcClassId { get; set; }

        public IEnumerable<Enumeration<int>> CellPositionChoices { get; set; }

        public int CellPositionId { get; set; }

        public string Code { get; set; }

        public IEnumerable<CompartmentDetails> Compartments { get; set; }

        public int Id { get; set; }

        public int Length
        {
            get => this.length;
            set => SetIfStrictlyPositive(ref this.length, value);
        }

        public IEnumerable<Enumeration<string>> LoadingUnitStatusChoices { get; set; }

        public string LoadingUnitStatusId { get; set; }

        public IEnumerable<Enumeration<int>> LoadingUnitTypeChoices { get; set; }

        public int LoadingUnitTypeId { get; set; }

        public int Width
        {
            get => this.width;
            set => SetIfStrictlyPositive(ref this.width, value);
        }

        #endregion Properties
    }
}
