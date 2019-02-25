namespace Ferretto.VW.Common_Utils
{
    public class InMemoryDataLayerException : DataLayerException
    {
        public InMemoryDataLayerException(DataLayerExceptionEnum exceptionEnum) : base(exceptionEnum) {
        }

        public InMemoryDataLayerException(DataLayerExceptionEnum exceptionEnum, string message) : base(message)
        {
            ConfigurationExceptionCode = exceptionEnum;
        }

        public InMemoryDataLayerException(DataLayerExceptionEnum exceptionEnum, string message,
            System.Exception inner) : base(message, inner)
        {
            ConfigurationExceptionCode = exceptionEnum;
        }
    }
}
