using System.Threading;

namespace Ferretto.VW.ActionBlocks
{
    public class EmbarkDrawer
    {
        // NOTE: To be modified!!

        #region Fields

        private const int TIME_OUT = 100;                   // Time out

        private InverterDriver.InverterDriver InverterDriver;
        private AutoResetEvent evAckTerminate;
        private AutoResetEvent evTerminate;
        private Thread threadMain;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public EmbarkDrawer()
        {
            // Classe InverterDriver
            InverterDriver = null;

            // AutoResetEvent
            evTerminate = null;
            evAckTerminate = null;

            // Thread
            threadMain = null;
        }

        #endregion Constructors

        #region Methods

        public bool Initialize(InverterDriver.InverterDriver drv)
        {
            // Assign the driver
            InverterDriver = drv;

            // Create the thread
            CreateThread();

            return true;
        }

        //! Start the execution of action
        public void Start()
        {
            // Start the thread
            if (threadMain != null)
            {
                threadMain.Start();
            }

            // Call the SelectMovement function of the inverter driver
            // InverterDriver.SetTypeOfMotorMovement(0xF1);
        }

        public void Terminate()
        {
            // destroy resources fot thread
            DestroyThread();

            InverterDriver = null;
        }

        //! Create thread
        private void CreateThread()
        {
            evTerminate = new AutoResetEvent(false);
            evAckTerminate = new AutoResetEvent(false);

            // create the thread
            threadMain = new Thread(this.EmbarkDrawerThread);
        }

        //! Destroy thread
        private void DestroyThread()
        {
            // TODO
        }

        //! Main thread
        private void EmbarkDrawerThread()
        {
            const int EV_TERMINATE = 0;
            var handles = new WaitHandle[1];
            handles[0] = this.evTerminate;

            var bExit = false;
            while (!bExit)
            {
                var code = WaitHandle.WaitAny(handles, TIME_OUT);
                switch (code)
                {
                    case EV_TERMINATE:
                        {
                            // exit from the cycle
                            bExit = true;
                            break;
                        }

                    case WaitHandle.WaitTimeout:
                        {
                            // call the read method from inverter driver class (i.e. name: bool IsOperationDone()
                            // Da decommentare appena si può
                            // if (this.driver.IsOperationDone() == true)
                            // {
                            //    this.driver.MoveAlongHorizontalAxisWithProfile();
                            // }
                            break;
                        }
                }
            }
        }

        #endregion Methods
    }
}
