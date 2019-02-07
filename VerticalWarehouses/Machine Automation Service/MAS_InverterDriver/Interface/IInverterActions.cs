namespace Ferretto.VW.MAS_InverterDriver.Interface
{
    public interface IInverterActions
    {
        event EndEventHandler EndEvent;
        event ErrorEventHandler ErrorEvent;
    }
    
}
