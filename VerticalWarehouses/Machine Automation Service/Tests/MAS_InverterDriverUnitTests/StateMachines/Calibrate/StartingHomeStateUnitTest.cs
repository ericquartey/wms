﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
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
//    public class StartingHomeStateUnitTest
//    {
//        #region Fields

//        private readonly ushort parameterValue;

//        private InverterMessage message;

//        #endregion

//        //[TestMethod]
//        //public void IsNotNullStartingHomeState()
//        //{
//        //    var parentStateMachineMock = new Mock<IInverterStateMachine>();
//        //    var loggerMock = new Mock<ILogger>();
//        //    var startingHomeState = new CalibrateAxisStartHomingState(parentStateMachineMock.Object, Axis.Both, loggerMock.Object);
//        //    var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam);

//        //    Assert.IsNotNull(startingHomeState);
//        //    Assert.IsNotNull(inverterMessage);
//        //}

//        /*TEMP
//        [TestMethod]
//        public void IsTrueNotifyMessage()
//        {
//            var parentStateMachineMock = new Mock<IInverterStateMachine>();

//            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Both);
//            var endState = new EndState(parentStateMachineMock.Object, Axis.Both);

//            if (message.IsError)
//            {
//                Assert.IsTrue(errorState);
//            }

//            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
//                if (message.ShortPayload == this.parameterValue)
//                {
//                    Assert.IsTrue(endState);
//                }
//        }
//       */
//    }
//}
