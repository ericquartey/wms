using System;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus
{
    public class AngInverterStatus : InverterStatusBase, IAngInverterStatus
    {
        #region Constructors

        public AngInverterStatus(byte systemIndex)
        {
            this.SystemIndex = systemIndex;
        }

        #endregion

        #region Properties

        public IHomingControlWord HomingControlWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.Homing)
                {
                    throw new InvalidOperationException("Inverter is not configured for Homing Mode");
                }

                if (this.controlWord is IHomingControlWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType()} is not compatible with Homing Mode");
            }
        }

        public IHomingStatusWord HomingStatusWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.Homing)
                {
                    throw new InvalidOperationException("Inverter is not configured for Homing Mode");
                }

                if (this.statusWord is IHomingStatusWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType()} is not compatible with Homing Mode");
            }
        }

        public IPositionControlWord PositionControlWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.Position)
                {
                    throw new InvalidOperationException("Inverter is not configured for Position Mode");
                }

                if (this.controlWord is IPositionControlWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Control Word Type {this.controlWord.GetType()} is not compatible with Position Mode");
            }
        }

        public IPositionStatusWord PositionStatusWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.Position)
                {
                    throw new InvalidOperationException("Inverter is not configured for Position Mode");
                }

                if (this.statusWord is IPositionStatusWord word)
                {
                    return word;
                }

                throw new InvalidCastException($"Current Status Word Type {this.statusWord.GetType()} is not compatible with Position Mode");
            }
        }

        #endregion
    }
}
