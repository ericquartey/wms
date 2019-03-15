using System;
using System.IO;
using System.Linq;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

            var path = Directory.GetCurrentDirectory();

            var filesInfo = new FilesInfo
            {
                GeneralInfoPath = "..\\..\\..\\..\\..\\MAS_AutomationService\\general_info.json",
                InstallationInfoPath = "..\\..\\..\\..\\..\\MAS_AutomationService\\installation_info.json"
            };
            var iOptions = Options.Create(filesInfo);

            this.dataLayer = new DataLayer(this.context, mockEventAggregator.Object, iOptions);
        }

        [TestMethod]
        public void GetBoolConfigurationValue()
        {
            var alfaNum1 = true;

            var stringAN1 = new ConfigurationValue { VarName = ConfigurationValueEnum.Alfa_Num_1, VarType = DataTypeEnum.booleanType, VarValue = alfaNum1.ToString() };

            this.context.ConfigurationValues.Add(stringAN1);

            this.context.SaveChanges();

            Assert.AreEqual(alfaNum1, this.dataLayer.GetBoolConfigurationValue(ConfigurationValueEnum.Alfa_Num_1));
        }

        [TestMethod]
        public void GetDateTimeConfigurationValue()
        {
            var strInstallationDate = "2018-10-23T15:32:21.9961723+02:00";

            if (DateTime.TryParse(strInstallationDate, out var dateTimeInstallationDate))
            {
                this.dataLayer.SetDateTimeConfigurationValue(ConfigurationValueEnum.Installation_Date, dateTimeInstallationDate);
                var returnDateTime = this.dataLayer.GetDateTimeConfigurationValue(ConfigurationValueEnum.Installation_Date);
                Assert.AreEqual(dateTimeInstallationDate.ToString(), returnDateTime.ToString());
            }
        }

        [TestMethod]
        public void GetDecimalConfigurationValue()
        {
            var setDecResolution = 100.01m;

            var decimalValue = new ConfigurationValue { VarName = ConfigurationValueEnum.resolution, VarType = DataTypeEnum.decimalType, VarValue = setDecResolution.ToString() };

            this.context.ConfigurationValues.Add(decimalValue);

            this.context.SaveChanges();

            Assert.AreEqual(setDecResolution, this.dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.resolution));
        }

        [TestMethod]
        public void GetIntegerConfigurationValue()
        {
            var setTypeBay1 = 1;
            var integerValue = new ConfigurationValue { VarName = ConfigurationValueEnum.Type_Bay_1, VarType = DataTypeEnum.integerType, VarValue = setTypeBay1.ToString() };

            this.context.ConfigurationValues.Add(integerValue);

            this.context.SaveChanges();

            Assert.AreEqual(setTypeBay1, this.dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.Type_Bay_1));
        }

        [TestMethod]
        public void GetIPAddressConfigurationValue()
        {
            var setStrInvAddress = "169.254.231.248";

            var ipAddrrValue = new ConfigurationValue { VarName = ConfigurationValueEnum.InverterAddress, VarType = DataTypeEnum.IPAddressType, VarValue = setStrInvAddress };

            this.context.ConfigurationValues.Add(ipAddrrValue);

            this.context.SaveChanges();

            Assert.AreEqual(setStrInvAddress, this.dataLayer.GetIPAddressConfigurationValue(ConfigurationValueEnum.InverterAddress).ToString());
        }

        [TestMethod]
        public void GetStringConfigurationValue()
        {
            var strAddress = "Corso Andrea Palladio";

            var stringA = new ConfigurationValue { VarName = ConfigurationValueEnum.Address, VarType = DataTypeEnum.stringType, VarValue = strAddress };

            this.context.ConfigurationValues.Add(stringA);

            this.context.SaveChanges();

            Assert.AreEqual(strAddress, this.dataLayer.GetStringConfigurationValue(ConfigurationValueEnum.Address));
        }

        [TestMethod]
        public void ReadGeneralInfoJson()
        {
            this.dataLayer.LoadConfigurationValuesInfo(InfoFilesEnum.GeneralInfo);

            Assert.IsTrue(this.context.ConfigurationValues.Any());
        }

        [TestMethod]
        public void ReadInstallationInfoJson()
        {
            this.dataLayer.LoadConfigurationValuesInfo(InfoFilesEnum.InstallationInfo);

            Assert.IsTrue(this.context.ConfigurationValues.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(InMemoryDataLayerException))]
        public void TestErrorLoadFilesJson()
        {
            // INFO Arrange - Empty

            // INFO Act
            this.dataLayer.LoadConfigurationValuesInfo((InfoFilesEnum)3);

            // INFO Assert - Expects exception
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
