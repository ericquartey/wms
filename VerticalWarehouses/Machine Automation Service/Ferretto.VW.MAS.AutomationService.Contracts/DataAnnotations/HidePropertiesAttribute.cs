using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class HidePropertiesAttribute : Attribute
    {
        #region Constructors

        public HidePropertiesAttribute(params string[] propertyList)
        {
            this.PropertyList = new List<string>(propertyList);
        }

        #endregion

        #region Properties

        public List<string> PropertyList { get; set; }

        #endregion
    }
}
