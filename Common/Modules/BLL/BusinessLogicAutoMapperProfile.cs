using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Ferretto.Common.Modules.BLL.Services
{
    namespace Ferretto.Common.Modules.BLL
    {
        public class BusinessLogicAutoMapperProfile : Profile
        {
            #region Constructors

            public BusinessLogicAutoMapperProfile()
            {
                // this.CreateMap < Ferretto.Common.Models., Common.BLL.Interfaces.Models.Item > ().As<Models.Item>();
                // this.CreateMap<Models.Item, Common.Models.Item>();
            }

            #endregion Constructors
        }
    }
}
