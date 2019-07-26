namespace Ferretto.Common.DataModels
{
    public class GlobalSettings : IDataModel<int>
    {
        #region Properties

        public int Id { get; set; }

        public double MinStepCompartment { get; set; }

        #endregion
    }
}
