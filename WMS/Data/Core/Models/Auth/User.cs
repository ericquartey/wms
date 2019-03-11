namespace Ferretto.WMS.Data.Core.Models
{
    public class User : BaseModel<int>
    {
        #region Properties

        public string Login { get; set; }

        public string Password { get; set; }

        #endregion
    }
}
