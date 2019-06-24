using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using Prism.Ioc;
using Unity;

namespace Ferretto.WMS.App.Modules.BLL
{
    public class MapperProvider
    {
        #region Fields

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public MapperProvider(IUnityContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Methods

        public IMapper GetMapper()
        {
            var expression = new MapperConfigurationExpression();
            expression.ConstructServicesUsing(type => this.container.Resolve(type));

            expression.AddProfiles(this.GetType().Assembly);

            var configuration = new MapperConfiguration(expression);
            configuration.AssertConfigurationIsValid();

            return new Mapper(configuration, type => this.container.Resolve(type));
        }

        #endregion
    }
}
