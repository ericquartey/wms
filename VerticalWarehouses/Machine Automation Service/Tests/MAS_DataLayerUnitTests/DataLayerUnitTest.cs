using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public class DataLayerUnitTest
    {
        #region Fields

        public decimal setDecResolution = 100.01m;

        public int setIntBayHeight = 100;

        public string setStrInvAddress = "169.254.231.248";

        public string strBayHeightFromGround = "10.000025";

        private DataLayer dataLayer;

        #endregion

        #region Methods

        [TestInitialize]
        public void CreateNewContext()
        {
            var context = this.CreateContext();
            this.InitializeContext(context);

            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(s => s.GetEvent<CommandEvent>()).Returns(new CommandEvent());
            mockEventAggregator.Setup(s => s.GetEvent<NotificationEvent>()).Returns(new NotificationEvent());
            this.dataLayer = new DataLayer(context, mockEventAggregator.Object);
        }

        [TestMethod]
        public void NewGetDecimalConfigurationValue()
        {
            Assert.AreEqual(this.setDecResolution, this.dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.resolution));
        }

        [TestMethod]
        public void NewGetIntegerConfigurationValue()
        {
            Assert.AreEqual(this.setIntBayHeight, this.dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.bayHeight));
        }

        [TestMethod]
        public void NewGetIPAddressConfigurationValue()
        {
            Assert.AreEqual(this.setStrInvAddress, this.dataLayer.GetIPAddressConfigurationValue(ConfigurationValueEnum.InverterAddress).ToString());
        }

        [TestMethod]
        public void NewGetStringConfigurationValue()
        {
            Assert.AreEqual(this.strBayHeightFromGround, this.dataLayer.GetStringConfigurationValue(ConfigurationValueEnum.bayHeightFromGround));
        }

        protected DataLayerContext CreateContext()
        {
            return new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>()
                    .UseInMemoryDatabase(this.GetType().FullName)
                    .Options);
        }

        private void InitializeContext(DataLayerContext context)
        {
            var decimalValue = new ConfigurationValue { VarName = ConfigurationValueEnum.resolution, VarType = DataTypeEnum.decimalType, VarValue = this.setDecResolution.ToString() };
            var integerValue = new ConfigurationValue { VarName = ConfigurationValueEnum.bayHeight, VarType = DataTypeEnum.integerType, VarValue = this.setIntBayHeight.ToString() };
            var ipAddrrValue = new ConfigurationValue { VarName = ConfigurationValueEnum.InverterAddress, VarType = DataTypeEnum.IPAddressType, VarValue = this.setStrInvAddress };
            var stringBHFGrn = new ConfigurationValue { VarName = ConfigurationValueEnum.bayHeightFromGround, VarType = DataTypeEnum.stringType, VarValue = this.strBayHeightFromGround };

            context.ConfigurationValues.Add(integerValue);
            context.ConfigurationValues.Add(decimalValue);
            context.ConfigurationValues.Add(ipAddrrValue);
            context.ConfigurationValues.Add(stringBHFGrn);

            context.SaveChanges();
        }

        #endregion
    }
}
