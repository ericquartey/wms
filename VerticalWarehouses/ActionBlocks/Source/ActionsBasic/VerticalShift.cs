using System;

namespace Ferretto.VW.ActionBlocks.Source.ActionsBasic
{
    public class VerticalShift
    {
        #region Fields
        // Parameters for step: "1-Set home parameters"
        private int TargetPos;      // Param 1455

        private float TargetSpeed;  // Param 1456

        private float Acceleration; // Param 1457

        private float Deceleration; // Param 1458

        // Parameters for step: "2-Set operating mode"
        private int OpModeOverride; // Param 1454

        private int ActualOpMode;
        
        // Parameters for step: "3-Engine commands"
        private byte[] ControlWord; // Param 410 - vedere se usare una ushort che ha 16 bit e una stringa di 0/1

        private byte[] StatusWord; // Param 411 - vedere se usare una ushort che ha 16 bit e una stringa di 0/1
        #endregion Fields
        
        #region Constructors

        public VerticalShift()
        {
            // Parameters for step: "1-Set home parameters"
            this.TargetPos = 0;
            this.TargetSpeed = 0;  // Verif
            this.Acceleration = 0; // Verif
            this.Deceleration = 0; // Verif
            // Parameters for step: "2-Set operating mode"
            this.OpModeOverride = 1;
            this.ActualOpMode = 1;
            // Parameters for step: "3-Engine commands"
            this.ControlWord = new byte[2];
            this.StatusWord = new byte[2];
        }

        #endregion Constructors

        #region Delegates

        // Delegate for operation end to send UI
        public delegate void EndEventHandler(bool result);

        // Delegate for operation end to send UI
        public delegate void ErrorEventHandler(string ErrorMessage);

        #endregion Delegates

        #region Events

        // Event end operation to send UI
        public event EndEventHandler ThrowEndEvent;

        // Event end operation to send UI
        public event ErrorEventHandler ThrowErrorEvent;

        #endregion Events

        // Subscription to the Inverter Driver End Execution event
        //this.InverterDriver.ThrowEvent += new Program.MessageEventHandler(this.EndExecution);

        // Subscription to the Inverter Driver Error event
        //this.InverterDriver.ThrowEvent += new Program.MessageEventHandler(this.ErrorEvent);

        #region Methods

        // CallBack function for the EndExecution delegate
        public void EndExecution(byte[] StatusWord)
        {
            var result = true;

            try
            {
            }
            catch (Exception ex)
            {
                result = false;
            }

            ThrowEndEvent.Invoke(result);
        }

        // CallBack function for the EndExecution delegate
        public void ErrorEvent(int ErrorCode)
        {
            var ErrorMessage = "";

            try
            {
            }
            catch (Exception ex)
            {
            }

            ThrowErrorEvent.Invoke(ErrorMessage);
        }

        // Insert here the InverterDriver declaration
        public bool StartExecution()
        {
            var result = true;

            try
            {
                // Inverter Driver class instantiate

                // Call the Inverter Driver Initialize method
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        #endregion Methods
    }
}
