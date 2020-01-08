using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ferretto.VW.App.Scaffolding.Models
{
    public class ScaffoldedEntity
    {
        internal ScaffoldedEntity(PropertyInfo property, object owner, IEnumerable<Attribute> metadata, int id): this(property, owner, metadata, id, property.DisplayName(metadata))
        {
        }

        internal ScaffoldedEntity(PropertyInfo property, object owner, IEnumerable<Attribute> metadata, int id, string caption)
        {
            this.Property = property ?? throw new ArgumentNullException(nameof(property));
            this.Instance = owner ?? throw new ArgumentNullException(nameof(owner));
            this.Id = id;
            this.Caption = caption;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Gets the metadata <see cref="Property"/> relevant metadata.
        /// </summary>
        public IEnumerable<Attribute> Metadata { get; }

        /// <summary>
        /// Gets or sets the editable property reference.
        /// </summary>
        public PropertyInfo Property
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the owner of the property info (to set the <see cref="Property"/> against).
        /// </summary>
        public object Instance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the numeric property identifier.
        /// </summary>
        public int Id
        {
            get;
        }

        /// <summary>
        /// Gets the descriptive caption for the property.
        /// </summary>
        /// <remarks>Overrides the one retrieved from the <see cref="Metadata"/> when not empty.</remarks>
        public string Caption
        {
            get; 
        }
    }
}
