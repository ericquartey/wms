using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Regions;

namespace Ferretto.Common.Utils.Testing
{
    [TestClass]
    public class PrismTest : UnityTest
    {
        protected PrismTest()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            var mockRegionManager = new Mock<IRegionManager>();

            this.Container.RegisterInstance<IRegionManager>(mockRegionManager.Object);
        }
    }
}
