using System.Linq;
using System.Reflection;

namespace Ferretto.VW.MAS.DataModels.Extensions
{
    public static class MachineErrorCodeExtension
    {
        #region Methods

        public static string GetDescription(this MachineErrorCode machineErrorCode)
        {
            return machineErrorCode.GetType()
                                   .GetField(machineErrorCode.ToString())
                                   .GetCustomAttributes<ErrorDescriptionAttribute>(false)
                                   .FirstOrDefault()?.Description;
        }

        public static string GetReason(this MachineErrorCode machineErrorCode)
        {
            return machineErrorCode.GetType()
                                   .GetField(machineErrorCode.ToString())
                                   .GetCustomAttributes<ErrorDescriptionAttribute>(false)
                                   .FirstOrDefault()?.Reason;
        }

        #endregion
    }
}
