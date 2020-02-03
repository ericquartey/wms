using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ferretto.VW.App.Scaffolding.Models
{
    public class ScaffoldedStructure
    {
        #region Constructors

        internal ScaffoldedStructure(string category, IEnumerable<ScaffoldedEntity> entities, IEnumerable<ScaffoldedStructure> children)
        {
            this.Category = category;
            this.Entities = new ReadOnlyCollection<ScaffoldedEntity>(entities.ToList());
            this.Children = new ReadOnlyCollection<ScaffoldedStructure>(children.ToList());
        }

        #endregion

        #region Properties

        public string AbbrevationCategory => this.Category?.Substring(0, 2);

        //public string AbbrevationCategory => this.Id > 0 ? this.Id.ToString() : this.Category?.Substring(0, 2) ?? null;

        public string Category { get; }

        public ReadOnlyCollection<ScaffoldedStructure> Children { get; }

        public string Description { get; set; }

        public ReadOnlyCollection<ScaffoldedEntity> Entities { get; }

        public int Id { get; set; }

        #endregion
    }

    public class ScaffoldedStructureRoot : ScaffoldedStructure
    {
        #region Constructors

        internal ScaffoldedStructureRoot(IEnumerable<ScaffoldedEntity> entities, IEnumerable<ScaffoldedStructure> children)
            : base(default, entities, children)
        {
            this.BuildUpDictionaries();
        }

        #endregion

        #region Methods

        private void BuildUpDictionaries()
        {
            // Dictionary<int, ScaffoldedStructure> dictionary = new Dictionary<int, ScaffoldedStructure>();
        }

        #endregion
    }
}
