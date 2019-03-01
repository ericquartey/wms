using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Modbus.Device;

namespace Ferretto.VW.RemoteIODriver
{
    public enum SensorsNames
    {
        LU_PRESENT_IN_BAY,

        SECURITY_FUNCTION_ACTIVE
    }

    /// <summary>
    /// This class manages the remote IO device, via the NModBus C# wrapper interface.
    /// Every TIME_OUT elapsed, it makes a read opration and a write operation in the IO digital lines of physical device.
    /// It uses a process image paradigm to handle the remote device.
    /// Used with MOXA ioLogick E1212 device.
    /// </summary>
    public class RemoteIO : IRemoteIO
    {
        #region Fields

        private const int DELAY_TO_CLOSE = 250; // ms

        private const ushort INPUT_ORIGIN = 0;

        private const ushort INPUT_QUANTITY = 8;

        private const ushort OUTPUT_ORIGIN = 3;

        private const ushort OUTPUT_QUANTITY = 5;

        private const int PULSE_TIME = 350; // ms

        private const int TIME_OUT = 50; // ms

        private static readonly object locker = new object();

        private bool bConnected;

        private TcpClient client;

        private AutoResetEvent evTerminate;

        private List<bool> inputs;

        private ModbusIpMaster master;

        private List<bool> outputs;

        private RegisteredWaitHandle regMainThreadWaitHandle;

        #endregion

        #region Constructors

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public RemoteIO()
        {
            this.inputs = new List<bool>();
            this.outputs = new List<bool>();
            for (var i = 0; i < OUTPUT_QUANTITY; i++)
            {
                this.outputs.Add(false);
            }
            this.bConnected = false;
        }

        #endregion

        #region Destructors

        /// <summary>
        /// D-tor.
        /// </summary>
        ~RemoteIO()
        {
            if (this.bConnected)
            {
                this.Disconnect();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the digital inputs.
        /// </summary>
        public List<bool> Inputs
        {
            get
            {
                lock (locker)
                {
                    return this.inputs;
                }
            }
        }

        /// <summary>
        /// Get/set the IP address. Ipv4 format.
        /// </summary>
        public string IPAddress { get; set; } = "169.254.231.10";

        /// <summary>
        /// Set the digital outputs.
        /// </summary>
        public List<bool> Outputs
        {
            set
            {
                lock (locker)
                {
                    this.outputs = value;
                }
            }
        }

        /// <summary>
        /// Get/set the port address. Fixed value for the port address (see the NModBus class documentation).
        /// </summary>
        public int Port { get; set; } = 502;

        #endregion

        #region Methods

        /// <summary>
        /// Connect to remote device.
        /// </summary>
        public void Connect()
        {
            try
            {
                this.client = new TcpClient(this.IPAddress, this.Port);
                this.master = ModbusIpMaster.CreateIp(this.client);
            }
            catch (Exception)
            {
                this.client = null;
            }

            if (this.client != null)
            {
                if (this.client.Connected)
                {
                    // Power up the machine
                    this.powerUp();  // uncomment these code lines

                    // Set the digital signal to high level related to the encoder of vertical motor enabling (as default).
                    this.outputs.Clear();
                    this.outputs = new List<bool>();
                    this.outputs.Add(false);
                    this.outputs.Add(true);
                    this.outputs.Add(false);
                    this.outputs.Add(false);
                    this.outputs.Add(false);
                    this.writeData();

                    // Create and start internal thread to read/write digital lines
                    this.createThread();
                    this.bConnected = true;
                }
            }
        }

        /// <summary>
        /// Disconnect from remote device.
        /// </summary>
        public void Disconnect()
        {
            this.evTerminate?.Set();
            this.destroyThread();

            // Just wait before to close the TCPClient object
            Thread.Sleep(DELAY_TO_CLOSE);

            this.client?.Close();
            this.client?.Dispose();
            this.bConnected = false;
        }

        /// <summary>
        /// Create and start the internal thread.
        /// </summary>
        private void createThread()
        {
            this.evTerminate = new AutoResetEvent(false);
            this.regMainThreadWaitHandle = ThreadPool.RegisterWaitForSingleObject(this.evTerminate, this.onMainThread, null, TIME_OUT, false);
        }

        /// <summary>
        /// Destroy the internal thread.
        /// </summary>
        private void destroyThread()
        {
            this.regMainThreadWaitHandle?.Unregister(this.evTerminate);
        }

        /// <summary>
        /// Main thread.
        /// Every TIME_OUT time elapsed, the read operation and write operation are performed in the remote device.
        /// It is used according to the process image paradigm.
        /// </summary>
        private void onMainThread(object data, bool bTimeOut)
        {
            if (bTimeOut)
            {
                lock (locker)
                {
                    // read data from remote device
                    this.readData();
                    // write data to remote device
                    this.writeData();
                }
            }
        }

        /// <summary>
        /// Power up the engines.
        /// Manage the output line DO3 with a pulse signal (time >= 350 ms) in order to power up the machine.
        /// </summary>
        private void powerUp()
        {
            var digitalOuts = new bool[OUTPUT_QUANTITY];
            for (var i = 0; i < OUTPUT_QUANTITY; i++)
            {
                digitalOuts[i] = false;
            }
            this.master.WriteMultipleCoils(OUTPUT_ORIGIN, digitalOuts);

            digitalOuts[0] = true; // Low 2 High signal
            this.master.WriteMultipleCoils(OUTPUT_ORIGIN, digitalOuts);

            Thread.Sleep(PULSE_TIME);

            digitalOuts[0] = false; // High to Low signal
            this.master.WriteMultipleCoils(OUTPUT_ORIGIN, digitalOuts);
        }

        /// <summary>
        /// Read the inputs from remote device.
        /// </summary>
        private void readData()
        {
            if (this.client.Connected)
            {
                this.inputs.Clear();
                var tmp = this.master.ReadInputs(INPUT_ORIGIN, INPUT_QUANTITY);

                for (var i = 0; i < tmp.Length; i++)
                {
                    this.inputs.Add(tmp[i]);
                }
            }
        }

        /// <summary>
        /// Write the outputs to remote device.
        /// </summary>
        private void writeData()
        {
            if (this.client.Connected)
            {
                var digitalOuts = new bool[OUTPUT_QUANTITY];
                this.outputs.CopyTo(digitalOuts);

                if (this.outputs.Count != 0)
                {
                    this.master.WriteMultipleCoils(OUTPUT_ORIGIN, digitalOuts);
                }
            }
        }

        #endregion
    }
}
