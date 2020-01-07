using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ferretto.VW.App.Scaffolding.Models
{
    public class ScaffoldedStructure
    {
        internal ScaffoldedStructure(string category, IEnumerable<ScaffoldedEntity> entities, IEnumerable<ScaffoldedStructure> children)
        {
            this.Category = category;
            this.Entities = new ReadOnlyCollection<ScaffoldedEntity>(entities.ToList());
            this.Children = new ReadOnlyCollection<ScaffoldedStructure>(children.ToList());
        }

        public string Category { get; }

        public ReadOnlyCollection<ScaffoldedEntity> Entities { get; }

        public ReadOnlyCollection<ScaffoldedStructure> Children { get; }
    }

    public class ScaffoldedStructureRoot : ScaffoldedStructure
    {
        internal ScaffoldedStructureRoot(IEnumerable<ScaffoldedEntity> entities, IEnumerable<ScaffoldedStructure> children)
            : base(default, entities, children)
        {
            this.BuildUpDictionaries();
        }

        private void BuildUpDictionaries()
        {
            // Dictionary<int, ScaffoldedStructure> dictionary = new Dictionary<int, ScaffoldedStructure>();
        }
    }
}
