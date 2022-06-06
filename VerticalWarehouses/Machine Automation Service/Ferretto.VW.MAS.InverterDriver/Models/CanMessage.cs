using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class CanMessage
    {
        #region Fields

        private static readonly Dictionary<ulong, string> abortStrings = new Dictionary<ulong, string>()
        {
            {0, " No error" },
            {1, " Server error: too many SDO objects" },
            {2, " Server error: SDO object not found" },
            {3, " Server error: timeout" },
            {4, " Server error: SDO read received length 0" },
            {0x06010000, " Unsupported access to an object. Parameter cannot be written or read" },
            {0x06020000, " Object does not exist. Parameter does not exist" },
            {0x06040047, " General internal incompatibility in the device. Data sets differ" },
            {0x06060000, " Access failed due to a hardware error. EEPROM Error (R/W/checksum)" },
            {0x06070010, " Data type does not match. Parameter has a different data type" },
            {0x06070012, " Data type does not match or length of Service telegram too big. Parameter has a different data type or telegram length not correct." },
            {0x06070013, " Data type does not match or length of Service telegram too small. Parameter has a different data type or telegram length not correct." },
            {0x06090011, " Subindex does not exist. Data set does not exist" },
            {0x06090030, " Value range of parameter exceeded. Parameter value too large or too small" },
            {0x06090031, " Value of parameter written too high. Parameter value too large" },
            {0x06090032, " Value of parameter written too low. Parameter value too small" },
            {0x08000020, " Data cannot be transmitted or saved. Invalid value for operation" },
            {0x08000021, " Data cannot be transferred because of local control. Parameter cannot be written in operation" },
            {0x08000022, " No data transfer because of present device state. NMT state machine is not in correct state" },
        };

        private static readonly Dictionary<ulong, string> errorStrings = new Dictionary<ulong, string>()
        {
            {0, " No error " },
            {0x2310, " Frequency inverter was overloaded " },
            {0x4210, " Case temperature outside the temperature limits " },
            {0x4110, " Inside temperature outside of temperature limits " },
            {0x4310, " Motor temperature too high or sensor defective " },
            {0x2340, " Motor phase current above current limit " },
            {0x3210, " DC link voltage outside the voltage range " },
            {0x5111, " Electronic voltage outside the voltage range " },
            {0x2330, " Earth fault on frequency inverter output " },
            {0x1000, " Generic error " },
        };

        #endregion

        #region Constructors

        public CanMessage()
        {
        }

        public CanMessage(InverterMessage message)
        {
            switch (message?.ParameterId)
            {
                case InverterParameterId.CurrentError:
                    // TEST with Nord
                    this.Index = (ushort)(0x2000 + 700);
                    this.Subindex = 1;
                    // no test
                    //this.Index = 0x603F;
                    //this.Subindex = 0;
                    break;

                case InverterParameterId.HomingCalibration:
                    this.Index = 0x6098;
                    this.Subindex = 0;
                    break;

                case InverterParameterId.HomingFastSpeed:
                    this.Index = 0x6099;
                    this.Subindex = 1;
                    break;

                case InverterParameterId.HomingCreepSpeed:
                    this.Index = 0x6099;
                    this.Subindex = 2;
                    break;

                case InverterParameterId.HomingAcceleration:
                    this.Index = 0x6099;
                    this.Subindex = 0;
                    break;

                case InverterParameterId.HomingOffset:
                case InverterParameterId.HomingSensor:
                case InverterParameterId.ShutterTargetPosition:
                case InverterParameterId.ShutterTargetVelocity:
                case InverterParameterId.ShutterLowVelocity:
                case InverterParameterId.ShutterHighVelocityDuration:
                case InverterParameterId.TableTravelTableIndex:
                case InverterParameterId.TableTravelTargetPosition:
                case InverterParameterId.TableTravelTargetSpeeds:
                case InverterParameterId.TableTravelTargetAccelerations:
                case InverterParameterId.TableTravelTargetDecelerations:
                case InverterParameterId.TableTravelDirection:
                case InverterParameterId.BlockDefinition:
                case InverterParameterId.BlockWrite:
                case InverterParameterId.AxisChanged:
                case InverterParameterId.ActiveDataset:
                case InverterParameterId.RunMode:
                case InverterParameterId.Program:
                    this.Index = (ushort)(0x2000 + message.ParameterId);
                    this.Subindex = message.DataSetIndex;
                    break;

                case InverterParameterId.PositionAcceleration:
                    this.Index = 0x6083;
                    this.Subindex = 0;
                    break;

                case InverterParameterId.PositionDeceleration:
                    this.Index = 0x6084;
                    this.Subindex = 0;
                    break;

                case InverterParameterId.PositionTargetSpeed:
                    this.Index = 0x6081;
                    this.Subindex = 0;
                    break;

                case InverterParameterId.TorqueCurrent:
                    this.Index = 0x6078;
                    this.Subindex = 0;
                    break;

                case InverterParameterId.ProfileInput:
                    this.Index = 0x3007;
                    this.Subindex = 0;
                    break;
            }
            if (this.Index != 0)
            {
                this.IsWriteMessage = message.IsWriteMessage;
                if (message.RawData != null)
                {
                    switch (message?.ParameterId)
                    {
                        case InverterParameterId.HomingCalibration:
                            var raw = BitConverter.ToUInt16(message.RawData, 0);
                            this.Data = BitConverter.GetBytes((byte)raw);
                            break;

                        default:
                            this.Data = message.RawData;
                            break;
                    }
                }
            }
        }

        #endregion

        #region Properties

        public byte[] Data { get; }

        public ushort Index { get; }

        public bool IsWriteMessage { get; }

        public byte NodeId { get; set; }

        public byte Subindex { get; }

        #endregion

        #region Methods

        public static string AbortString(ulong abortCode)
        {
            if (abortStrings.TryGetValue(abortCode, out var errorString))
            {
                return $"{abortCode:X08}{errorString}";
            }
            return $"{abortCode:X08}";
        }

        public static string ErrorString(ulong errorCode)
        {
            if (errorStrings.TryGetValue(errorCode, out var errorString))
            {
                return $"{errorCode:X04}{errorString}";
            }
            return $"{errorCode:X04}";
        }

        #endregion
    }
}
