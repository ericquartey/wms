using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class BusinessObject : IBusinessObject
    {
        #region Constructors

        protected BusinessObject()
        { }

        #endregion Constructors

        #region Methods

        public static void SetIfStrictlyPositive(ref int? member, int? value)
        {
            if (value.HasValue)
            {
                if (value.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    member = value;
                }
            }
        }

        #endregion Methods
    }
}
