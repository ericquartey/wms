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
//    public class HomingModeStateUnitTest
//    {
//        #region Fields

//        private readonly ushort parameterValue;

//        private InverterMessage message;

//        #endregion

//        //[TestMethod]
//        //public void IsNotNullHomingModeState()
//        //{
//        //    var parentStateMachineMock = new Mock<IInverterStateMachine>();
//        //    var loggerMock = new Mock<ILogger>();
//        //    var homingModeState = new CalibrateAxisStartState(parentStateMachineMock.Object, Axis.Both, loggerMock.Object);
//        //    var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam);

//        //    Assert.IsNotNull(homingModeState);
//        //    Assert.IsNotNull(inverterMessage);
//        //}

//        /* TEMP
//        [TestMethod]
//        public void IsTrueNotifyMessage()
//        {
//            var parentStateMachineMock = new Mock<IInverterStateMachine>();

//            var errorState = new ErrorState(parentStateMachineMock.Object, Axis.Both);
//            var shutDownState = new ShutdownState(parentStateMachineMock.Object, Axis.Both);

//            if (message.IsError)
//            {
//                Assert.IsTrue(errorState);
//            }

//            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
//                if (message.ShortPayload == this.parameterValue)
//                {
//                    Assert.IsTrue(shutDownState);
//                }
//        }
//       */
//    }
//}
