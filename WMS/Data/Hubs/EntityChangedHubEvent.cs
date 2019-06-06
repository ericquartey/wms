namespace Ferretto.WMS.Data.Hubs
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
