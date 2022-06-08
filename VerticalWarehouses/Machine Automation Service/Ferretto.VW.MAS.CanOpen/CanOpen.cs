using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ixxat.Vci4;
using Ixxat.Vci4.Bal;
using Ixxat.Vci4.Bal.Can;

namespace Ferretto.VW.MAS.CanOpenClient
{
    public delegate void ImplicitMessageEventHandler(CanOpen sender);

    public class CanOpen
    {
        #region Fields

        private const int COB_ID_NMT = 0x700;

        private const int COB_ID_RXPDO = 0x200;

        private const int COB_ID_RXSDO = 0x580;

        private const int COB_ID_SYNC = 0x80;

        private const int COB_ID_TXPDO = 0x180;

        private const int COB_MAX_NODE = 0x80;

        /// <summary>
        ///   Reference to the CAN message communication channel.
        /// </summary>
        private static ICanChannel mCanChn;

        /// <summary>
        ///   Reference to the CAN controller.
        /// </summary>
        private static ICanControl mCanCtl;

        /// <summary>
        ///   Reference to the CAN message scheduler.
        /// </summary>
        private static ICanScheduler mCanSched;

        /// <summary>
        ///   Reference to the used VCI device.
        /// </summary>
        private static IVciDevice mDevice;

        /// <summary>
        ///   Quit flag for the receive thread.
        /// </summary>
        private static long mMustQuit = 0;

        /// <summary>
        ///   Reference to the message reader of the CAN message channel.
        /// </summary>
        private static ICanMessageReader mReader;

        /// <summary>
        ///   Event that's set if at least one message was received.
        /// </summary>
        private static AutoResetEvent mRxEvent;

        /// <summary>
        ///   Reference to the message writer of the CAN message channel.
        /// </summary>
        private static ICanMessageWriter mWriter;

        private static Thread pdoThread;

        /// <summary>
        ///   Thread that handles the message reception.
        /// </summary>
        private static Thread rxThread;

        private ConcurrentQueue<MessageEmergency> messageEmergency;

        private ConcurrentQueue<MessageNMT> messageNMT;

        /// <summary>
        /// list of all pdo message variations detected, consumed by GetPDO
        /// </summary>
        private ConcurrentQueue<MessagePDO> messagePDO;

        /// <summary>
        /// list of all sdo messages to be transmitted. Only one by node is allowed
        /// produced and consumed at runtime
        /// </summary>
        private Dictionary<byte, MessageSDO> messageSDO;

        /// <summary>
        /// list of pdo messages to be received. Only one by node is allowed
        /// must stay alive for all communication
        /// </summary>
        private Dictionary<byte, ICanCyclicTXMsg> readPDO;

        private int readTimeout;

        private AutoResetEvent sdoEvent;

        /// list of pdo messages to be trasmitted. Only one by node is allowed
        /// must stay alive for all communication
        private Dictionary<byte, ICanCyclicTXMsg> writePDO;

        #endregion

        #region Constructors

        public CanOpen()
        {
        }

        #endregion

        #region Events

        public event ImplicitMessageEventHandler ImplicitMessageEvent;

        #endregion

        //public ushort EmergencyError { get; set; }

        //public ushort EmergencyManufacturerError { get; set; }

        //public byte EmergencyNode { get; set; }

        //public byte EmergencyRegister { get; set; }

        //public bool IsSync { get; set; }

        //public byte NMTNode { get; private set; }

        //public byte NMTState { get; private set; }

        #region Methods

        //************************************************************************
        /// <summary>
        ///   Finalizes the application
        /// </summary>
        //************************************************************************
        public void FinalizeApp()
        {
            //
            // Dispose all hold VCI objects.
            //

            // Dispose message reader
            DisposeVciObject(mReader);

            // Dispose message writer
            DisposeVciObject(mWriter);

            // Dispose CAN channel
            DisposeVciObject(mCanChn);

            // Dispose CAN controller
            DisposeVciObject(mCanCtl);

            // Dispose VCI device
            DisposeVciObject(mDevice);

            DisposeVciObject(this.sdoEvent);
        }

        public bool GetEmergency(out byte node,
            out ushort error,
            out byte register,
            out ushort manufacturerError)
        {
            node = 0;
            error = 0;
            register = 0;
            manufacturerError = 0;

            if (this.messageEmergency.IsEmpty)
            {
                return false;
            }
            if (this.messageEmergency.TryDequeue(out var message))
            {
                node = message.Node;
                error = message.Error;
                register = message.Register;
                manufacturerError = message.ManufacturerError;
                return true;
            }
            return false;
        }

