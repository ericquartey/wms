namespace Ferretto.WMS.IdentityServer.Device
{
    public class DeviceAuthorizationViewModel : ConsentViewModel
    {
        #region Properties

        public bool ConfirmUserCode { get; set; }

        public string UserCode { get; set; }

        #endregion
    }
}
