using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class MeasureUnitProvider : IMeasureUnitProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IMeasureUnitsDataService measureUnitsDataService;

        #endregion

        #region Constructors

        public MeasureUnitProvider(WMS.Data.WebAPI.Contracts.IMeasureUnitsDataService measureUnitsDataService)
        {
            this.measureUnitsDataService = measureUnitsDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<EnumerationString>> GetAllAsync()
        {
            try
            {
                return (await this.measureUnitsDataService.GetAllAsync())
                    .Select(c => new EnumerationString(c.Id, c.Description));
            }
            catch
            {
                return new List<EnumerationString>();
            }
        }

        public async Task<int> GetAllCountAsync()
        {
            try
            {
                return await this.measureUnitsDataService.GetAllCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<EnumerationString> GetByIdAsync(string id)
        {
            try
            {
                var measureUnit = await this.measureUnitsDataService.GetByIdAsync(id);
                return new EnumerationString(measureUnit.Id, measureUnit.Description);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
