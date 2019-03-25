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

        public Mock<IEventAggregator> eventAggregator;

        protected DataLayerContext context;

        protected DataLayer dataLayer;

        #endregion

        #region Properties

        public object GeneralInfoEnum { get; private set; }

        #endregion

        #region Methods

        [TestInitialize]
        public void CreateNewContext()
        {
            this.context = this.CreateContext();

            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(s => s.GetEvent<CommandEvent>()).Returns(new CommandEvent());
            mockEventAggregator.Setup(s => s.GetEvent<NotificationEvent>()).Returns(new NotificationEvent());

            this.eventAggregator = mockEventAggregator;

            var filesInfo = new FilesInfo
            {
                GeneralInfoPath = @"..\..\..\..\..\MAS_AutomationService\general_info.json",
                InstallationInfoPath = @"..\..\..\..\..\MAS_AutomationService\installation_info.json"
            };
            var iOptions = Options.Create(filesInfo);

            this.dataLayer = new DataLayer(this.context, mockEventAggregator.Object, iOptions);
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
