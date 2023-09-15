using System;
using System.Collections.Generic;
using System.Reflection;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.App.Scaffolding.Models
{
    public class ScaffoldedEntity
    {
        #region Constructors

        internal ScaffoldedEntity(PropertyInfo property, object owner, IEnumerable<Attribute> metadata, int id) : this(property, owner, metadata, id, property.DisplayName(metadata))
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

        #endregion

        #region Properties

        /// <summary>
        /// Gets the descriptive caption for the property.
        /// </summary>
        /// <remarks>Overrides the one retrieved from the <see cref="Metadata"/> when not empty.</remarks>
        public string Caption
        {
            get;
        }

        public bool DifferentFromJson
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
        /// Gets or sets the owner of the property info (to set the <see cref="Property"/> against).
        /// </summary>
        public object Instance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the editable property reference from json.
        /// </summary>
        public object JsonInstance
        {
            get;
            set;
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

        #endregion
    }
}
