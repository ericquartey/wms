using System;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL
{
  public static class ModelExtensions
  {
    public static void SetIfStrictlyPositive(this IModel<int> model, ref int? member, int? value)
    {
      if (value.HasValue)
      {
        if (value.Value <= 0)
        {
          throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
        }

        if (!member.HasValue || member.Value != value.Value)
        {
          member = value;
        }
      }
    }
  }
}
