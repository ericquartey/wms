namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IMeasureProfileFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool Enable { get; }

        ushort Profile { get; }

        #endregion
    }
}
