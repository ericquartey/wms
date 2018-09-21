using System.Threading;

using Ferretto.VW.InverterDriver;


namespace Ferretto.VW.ActionBlocks
{
    public class CActions
    { }

    /// <summary>
    /// Actions codes
    /// </summary>
    public enum ActionCodes
    {
        /// <summary>
        /// Set zero of vertical axes.
        /// </summary>
        SetZeroOfVerticalAxes = 0x00,
        /// <summary>
        /// Set resolution of vertical axes. (?)
        /// </summary>
        SetResolutionOfVerticalAxes = 0x01,
        /// <summary>
        /// Embark the drawer.
        /// </summary>
        EmbarkDrawer = 0x02,
        /// <summary>
        /// Disembark the drawer.
        /// </summary>
        DisembarkDrawer = 0x03

        // Add here...
    }


    /// <summary>
    /// This class handles the action to perform the zero of vertical axes for the inverter.
    /// It contains a thread to manage the sequential operations.
    /// It is used by the application UI (send command/receive notification).
    /// </summary>
    public class CSetZeroOfVerticalAxes
    {
        private readonly Thread m_thread;

        public CSetZeroOfVerticalAxes()
        {
            this.m_thread = new Thread(this.startMainThread);
        }

        public bool Start()
        {
            // Add your implementation code here
            return true;
        }

        public void Stop()
        {
            // Add your immplementation code here
        }

        /// <summary>
        /// Main thread.
        /// </summary>
        private void startMainThread()
        {
            // Add your implementation code here
        }

    }


    /// <summary>
    /// This class handles the action to embark the drawer in the warehouse.
    /// It contains a thread to manage the sequential operations. 
    /// </summary>
    public class CEmbarkDrawer
    {
        // TODO...
    }




}
