using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IWmsGridViewModel 
    {
        void SetDataSource(object source);

        void RefreshGrid();
    }
}
