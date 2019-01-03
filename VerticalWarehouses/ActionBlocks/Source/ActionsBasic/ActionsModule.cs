using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.VW.ActionBlocks.Source.ActionsBasic
{
    public class ActionsModule : IModule
    {
        #region Fields

        public IUnityContainer Container;

        #endregion Fields

        #region Constructors

        public ActionsModule(IUnityContainer _container)
        {
            this.Container = _container;
            var positioningDrawerInstance = new PositioningDrawer();
            var calibrateVerticalAxisInstance = new CalibrateVerticalAxis();
            var drawerWeightDetection = new DrawerWeightDetection();

            this.Container.RegisterInstance<IPositioningDrawer>(positioningDrawerInstance);
            this.Container.RegisterInstance<ICalibrateVerticalAxis>(calibrateVerticalAxisInstance);
            this.Container.RegisterInstance<IDrawerWeightDetection>(drawerWeightDetection);

            positioningDrawerInstance.InitializeAction(this.Container);
        }

        #endregion Constructors

        #region Methods

        public void Initialize()
        {
        }

        #endregion Methods
    }
}
