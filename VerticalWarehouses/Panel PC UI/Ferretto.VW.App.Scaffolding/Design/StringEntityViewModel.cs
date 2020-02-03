using System;
using System.Collections.Generic;
using System.Reflection;
using Ferretto.VW.App.Scaffolding.Models;

namespace Ferretto.VW.App.Scaffolding.Design
{
    public class StringEntityViewModel : ScaffoldedEntity
    {
        private static readonly Entity _instance;
        private static readonly PropertyInfo _property;
        private static readonly IEnumerable<Attribute> _metadata;

        private class Entity
        {
            [System.ComponentModel.DataAnnotations.Display(Name = "Property name")]
            public string Value { get; set; } = "Property value";
        }

        static StringEntityViewModel()
        {
            _instance = new Entity();
            _property = typeof(Entity).GetProperty(nameof(Entity.Value));
            _metadata = _property.GetCustomAttributes<Attribute>();
        }

        public StringEntityViewModel() : base(_property, _instance, _metadata, 1)
        {
        }
    }
}
