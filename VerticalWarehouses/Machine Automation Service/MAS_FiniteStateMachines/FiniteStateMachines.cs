using System;
using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : IFiniteStateMachines
    {
        #region Fields

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        //private DataLayerContext dtContext;

        private StateMachineHoming homing;

        private StateMachineVerticalHoming verticalHoming;

        #endregion Fields

        //private WriteLogService writeLog;

        #region Constructors

        public FiniteStateMachines(MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.driver = iDriver;  //Singleton<MAS_InverterDriver.InverterDriver>.UniqueInstance;
            this.data = iWriteLogService;

            this.homing = new StateMachineHoming(this);
            this.verticalHoming = new StateMachineVerticalHoming(this, this.driver, this.data);

            //this.dtContext = new DataLayerContext();
            //this.writeLog = new WriteLogService(this.dtContext);
        }

        #endregion Constructors

        #region Methods

        public void Destroy()
        {
            this.driver.Destroy();
        }

        public void DoHoming(BroadcastDelegate broadcastDelegate)
        {
            if (this.homing == null)
            {
                throw new InvalidOperationException();
            }

            this.homing.Start();
            this.homing.DoAction(IdOperation.SwitchVerticalToHorizontal);
            this.homing.DoAction(IdOperation.HorizontalHome);
            this.homing.DoAction(IdOperation.SwitchHorizontalToVertical);
            this.homing.DoAction(IdOperation.VerticalHome);
            this.homing.DoAction(IdOperation.SwitchVerticalToHorizontal);
            this.homing.DoAction(IdOperation.HorizontalHome);
            this.homing.DoAction(IdOperation.SwitchHorizontalToVertical);
        }

        public void DoVerticalHoming(BroadcastDelegate broadcastDelegate)
        {
            if (this.verticalHoming == null)
            {
                throw new InvalidOperationException();
            }

            this.data.LogWriting("Start");

            this.verticalHoming.Start();
            this.verticalHoming.DoAction(IdOperation.VerticalHome);

            this.data.LogWriting("End homing");
        }

        public void MakeOperationByInverter(IdOperation code)
        {
            switch (code)
            {
                case IdOperation.HorizontalHome:
                    {
                        // TODO await driver.ExecuteAction("Horizontal Home");
                        break;
                    }
                case IdOperation.SwitchHorizontalToVertical:
                    {
                        // TODO await driver.ExecuteAction("SwitchHorizontalToVertical");
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        this.driver.ExecuteVerticalHoming();
                        break;
                    }
                case IdOperation.SwitchVerticalToHorizontal:
                    {
                        // TODO await driver.ExecuteAction("SwitchVerticalToHorizontal");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion Methods
    }
}
