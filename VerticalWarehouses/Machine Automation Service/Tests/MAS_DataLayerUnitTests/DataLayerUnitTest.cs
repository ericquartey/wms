using System.Linq;
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

        #endregion

        #region Methods

        [TestInitialize]
        public void CreateNewContext()
        {
            this.context = this.CreateContext();

            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(s => s.GetEvent<CommandEvent>()).Returns(new CommandEvent());
            mockEventAggregator.Setup(s => s.GetEvent<NotificationEvent>()).Returns(new NotificationEvent());
            this.dataLayer = new DataLayer(this.context, mockEventAggregator.Object);
        }

        [TestMethod]
        public void NewGetDecimalConfigurationValue()
        {
            var setDecResolution = 100.01m;

            var decimalValue = new ConfigurationValue { VarName = ConfigurationValueEnum.resolution, VarType = DataTypeEnum.decimalType, VarValue = setDecResolution.ToString() };

            this.context.ConfigurationValues.Add(decimalValue);

            this.context.SaveChanges();

            Assert.AreEqual(setDecResolution, this.dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.resolution));
        }

        [TestMethod]
        public void NewGetIntegerConfigurationValue()
        {
            var setIntBayHeight = 100;
            var integerValue = new ConfigurationValue { VarName = ConfigurationValueEnum.bayHeight, VarType = DataTypeEnum.integerType, VarValue = setIntBayHeight.ToString() };

            this.context.ConfigurationValues.Add(integerValue);

            this.context.SaveChanges();

            Assert.AreEqual(setIntBayHeight, this.dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.bayHeight));
        }

        [TestMethod]
        public void NewGetIPAddressConfigurationValue()
        {
            var setStrInvAddress = "169.254.231.248";

            var ipAddrrValue = new ConfigurationValue { VarName = ConfigurationValueEnum.InverterAddress, VarType = DataTypeEnum.IPAddressType, VarValue = setStrInvAddress };

            this.context.ConfigurationValues.Add(ipAddrrValue);

            this.context.SaveChanges();

            Assert.AreEqual(setStrInvAddress, this.dataLayer.GetIPAddressConfigurationValue(ConfigurationValueEnum.InverterAddress).ToString());
        }

        [TestMethod]
        public void NewGetStringConfigurationValue()
        {
            var strBayHeightFromGround = "10.000025";

            var stringBHFGrn = new ConfigurationValue { VarName = ConfigurationValueEnum.bayHeightFromGround, VarType = DataTypeEnum.stringType, VarValue = strBayHeightFromGround };

            this.context.ConfigurationValues.Add(stringBHFGrn);

            this.context.SaveChanges();

            Assert.AreEqual(strBayHeightFromGround, this.dataLayer.GetStringConfigurationValue(ConfigurationValueEnum.bayHeightFromGround));
        }

        [TestMethod]
        public void ReadGeneralInfoJson()
        {
            var generalInfoPath = "..\\..\\..\\..\\..\\MAS_AutomationService\\general_info.json";

            this.dataLayer.LoadGeneralInfo(generalInfoPath);

            Assert.IsTrue(this.context.GeneralInfos.Any());
        }

        protected DataLayerContext CreateContext()
        {
            return new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>()
                    .UseInMemoryDatabase(this.GetType().FullName)
                    .Options);
        }

        #endregion
    }
}
