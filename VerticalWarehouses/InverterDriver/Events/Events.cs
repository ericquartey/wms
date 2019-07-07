namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    ///Delegate for the [Connected] event.
    /// </summary>
    public delegate void ConnectedEventHandler(object sender, ConnectedEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [EnquiryTelegramDone] event.
    /// </summary>
    public delegate void EnquiryTelegramDoneEventHandler(object sender, EnquiryTelegramDoneEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [Error] event.
    /// </summary>
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [LastRequestDone] event.
    /// </summary>
    public delegate void LastRequestDoneEventHandler(object sender, LastRequestDoneEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [SelectTelegramDone] event.
    /// </summary>
    public delegate void SelectTelegramDoneEventHandler(object sender, SelectTelegramDoneEventArgs eventArgs);
}
