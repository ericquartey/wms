using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IWmsGridViewModel
    {
        IFilter CurrentFilter { get; set; }

        void RefreshGrid();
    }
}
