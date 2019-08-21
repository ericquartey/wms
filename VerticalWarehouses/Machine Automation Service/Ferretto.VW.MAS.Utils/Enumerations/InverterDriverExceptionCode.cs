namespace Ferretto.VW.MAS.Utils.Enumerations
{
    public enum InverterDriverExceptionCode
    {
        SocketOpen = 1,

        RemoteEndPointCreationFailure,

        TcpClientCreationFailed,

        TcpInverterConnectionFailed,

        GetNetworkStreamFailed,

        UninitializedNetworkStream,

        MisconfiguredNetworkStream,

        NetworkStreamReadFailure,

        NetworkStreamWriteFailure,

        RequestReadOnWriteOnlyParameter,

        RequestWriteOnReadOnlyParameter,

        InverterPacketMalformed
    }
}