        public bool GetNMT(out byte node,
            out byte state,
            out bool isSync)
        {
            node = 0;
            state = 0;
            isSync = false;

            if (this.messageNMT.IsEmpty)
            {
                return false;
            }
            if (this.messageNMT.TryDequeue(out var message))
            {
                node = message.Node;
                state = message.State;
                isSync = message.IsSync;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads all the changes to the reading PDO object. call multiple times until it gets empty
        /// </summary>
        /// <param name="node"></param>
        /// <param name="data"></param>
        /// <returns>true if data is valid</returns>
        public bool GetPDO(out byte node, out byte[] data)
        {
            node = 0;
            data = null;
            if (this.messagePDO.IsEmpty)
            {
                return false;
            }
            if (this.messagePDO.TryDequeue(out var message))
            {
                node = message.Node;
                data = message.Data;
                return true;
            }
            return false;
        }

        public void Guarding(byte node)
        {
            TransmitData((uint)COB_ID_NMT + node, null, 0);
        }

        //************************************************************************
        /// <summary>
        ///   Opens the specified socket, creates a message channel, initializes
        ///   and starts the CAN controller.
        /// </summary>
        /// <param name="canNo">
        ///   Number of the CAN controller to open.
        /// </param>
        /// <returns>
        ///   A value indicating if the socket initialization succeeded or failed.
        /// </returns>
        //************************************************************************
        public bool InitSocket(Byte canNo, IEnumerable<int> nodeList)
        {
            IBalObject bal = null;
            bool succeeded = false;

            try
            {
                //
                // Open bus access layer
                //
                bal = mDevice.OpenBusAccessLayer();

                //
                // Open a message channel for the CAN controller
                //
                mCanChn = bal.OpenSocket(canNo, typeof(ICanChannel)) as ICanChannel;

                //
                // Open the scheduler of the CAN controller
                //
                mCanSched = bal.OpenSocket(canNo, typeof(ICanScheduler)) as ICanScheduler;

                // Initialize the message channel
                mCanChn.Initialize(1024, 128, false);

                // Get a message reader object
                mReader = mCanChn.GetMessageReader();

                // Initialize message reader
                mReader.Threshold = 1;

                // Create and assign the event that's set if at least one message
                // was received.
                mRxEvent = new AutoResetEvent(false);
                mReader.AssignEvent(mRxEvent);

                // Get a message writer object
                mWriter = mCanChn.GetMessageWriter();

                // Initialize message writer
                mWriter.Threshold = 1;

                // Activate the message channel
                mCanChn.Activate();

                //
                // Open the CAN controller
                //
                mCanCtl = bal.OpenSocket(canNo, typeof(ICanControl)) as ICanControl;

                // Initialize the CAN controller
                mCanCtl.InitLine(CanOperatingModes.Standard |
                  CanOperatingModes.Extended |
                  CanOperatingModes.ErrFrame,
                  CanBitrate.Cia250KBit);

                //
                // print line status
                //
                Console.WriteLine(" LineStatus: {0}", mCanCtl.LineStatus);

                // Set the acceptance filter for std identifiers
                mCanCtl.SetAccFilter(CanFilter.Std,
                                     (uint)CanAccCode.All, (uint)CanAccMask.All);

                // Set the acceptance filter for ext identifiers
                mCanCtl.SetAccFilter(CanFilter.Ext,
                                     (uint)CanAccCode.All, (uint)CanAccMask.All);

                // Start the CAN controller
                mCanCtl.StartLine();

                //
                // start the receive thread
                //
                rxThread = new Thread(new ThreadStart(this.ReceiveThreadFunc));
                rxThread.Start();

                this.messageSDO = new Dictionary<byte, MessageSDO>();
                this.sdoEvent = new AutoResetEvent(false);

                this.writePDO = new Dictionary<byte, ICanCyclicTXMsg>();
                this.readPDO = new Dictionary<byte, ICanCyclicTXMsg>();
                foreach (var node in nodeList)
                {
                    // TXPDO (from inverter to PC)
                    this.readPDO.Add((byte)node, CreateCyclicMsg(COB_ID_TXPDO + (uint)node, 8));

                    // RXPDO (from pc to inverter)
                    this.writePDO.Add((byte)node, CreateCyclicMsg(COB_ID_RXPDO + (uint)node, 8));
                }

                this.messageEmergency = new ConcurrentQueue<MessageEmergency>();
                this.messageNMT = new ConcurrentQueue<MessageNMT>();
                if (this.readPDO.Count > 0)
                {
                    this.messagePDO = new ConcurrentQueue<MessagePDO>();
                    pdoThread = new Thread(new ThreadStart(this.PdoThreadFunc));
                    pdoThread.Start();
                }

                succeeded = true;
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error: Initializing socket failed : " + exc.Message);
                succeeded = false;
            }
            finally
            {
                //
                // Dispose bus access layer
                //
                DisposeVciObject(bal);
            }

            return succeeded;
        }

        /// <summary>
        /// ReadSDO
        /// </summary>
        /// <param name="node"></param>
        /// <param name="index"></param>
        /// <param name="subindex"></param>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        /// <returns>true if message is written without errors</returns>
        public bool ReadSDO(byte node, ushort index, byte subindex, out byte[] data, out int dataLength, out ulong abortCode)
        {
            data = null;
            dataLength = 0;
            abortCode = 0;
            // only one SDO at a time
            if (this.messageSDO != null && this.messageSDO.ContainsKey(node))
            {
                abortCode = 1;
                return false;
            }
            var isOk = false;
            this.sdoEvent.Reset();
            var msg = new MessageSDO(node, index, subindex, data, 0);
            this.messageSDO.Add(node, msg);

            TransmitData(msg.Id, msg.TxData, 8);

            if (this.sdoEvent.WaitOne(this.readTimeout))
            {
                if (this.messageSDO.TryGetValue(node, out var rxMsg))
                {
                    if (rxMsg.DataLength > 0)
                    {
                        dataLength = rxMsg.DataLength;
                        data = new byte[rxMsg.DataLength];
                        Array.Copy(rxMsg.Data, data, dataLength);
                        abortCode = rxMsg.AbortCode;
                        isOk = abortCode == 0;
                    }
                    else
                    {
                        abortCode = 4;
                    }
                }
                else
                {
                    abortCode = 2;
                }
            }
            else
            {
                abortCode = 3;
            }
            this.sdoEvent.Reset();
            this.messageSDO.Remove(node);
            return isOk;
        }

        //************************************************************************
        /// <summary>
        ///   Selects the first CAN adapter.
        /// </summary>
        //************************************************************************
        public void SelectDevice(int readTimout)
        {
            IVciDeviceManager deviceManager = null;
            IVciDeviceList deviceList = null;
            IEnumerator deviceEnum = null;

            try
            {
                //
                // Get device manager from VCI server
                //
                deviceManager = VciServer.Instance().DeviceManager;

                //
                // Get the list of installed VCI devices
                //
                deviceList = deviceManager.GetDeviceList();

                //
                // Get enumerator for the list of devices
                //
                deviceEnum = deviceList.GetEnumerator();

                //
                // Get first device
                //
                deviceEnum.MoveNext();
                mDevice = deviceEnum.Current as IVciDevice;

                //
                // print bus type and controller type of first controller
                //
                IVciCtrlInfo info = mDevice.Equipment[0];
                Console.Write(" BusType    : {0}\n", info.BusType);
                Console.Write(" CtrlType   : {0}\n", info.ControllerType);

                // show the device name and serial number
                object serialNumberGuid = mDevice.UniqueHardwareId;
                string serialNumberText = GetSerialNumberText(ref serialNumberGuid);
                Console.Write(" Interface    : " + mDevice.Description + "\n");
                Console.Write(" Serial number: " + serialNumberText + "\n");

                this.readTimeout = readTimout;
            }
            catch (Exception exc)
            {
                throw new InvalidOperationException(exc.Message);
            }
            finally
            {
                //
                // Dispose device manager ; it's no longer needed.
                //
                DisposeVciObject(deviceManager);

                //
                // Dispose device list ; it's no longer needed.
                //
                DisposeVciObject(deviceList);

                //
                // Dispose device list ; it's no longer needed.
                //
                DisposeVciObject(deviceEnum);
            }
        }

        public void Stop()
        {
            //
            // tell receive thread to quit
            //
            Interlocked.Exchange(ref mMustQuit, 1);

            //
            // Wait for termination of receive thread
            //
            rxThread.Join();

            pdoThread.Join();

            mCanCtl.StopLine();

            this.FinalizeApp();
        }

        /// <summary>
        /// Sync: send SYNC message
        /// </summary>
        public void Sync()
        {
            TransmitData(COB_ID_SYNC, null, 0);
        }

        public bool WritePDO(byte node, byte[] data)
        {
            if (this.writePDO.TryGetValue(node, out var msg))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    msg[i] = data[i];
                }
                if (msg.Status != CanCyclicTXStatus.Busy)
                {
                    msg.Start(0);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// WriteSDO
        /// </summary>
        /// <param name="node"></param>
        /// <param name="index"></param>
        /// <param name="subindex"></param>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        /// <returns>true if message is written without errors</returns>
        public bool WriteSDO(byte node, ushort index, byte subindex, byte[] data, ushort dataLength, out ulong abortCode)
        {
            abortCode = 0;
            // only one SDO at a time
            if (this.messageSDO != null && this.messageSDO.ContainsKey(node))
            {
                abortCode = 1;
                return false;
            }
            var isOk = false;
            this.sdoEvent.Reset();
            var msg = new MessageSDO(node, index, subindex, data, dataLength);
            this.messageSDO.Add(node, msg);

            TransmitData(msg.Id, msg.TxData, 8);

            if (this.sdoEvent.WaitOne(this.readTimeout))
            {
                if (this.messageSDO.TryGetValue(node, out var txMsg))
                {
                    abortCode = txMsg.AbortCode;
                    isOk = (abortCode == 0) && (txMsg.TransmittedBytes == dataLength);
                }
                else
                {
                    abortCode = 2;
                }
            }
            else
            {
                abortCode = 3;
            }
            this.sdoEvent.Reset();
            this.messageSDO.Remove(node);
            return isOk;
        }

        private static ICanCyclicTXMsg CreateCyclicMsg(uint id, byte dataLength)
        {
            ICanCyclicTXMsg cyclicMsg = mCanSched.AddMessage();

            cyclicMsg.Identifier = id;
            cyclicMsg.CycleTicks = 100;
            cyclicMsg.DataLength = dataLength;
            cyclicMsg.SelfReceptionRequest = true;

            return cyclicMsg;
        }

        //************************************************************************
        /// <summary>
        ///   This method tries to dispose the specified object.
        /// </summary>
        /// <param name="obj">
        ///   Reference to the object to be disposed.
        /// </param>
        /// <remarks>
        ///   The VCI interfaces provide access to native driver resources.
        ///   Because the .NET garbage collector is only designed to manage memory,
        ///   but not native OS and driver resources the application itself is
        ///   responsible to release these resources via calling
        ///   IDisposable.Dispose() for the obects obtained from the VCI API
        ///   when these are no longer needed.
        ///   Otherwise native memory and resource leaks may occure.
        /// </remarks>
        //************************************************************************
        private static void DisposeVciObject(object obj)
        {
            if (null != obj)
            {
                IDisposable dispose = obj as IDisposable;
                if (null != dispose)
                {
                    dispose.Dispose();
                    obj = null;
                }
            }
        }

        /// <summary>
        /// Returns the UniqueHardwareID GUID number as string which
        /// shows the serial number.
        /// Note: This function will be obsolete in later version of the VCI.
        /// Until VCI Version 3.1.4.1784 there is a bug in the .NET API which
        /// returns always the GUID of the interface. In later versions there
        /// the serial number itself will be returned by the UniqueHardwareID property.
        /// </summary>
        /// <param name="serialNumberGuid">Data read from the VCI.</param>
        /// <returns>The GUID as string or if possible the  serial number as string.</returns>
        private static string GetSerialNumberText(ref object serialNumberGuid)
        {
            string resultText;

            // check if the object is really a GUID type
            if (serialNumberGuid.GetType() == typeof(System.Guid))
            {
                // convert the object type to a GUID
                System.Guid tempGuid = (System.Guid)serialNumberGuid;

                // copy the data into a byte array
                byte[] byteArray = tempGuid.ToByteArray();

                // serial numbers starts always with "HW"
                if (((char)byteArray[0] == 'H') && ((char)byteArray[1] == 'W'))
                {
                    // run a loop and add the byte data as char to the result string
                    resultText = "";
                    int i = 0;
                    while (true)
                    {
                        // the string stops with a zero
                        if (byteArray[i] != 0)
                        {
                            resultText += (char)byteArray[i];
                        }
                        else
                        {
                            break;
                        }
                        i++;

                        // stop also when all bytes are converted to the string
                        // but this should never happen
                        if (i == byteArray.Length)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // if the data did not start with "HW" convert only the GUID to a string
                    resultText = serialNumberGuid.ToString();
                }
            }
            else
            {
                // if the data is not a GUID convert it to a string
                string tempString = (string)(string)serialNumberGuid;
                resultText = "";
                for (int i = 0; i < tempString.Length; i++)
                {
                    if (tempString[i] != 0)
                    {
                        resultText += tempString[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return resultText;
        }

        //************************************************************************
        /// <summary>
        /// Print a CAN message
        /// </summary>
        /// <param name="canMessage"></param>
        //************************************************************************
        private static void PrintMessage(ICanMessage canMessage)
        {
            switch (canMessage.FrameType)
            {
                //
                // show data frames
                //
                case CanMsgFrameType.Data:
                    {
                        if (!canMessage.RemoteTransmissionRequest)
                        {
                            Console.Write("\nTime: {0,10}  ID: {1,3:X}  DLC: {2,1}  Data:",
                                          canMessage.TimeStamp,
                                          canMessage.Identifier,
                                          canMessage.DataLength);

                            for (int index = 0; index < canMessage.DataLength; index++)
                            {
                                Console.Write(" {0,2:X}", canMessage[index]);
                            }
                        }
                        else
                        {
                            Console.Write("\nTime: {0,10}  ID: {1,3:X}  DLC: {2,1}  Remote Frame",
                                          canMessage.TimeStamp,
                                          canMessage.Identifier,
                                          canMessage.DataLength);
                        }
                        break;
                    }

                //
                // show informational frames
                //
                case CanMsgFrameType.Info:
                    {
                        switch ((CanMsgInfoValue)canMessage[0])
                        {
                            case CanMsgInfoValue.Start:
                                Console.Write("\nCAN started...");
                                break;

                            case CanMsgInfoValue.Stop:
                                Console.Write("\nCAN stopped...");
                                break;

                            case CanMsgInfoValue.Reset:
                                Console.Write("\nCAN reseted...");
                                break;
                        }
                        break;
                    }

                //
                // show error frames
                //
                case CanMsgFrameType.Error:
                    {
                        switch ((CanMsgError)canMessage[0])
                        {
                            case CanMsgError.Stuff:
                                Console.Write("\nstuff error...");
                                break;

                            case CanMsgError.Form:
                                Console.Write("\nform error...");
                                break;

                            case CanMsgError.Acknowledge:
                                Console.Write("\nacknowledgment error...");
                                break;

                            case CanMsgError.Bit:
                                Console.Write("\nbit error...");
                                break;

                            case CanMsgError.Fdb:
                                Console.Write("\nfast data bit error...");
                                break;

                            case CanMsgError.Crc:
                                Console.Write("\nCRC error...");
                                break;

                            case CanMsgError.Dlc:
                                Console.Write("\nData length error...");
                                break;

                            case CanMsgError.Other:
                                Console.Write("\nother error...");
                                break;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        ///   Transmits a CAN message by SDO.
        /// </summary>
        private static void TransmitData(uint id, byte[] data, byte dataLength)
        {
            IMessageFactory factory = VciServer.Instance().MsgFactory;
            ICanMessage canMsg = (ICanMessage)factory.CreateMsg(typeof(ICanMessage));

            canMsg.TimeStamp = 0;
            canMsg.Identifier = id;
            canMsg.FrameType = CanMsgFrameType.Data;
            canMsg.DataLength = dataLength;
            canMsg.SelfReceptionRequest = true;  // show this message in the console window

            for (Byte i = 0; i < canMsg.DataLength; i++)
            {
                canMsg[i] = data[i];
            }

            // Write the CAN message into the transmit FIFO
            mWriter.SendMessage(canMsg);
        }

        private void PdoThreadFunc()
        {
            if (this.readPDO == null || this.readPDO.Count == 0)
            {
                throw new ApplicationException("missing readPDO");
            }
            var savePDO = new Dictionary<byte, ICanCyclicTXMsg>(this.readPDO);
            do
            {
                var isChanged = false;
                foreach (var pdo in this.readPDO)
                {
                    var node = pdo.Key;
                    var pdoMsg = pdo.Value;
                    if (savePDO.TryGetValue(node, out var tmp))
                    {
                        var isChangedNode = false;
                        var pdoData = new byte[tmp.DataLength];
                        for (int i = 0; i < pdoMsg.DataLength; i++)
                        {
                            if (pdoMsg[i] != tmp[i])
                            {
                                tmp[i] = pdoMsg[i];
                                isChangedNode = true;
                            }
                            pdoData[i] = pdoMsg[i];
                        }
                        if (isChangedNode)
                        {
                            isChanged = true;
                            this.messagePDO.Enqueue(new MessagePDO(node, pdoData));
                        }
                    }
                }
                if (isChanged)
                {
                    this.ImplicitMessageEvent?.Invoke(this);
                }
                Thread.Sleep(10);
            } while (0 == mMustQuit);
        }

        //************************************************************************
        /// <summary>
        /// Demonstrate reading messages via MsgReader::ReadMessage() function
        /// </summary>
        //************************************************************************
        private void ReadMsgsViaReadMessage()
        {
            ICanMessage canMessage;

            do
            {
                // Wait 50 msec for a message reception
                if (mRxEvent.WaitOne(50, false))
                {
                    // read a CAN message from the receive FIFO
                    while (mReader.ReadMessage(out canMessage))
                    {
                        PrintMessage(canMessage);
                        if (canMessage.FrameType == CanMsgFrameType.Data
                            && canMessage.DataLength > 0)
                        {
                            var data = new byte[canMessage.DataLength];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = canMessage[i];
                            }
                            if (!this.ReceiveSDO(canMessage.Identifier, data)
                                && !this.ReceiveCanBase(canMessage.Identifier, data))
                            {
                            }
                        }
                    }
                }
            } while (0 == mMustQuit);
        }

        private bool ReceiveCanBase(uint id, byte[] data)
        {
            if (id > COB_ID_SYNC && id < (COB_ID_SYNC + COB_MAX_NODE))
            {
                this.messageEmergency.Enqueue(new MessageEmergency(
                    node: (byte)(id - COB_ID_SYNC),
                    error: BitConverter.ToUInt16(data),
                    register: data[2],
                    manufacturerError: BitConverter.ToUInt16(data, 6)));
                this.ImplicitMessageEvent?.Invoke(this);
                return true;
            }
            if (id == COB_ID_SYNC)
            {
                this.messageNMT.Enqueue(new MessageNMT(true));
                this.ImplicitMessageEvent?.Invoke(this);
                return true;
            }
            if (id > COB_ID_NMT && id < (COB_ID_NMT + COB_MAX_NODE))
            {
                this.messageNMT.Enqueue(new MessageNMT(
                    node: (byte)(id - COB_ID_NMT),
                    state: (byte)(data.Length > 0 ? (data[0] & 0x7F) : 0)));
                this.ImplicitMessageEvent?.Invoke(this);
                return true;
            }
            return false;
        }

        /// <summary>
        /// ReceiveSDO
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns>true if the received message is an SDO</returns>
        private bool ReceiveSDO(uint id, byte[] data)
        {
            byte node = 0;
            if (id > COB_ID_RXSDO && id < (COB_ID_RXSDO + COB_MAX_NODE))
            {
                node = (byte)(id - COB_ID_RXSDO);
                var command = data[0];
                if (this.messageSDO != null
                    && this.messageSDO.TryGetValue(node, out var msg))
                {
                    if (command == 0x80)
                    {
                        msg.AbortCode = data[4];
                        msg.AbortCode <<= 8 + data[5];
                        msg.AbortCode <<= 16 + data[6];
                        this.sdoEvent.Set();
                        return true;
                    }
                    if (msg.CheckSegmented(command, data))
                    {
                        if (msg.IsWrite)
                        {
                            if (command == 0x41
                                || command == 0x20
                                || command == 0x30)
                            {
                                msg.ContinueWrite();
                                TransmitData(msg.Id, msg.TxData, 8);
                            }
                        }
                        else
                        {
                            if (command == 0x41
                                || command == 0
                                || command == 0x10)
                            {
                                msg.ContinueRead();
                                TransmitData(msg.Id, msg.TxData, 8);
                            }
                        }
                    }
                    else
                    {
                        if (msg.IsWrite)
                        {
                            // SDO message transmitted
                            msg.TransmittedBytes = msg.DataLength;
                        }
                        else
                        {
                            // SDO message received
                            msg.DataLength = msg.ReceivedBytes;
                        }
                        this.sdoEvent.Set();
                    }
                    return true;
                }
            }
            return false;
        }

        //************************************************************************
        /// <summary>
        ///   This method is the works as receive thread.
        /// </summary>
        //************************************************************************
        private void ReceiveThreadFunc()
        {
            this.ReadMsgsViaReadMessage();
            //
            // alternative: use ReadMultipleMsgsViaReadMessages();
            //
        }

        #endregion
    }
}
