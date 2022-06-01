using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Ixxat.Vci4;
using Ixxat.Vci4.Bal;
using Ixxat.Vci4.Bal.Can;

namespace Ferretto.VW.MAS.CanOpenClient
{
    public class CanOpen
    {
        #region Fields

        private static readonly Dictionary<ulong, string> errorStrings = new Dictionary<ulong, string>()
        {
            {0, "No error" },
            {1, "Server error: too many SDO objects" },
            {2, "Server error: SDO object not found" },
            {3, "Server error: timeout" },
            {4, "Server error: SDO read received length 0" },
            {0x06010000, "Unsupported access to an object. Parameter cannot be written or read" },
            {0x06020000, "Object does not exist. Parameter does not exist" },
            {0x06040047, "General internal incompatibility in the device. Data sets differ" },
            {0x06060000, "Access failed due to a hardware error. EEPROM Error (R/W/checksum)" },
            {0x06070010, "Data type does not match. Parameter has a different data type" },
            {0x06070012, "Data type does not match or length of Service telegram too big. Parameter has a different data type or telegram length not correct." },
            {0x06070013, "Data type does not match or length of Service telegram too small. Parameter has a different data type or telegram length not correct." },
            {0x06090011, "Subindex does not exist. Data set does not exist" },
            {0x06090030, "Value range of parameter exceeded. Parameter value too large or too small" },
            {0x06090031, "Value of parameter written too high. Parameter value too large" },
            {0x06090032, "Value of parameter written too low. Parameter value too small" },
            {0x08000020, "Data cannot be transmitted or saved. Invalid value for operation" },
            {0x08000021, "Data cannot be transferred because of local control. Parameter cannot be written in operation" },
            {0x08000022, "No data transfer because of present device state. NMT state machine is not in correct state" },
        };

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

        /// <summary>
        ///   Thread that handles the message reception.
        /// </summary>
        private static Thread rxThread;

        private Dictionary<byte, ICanCyclicTXMsg> messagePDO;

        private Dictionary<byte, MessageSDO> messageSDO;

        private int readTimeout;

        private AutoResetEvent sdoEvent;

        #endregion

        #region Constructors

        public CanOpen()
        {
        }

        #endregion

        #region Methods

        public string ErrorString(ulong abortCode)
        {
            if (errorStrings.TryGetValue(abortCode, out var errorString))
            {
                return errorString;
            }
            return $"Error code {abortCode:X08}";
        }

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

                this.messagePDO = new Dictionary<byte, ICanCyclicTXMsg>();
                foreach (var node in nodeList)
                {
                    // TXPDO
                    this.messagePDO.Add((byte)node, CreateCyclicMsg(0x180 + (uint)node, 8));

                    // RXPDO
                    this.messagePDO.Add((byte)node, CreateCyclicMsg(0x200 + (uint)node, 8));
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

            this.FinalizeApp();
        }

        public bool WritePDO(byte node, byte[] data)
        {
            if (this.messagePDO.TryGetValue(node, out var msg))
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

        private static byte EncodedLength(byte dataLength)
        {
            switch (dataLength)
            {
                case 1: return 0xF;
                case 2: return 0xB;
                case 3: return 0x7;
                case 4: return 0x3;
                default: return 0;
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
                                && !this.ReceivePDO(canMessage.Identifier, data))
                            {
                            }
                        }
                    }
                }
            } while (0 == mMustQuit);
        }

        private bool ReceivePDO(uint identifier, byte[] data)
        {
            throw new NotImplementedException();
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
            if (id > 0x580 && id < 0x600)
            {
                node = (byte)(id - 0x580);
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
