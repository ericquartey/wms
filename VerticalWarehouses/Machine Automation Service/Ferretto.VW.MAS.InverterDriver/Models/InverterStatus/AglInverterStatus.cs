using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public class AglInverterStatus : InverterStatusBase, IAglInverterStatus
    {
        #region Fields

        private const int TOTAL_SENSOR_INPUTS = 8;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private ShutterPosition currentShutterPosition;

        #endregion

        #region Constructors

        public AglInverterStatus(InverterIndex systemIndex, IServiceScopeFactory serviceScopeFactory)
            : base(systemIndex)
        {
            this.Inputs = new bool[TOTAL_SENSOR_INPUTS];
            this.currentShutterPosition = ShutterPosition.Opened; // Set the Opened position (workaround)
            this.OperatingMode = (ushort)InverterOperationMode.ProfileVelocity;
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory)); ;
        }

        #endregion

        //INFO AGL Inputs

        #region Properties

        public bool AGL_FreeSensor1 => this.Inputs?[(int)InverterSensors.AGL_FreeSensor1] ?? false;

        public bool AGL_FreeSensor2 => this.Inputs?[(int)InverterSensors.AGL_FreeSensor2] ?? false;

        public bool AGL_FreeSensor3 => this.Inputs?[(int)InverterSensors.AGL_FreeSensor3] ?? false;

        public bool AGL_FreeSensor4 => this.Inputs?[(int)InverterSensors.AGL_FreeSensor4] ?? false;

        public bool AGL_HardwareSensorSS1 => this.Inputs?[(int)InverterSensors.AGL_HardwareSensorSS1] ?? false;

        public bool AGL_HardwareSensorSTO => this.Inputs?[(int)InverterSensors.AGL_HardwareSensorSTO] ?? false;

        public bool AGL_ShutterSensorA => this.Inputs?[(int)InverterSensors.AGL_ShutterSensorA] ?? false;

        public bool AGL_ShutterSensorB => this.Inputs?[(int)InverterSensors.AGL_ShutterSensorB] ?? false;

        //public bool AGL_HardwareSensorSTOB => this.Inputs?[(int)InverterSensors.AGL_HardwareSensorSTOB] ?? false;

        public ShutterPosition CurrentShutterPosition
        {
            get => this.GetCurrentPosition();
            private set => this.currentShutterPosition = value;
        }

        public IProfileVelocityControlWord ProfileVelocityControlWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.ProfileVelocity)
                {
                    throw new InvalidOperationException("Inverter is not configured for ProfileVelocity Mode");
                }

                if (this.controlWord is IProfileVelocityControlWord word)
                {
                    return word;
                }

                throw new InvalidOperationException($"Current Control Word Type {this.controlWord.GetType().Name} is not compatible with ProfileVelocity Mode");
            }
        }

        public IProfileVelocityStatusWord ProfileVelocityStatusWord
        {
            get
            {
                if (this.OperatingMode != (ushort)InverterOperationMode.ProfileVelocity)
                {
                    throw new InvalidOperationException("Inverter is not configured for ProfileVelocity Mode");
                }

                if (this.statusWord is IProfileVelocityStatusWord word)
                {
                    return word;
                }

                throw new InvalidOperationException($"Current Status Word Type {this.statusWord.GetType().Name} is not compatible with ProfileVelocity Mode");
            }
        }

        #endregion

        #region Methods

        public override bool UpdateInputsStates(bool[] newInputStates)
        {
            if (newInputStates is null)
            {
                return false;
            }

            var updateRequired = false;
            for (var index = 0; index < newInputStates.Length; index++)
            {
                if (index > TOTAL_SENSOR_INPUTS)
                {
                    continue;
                }

                if (this.Inputs[index] != newInputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }

            try
            {
                if (updateRequired)
                {
                    Array.Copy(newInputStates, 0, this.Inputs, 0, newInputStates.Length);
                    this.GetCurrentPosition();
                }
            }
            catch (Exception ex)
            {
                throw new InverterDriverException("Error while updating AGL inverter inputs.", ex);
            }

            return updateRequired;
        }

        private ShutterPosition GetCurrentPosition()
        {
            //var configurationEnvName = "";
            //using (var scope = this.serviceScopeFactory.CreateScope())
            //{
            //    var envProvider = scope.ServiceProvider.GetRequiredService<IHostingEnvironment>();
            //    configurationEnvName = envProvider.EnvironmentName;
            //}

            var value = ShutterPosition.NotSpecified;

            //if (configurationEnvName == "Prototype1")
            //{
            //    // Used only for Prototype1 (old true table for shutters)
            //    if (this.Inputs[(int)InverterSensors.AGL_ShutterSensorA])
            //    {
            //        if (this.Inputs[(int)InverterSensors.AGL_ShutterSensorB])
            //        {
            //            value = ShutterPosition.Opened;
            //        }
            //        else
            //        {
            //            value = ShutterPosition.Half;
            //        }
            //    }
            //    else
            //    {
            //        if (this.Inputs[(int)InverterSensors.AGL_ShutterSensorB])
            //        {
            //            value = ShutterPosition.Intermediate;
            //        }
            //        else
            //        {
            //            value = ShutterPosition.Closed;
            //        }
            //    }
            //}
            //else
            {
                if (!this.Inputs[(int)InverterSensors.AGL_ShutterSensorA])
                {
                    if (this.Inputs[(int)InverterSensors.AGL_ShutterSensorB])
                    {
                        value = ShutterPosition.Opened;
                    }
                    else
                    {
                        value = ShutterPosition.Closed;
                    }
                }
                else
                {
                    if (this.Inputs[(int)InverterSensors.AGL_ShutterSensorB])
                    {
                        value = ShutterPosition.Intermediate;
                    }
                    else
                    {
                        value = ShutterPosition.Half;
                    }
                }
            }

            this.currentShutterPosition = value;
            return value;
        }

        #endregion
    }
}
