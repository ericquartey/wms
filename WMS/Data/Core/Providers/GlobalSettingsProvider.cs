using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class GlobalSettingsProvider : BaseProvider, IGlobalSettingsProvider
    {
        #region Fields

        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public GlobalSettingsProvider(
            DatabaseContext dataContext,
            IMapper mapper,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.mapper = mapper;
        }

        #endregion

        #region Methods

        public IQueryable<GlobalSettings> GetAllDetailsBase()
        {
            return this.DataContext.GlobalSettings
                .ProjectTo<GlobalSettings>(this.mapper.ConfigurationProvider);
        }

        public async Task<GlobalSettings> GetGlobalSettingsAsync()
        {
            return await this.GetAllDetailsBase()
                .SingleOrDefaultAsync();
        }

        #endregion
    }
}
