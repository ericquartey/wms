namespace Ferretto.WMS.Data.Hubs.Models
{
    public class EntityChangedHubEvent
    {
        #region Properties

        public string EntityType { get; set; }

        public string Id { get; set; }

        public HubEntityOperation Operation { get; set; }

        #endregion
    }
}
