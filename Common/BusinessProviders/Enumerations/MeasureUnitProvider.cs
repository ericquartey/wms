using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
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
            return (await this.measureUnitsDataService.GetAllAsync())
                .Select(c => new EnumerationString(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.measureUnitsDataService.GetAllCountAsync();
        }

        public async Task<EnumerationString> GetByIdAsync(string id)
        {
            var measureUnit = await this.measureUnitsDataService.GetByIdAsync(id);
            return new EnumerationString(measureUnit.Id, measureUnit.Description);
        }

        #endregion
    }
}
