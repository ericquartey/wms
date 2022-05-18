using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class CanMessage
    {
        #region Constructors

        public CanMessage()
        {
        }

        public CanMessage(InverterMessage message)
        {
            switch (message?.ParameterId)
            {
                case InverterParameterId.CurrentError:
                    this.Index = 0x603F;
                    this.Subindex = 0;
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
    }
}
