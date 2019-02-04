using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core.FilteringUI;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public class FilteringChangedPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Fields

        private readonly CriteriaOperator filter;

        private readonly FilteringUIContext filteringContext;

        #endregion

        #region Constructors

        public FilteringChangedPubSubEvent(CriteriaOperator filter, FilteringUIContext filteringContext)
        {
            this.filter = filter;
            this.filteringContext = filteringContext;
        }

        #endregion

        #region Properties

        public CriteriaOperator Filter => this.filter;

        public FilteringUIContext FilteringContext => this.filteringContext;

        public string Token { get; }

        #endregion
    }
}
