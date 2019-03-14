using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Regions;
using Unity;

namespace Ferretto.Common.Utils.Testing
{
    [TestClass]
    public class PrismTest : UnityTest
    {
        #region Constructors

        protected PrismTest()
        {
        }

        #endregion

        #region Methods

        public override void Initialize()
        {
            base.Initialize();
            var mockRegionManager = new Mock<IRegionManager>();

            this.Container.RegisterInstance<IRegionManager>(mockRegionManager.Object);
        }

        #endregion
    }
}
