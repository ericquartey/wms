using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.ServiceLocation;

namespace Ferretto.Common.Utils.Testing
{
    [TestClass]
    public class UnityTest
    {
        #region Fields

        private UnityContainer container;

        private TestContext testContext;

        #endregion

        #region Constructors

        protected UnityTest()
        {
        }

        #endregion

        #region Properties

        public UnityContainer Container => this.container;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get => this.testContext;
            set => this.testContext = value;
        }

        #endregion

        #region Methods

        public virtual void Initialize()
        {
            this.container = new UnityContainer();
            var locator = new UnityServiceLocator(this.container);

            ServiceLocator.SetLocatorProvider(() => locator);

            this.container.RegisterInstance<IUnityContainer>(this.container);
        }

        #endregion
    }
}
