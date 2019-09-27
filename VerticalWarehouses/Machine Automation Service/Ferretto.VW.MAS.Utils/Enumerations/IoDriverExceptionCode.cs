namespace Ferretto.VW.MAS.Utils.Enumerations
{
    public enum IoDriverExceptionCode
    {
        CreationFailure = 1,

        DeviceNotConnected,

        GetIpMasterFailed,

        IoClientCreationFailed,

        SocketOpen = 5,

        RemoteEndPointCreationFailure,

        TcpClientCreationFailed,

        TcpInverterConnectionFailed,

        GetNetworkStreamFailed,

        UninitializedNetworkStream,

        MisconfiguredNetworkStream,

        NetworkStreamReadFailure,

        NetworkStreamWriteFailure,

        RequestReadOnWriteOnlyParameter,

        RequerstWriteOnReadOnlyParameter,
    }
}
