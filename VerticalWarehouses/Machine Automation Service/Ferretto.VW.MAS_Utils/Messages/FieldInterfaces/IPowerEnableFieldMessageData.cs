using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IPowerEnableFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool Enable { get; }

        #endregion
    }
}
