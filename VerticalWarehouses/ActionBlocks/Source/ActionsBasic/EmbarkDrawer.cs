using System.Threading;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.ActionBlocks.ActionsBasic
{
    public class EmbarkDrawer
    {
        // NOTE: To be modified!!

        #region Fields

        private const int TIME_OUT = 100;                   // Time out

        private CInverterDriver driver;
        private AutoResetEvent hevAckTerminate;
        private AutoResetEvent hevTerminate;
        private Thread threadMain;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public EmbarkDrawer()
        {
            // Classe InverterDriver
            this.driver = null;

            // AutoResetEvent
            this.hevTerminate = null;
            this.hevAckTerminate = null;

            // Thread
            this.threadMain = null;
        }

        #endregion Constructors

        #region Methods

        public bool Initialize(CInverterDriver drv)
        {
            // Assign the driver
            this.driver = drv;

            // Create the thread
            this.create_thread();

            return true;
        }

        //! Start the execution of action
        public void Start()
        {
            // Start the thread
            if (this.threadMain != null)
            {
                this.threadMain.Start();
            }

            // Call the SelectMovement function of the inverter driver
            this.driver.SetTypeOfMotorMovement(0xF1);
        }

        public void Terminate()
        {
            // destroy resources fot thread
            this.destroy_thread();

            this.driver = null;
        }

        //! Create thread
        private void create_thread()
        {
            this.hevTerminate = new AutoResetEvent(false);
            this.hevAckTerminate = new AutoResetEvent(false);

            // create the thread
            this.threadMain = new Thread(this.embarkDrawerThread);
        }

        //! Destroy thread
        private void destroy_thread()
        {
            // TODO
        }

        //! Main thread
        private void embarkDrawerThread()
        {
            const int EV_TERMINATE = 0;
            var handles = new WaitHandle[1];
            handles[0] = this.hevTerminate;

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
