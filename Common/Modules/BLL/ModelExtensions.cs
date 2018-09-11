using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Modules.BLL
{
  public static class ModelExtensions
  {
    #region Methods

    public static void SetIfStrictlyPositive(this IModel<int> model, ref int? member, int? value)
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
