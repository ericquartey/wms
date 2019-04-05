namespace Ferretto.VW.MAS_Utils.Enumerations
{
    public enum DataLayerPersistentExceptionCode
    {
        ValueNotFound = 001,

        DataContextNotValid = 002,

        ParseValue = 003,

        PrimaryPartitionFailure = 010,

        PrimaryAndSecondaryPartitionFailure = 011,

        SecondaryPartitionFailure = 012,
    }
}
