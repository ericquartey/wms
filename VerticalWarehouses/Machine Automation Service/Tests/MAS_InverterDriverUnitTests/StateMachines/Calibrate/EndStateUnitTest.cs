using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

//namespace MAS_InverterDriverUnitTests.StateMachines.Calibrate
//{
//    [TestClass]
//    public class EndStateUnitTest
//    {
//        #region Fields

//        private readonly ushort parameterValue;

//        private InverterMessage message;

//        #endregion

//        //[TestMethod]
//        //public void IsNotNullEndState()
//        //{
//        //    var parentStateMachineMock = new Mock<IInverterStateMachine>();
//        //    var loggerMock = new Mock<ILogger>();
//        //    var endState = new CalibrateAxisEndState(parentStateMachineMock.Object, Axis.Both, loggerMock.Object);
//        //    var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam);

//        //    Assert.IsNotNull(endState);
//        //    Assert.IsNotNull(inverterMessage);
//        //}

//        /* TEMP
//        [TestMethod]
//        public void IsTrueNotifyMessage()
//        {
//            var parentStateMachineMock = new Mock<IInverterStateMachine>();

//            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Both);

//            if (message.IsError)
//            {
//               Assert.IsTrue(errorState);
//            }
//        }
//       */
//    }
//}
