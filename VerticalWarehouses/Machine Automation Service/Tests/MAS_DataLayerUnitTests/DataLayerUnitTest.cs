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

        protected DataLayerContext context;

        protected DataLayer dataLayer;

        protected decimal setDecResolution = 100.01m;

        protected int setIntBayHeight = 100;

        protected string setStrInvAddress = "169.254.231.248";

        protected string strBayHeightFromGround = "10.000025";

        #endregion

        #region Methods

        [TestInitialize]
        public void CreateNewContext()
        {
            this.context = this.CreateContext();
            // this.InitializeContext(context);

            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(s => s.GetEvent<CommandEvent>()).Returns(new CommandEvent());
            mockEventAggregator.Setup(s => s.GetEvent<NotificationEvent>()).Returns(new NotificationEvent());
            this.dataLayer = new DataLayer(this.context, mockEventAggregator.Object);
        }

        [TestMethod]
        public void NewGetDecimalConfigurationValue()
        {
            var decimalValue = new ConfigurationValue { VarName = ConfigurationValueEnum.resolution, VarType = DataTypeEnum.decimalType, VarValue = this.setDecResolution.ToString() };

            this.context.ConfigurationValues.Add(decimalValue);

            this.context.SaveChanges();

            Assert.AreEqual(this.setDecResolution, this.dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.resolution));
        }

        [TestMethod]
        public void NewGetIntegerConfigurationValue()
        {
            var integerValue = new ConfigurationValue { VarName = ConfigurationValueEnum.bayHeight, VarType = DataTypeEnum.integerType, VarValue = this.setIntBayHeight.ToString() };

            this.context.ConfigurationValues.Add(integerValue);

            this.context.SaveChanges();

            Assert.AreEqual(this.setIntBayHeight, this.dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.bayHeight));
        }

        [TestMethod]
        public void NewGetIPAddressConfigurationValue()
        {
            var ipAddrrValue = new ConfigurationValue { VarName = ConfigurationValueEnum.InverterAddress, VarType = DataTypeEnum.IPAddressType, VarValue = this.setStrInvAddress };

            this.context.ConfigurationValues.Add(ipAddrrValue);

            this.context.SaveChanges();

            Assert.AreEqual(this.setStrInvAddress, this.dataLayer.GetIPAddressConfigurationValue(ConfigurationValueEnum.InverterAddress).ToString());
        }

        [TestMethod]
        public void NewGetStringConfigurationValue()
        {
            var stringBHFGrn = new ConfigurationValue { VarName = ConfigurationValueEnum.bayHeightFromGround, VarType = DataTypeEnum.stringType, VarValue = this.strBayHeightFromGround };

            this.context.ConfigurationValues.Add(stringBHFGrn);

            this.context.SaveChanges();

            Assert.AreEqual(this.strBayHeightFromGround, this.dataLayer.GetStringConfigurationValue(ConfigurationValueEnum.bayHeightFromGround));
        }

        protected DataLayerContext CreateContext()
        {
            return new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>()
                    .UseInMemoryDatabase(this.GetType().FullName)
                    .Options);
        }

        #endregion

        // TEMP
        //private void InitializeContext(DataLayerContext context)
        //{
        //}
    }
}
