using Ferretto.VW.App.Scaffolding.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ferretto.VW.App.Scaffolding.Models
{
    public class ScaffoldedEntity
    {
        internal ScaffoldedEntity(PropertyInfo property, object owner, IEnumerable<Attribute> metadata, int id): this(property, owner, metadata, id, property.DisplayName())
        {
        }

        internal ScaffoldedEntity(PropertyInfo property, object owner, IEnumerable<Attribute> metadata, int id, string caption)
        {
            this.Property = property;
            this.Instance = owner;
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
        public string Caption
        {
            get; 
        }
    }
}
