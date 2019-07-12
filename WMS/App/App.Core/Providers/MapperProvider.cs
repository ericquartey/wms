using AutoMapper;
using AutoMapper.Configuration;
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
