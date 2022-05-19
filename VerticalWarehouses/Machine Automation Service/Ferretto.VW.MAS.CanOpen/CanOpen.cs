using System;
using System.Collections;
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

        #endregion

        #region Constructors

        public CanOpen()
        {
        }

        #endregion

        #region Methods

        //************************************************************************
        /// <summary>
        ///   Selects the first CAN adapter.
        /// </summary>
        //************************************************************************
        public void SelectDevice()
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
                            resultText += (char)byteArray[i];
                        else
                            break;
                        i++;

                        // stop also when all bytes are converted to the string
                        // but this should never happen
                        if (i == byteArray.Length)
                            break;
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
                        resultText += tempString[i];
                    else
                        break;
                }
            }

            return resultText;
        }

        #endregion
    }
}
