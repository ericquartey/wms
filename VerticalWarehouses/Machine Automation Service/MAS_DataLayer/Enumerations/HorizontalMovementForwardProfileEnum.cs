using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_DataLayer.Enumerations
{
    public enum HorizontalMovementForwardProfileEnum : long
    {
        TotalSteps = 0L,

        InitialSpeed = 1L,

        Step1Position = 2L,

        Step1Speed = 3L,

        Step1AccDec = 4L,

        Step2Position = 5L,

        Step2Speed = 6L,

        Step2AccDec = 7L,

        Step3Position = 8L,

        Step3Speed = 9L,

        Step3AccDec = 10L,

        Step4Position = 11L,

        Step4Speed = 12L,

        Step4AccDec = 13L,
    }
}
