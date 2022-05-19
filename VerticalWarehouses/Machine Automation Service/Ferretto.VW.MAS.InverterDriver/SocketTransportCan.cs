using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.EnIPStack;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Microsoft.Extensions.Configuration;
using NLog;
using IXXAT;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class SocketTransportCan : ISocketTransportCan, IDisposable
    {
        #region Fields

        public const int UdpPollingInterval = 50;

        private const int idlePollingInterval = 1200;

        private const byte m_cPDO = 1;

        private readonly Timer implicitTimer;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The timeout for read operations on the socket.
        /// </summary>
        /// <remarks>
        /// The value -1 is used to indicate no timeout.
        /// </remarks>
        private readonly int readTimeoutMilliseconds;

        private bool isDisposed;

        private ushort m_boardHandle;

        private int m_CANline;

        private CANopenMasterAPI6.COP_t_EventCallback m_emcyCallback;

        private CANopenMasterAPI6.COP_t_EventCallback m_pdoCallback;

        private Dictionary<int, byte[]> m_pdoInData;

        private Dictionary<int, byte[]> m_pdoOutData;

        private CANopenMasterAPI6.COP_t_EventCallback m_statusCallback;

        private IEnumerable<int> nodeList;

        private DateTime receivedImplicitTime;

        #endregion

        #region Constructors

        public SocketTransportCan(IConfiguration configuration)
        {
            this.readTimeoutMilliseconds = configuration.GetValue<int>("Vertimag:Drivers:Inverter:ReadTimeoutMilliseconds", -1);
            this.implicitTimer = new Timer(this.ImplicitTimer, null, -1, -1);
        }

        #endregion

        #region Events

        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        public event EventHandler<ImplicitReceivedEventArgs> ImplicitReceivedChanged;

        #endregion

        #region Properties

        public bool IsConnected { get; set; }

        public bool IsConnectedUdp { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress inverterAddress, int sendPort, IEnumerable<int> nodeList = null)
        {
            var boardID = CANopenMasterAPI6.COP_1stBOARD;
            var boardType = CANopenMasterAPI6.COP_DEFAULTBOARD;
            UInt32 abortcode = 0;
            this.m_CANline = 0;
            if (nodeList is null)
            {
                throw new ApplicationException($"nodeList is null");
            }
            this.nodeList = nodeList;
            var result = CANopenMasterAPI6.COP_InitBoard(out this.m_boardHandle, ref boardType, ref boardID, this.m_CANline);
            if (CANopenMasterAPI6.COP_k_OK != result)
            {
                throw new ApplicationException($"CANopenMasterAPI6: Error init DLL");
            }

            this.m_pdoCallback = new CANopenMasterAPI6.COP_t_EventCallback(this.PDOCallback);
            this.m_emcyCallback = new CANopenMasterAPI6.COP_t_EventCallback(this.EmcyCallback);
            this.m_statusCallback = new CANopenMasterAPI6.COP_t_EventCallback(this.StatusCallback);
            this.m_pdoOutData = new Dictionary<int, byte[]>();
            this.m_pdoInData = new Dictionary<int, byte[]>();

            result = CANopenMasterAPI6.COP_DefineCallbacks(this.m_boardHandle   //  handle of CAN board
                                                          , this.m_pdoCallback   //  fp_rx_pdo
                                                          , this.m_emcyCallback  //  fp_emergency
                                                          , this.m_statusCallback//  fp_net_event
                                                          , null);         //  fp_sync
            if (CANopenMasterAPI6.COP_k_OK != result)
            {
                CANopenMasterAPI6.COP_ReleaseBoard(this.m_boardHandle);
                this.m_boardHandle = 0;
                throw new ApplicationException($"CANopenMasterAPI6: Error RegisterCallbacks");
            }

            result = CANopenMasterAPI6.COP_InitInterface(this.m_boardHandle     //  handle of CAN board
                                                        , CANopenMasterAPI6.COP_k_BAUD_CIA
                                                        , CANopenMasterAPI6.COP_k_250_KB         //  baudrate
                                                        , 0                 //  m_bNode of the master (here: not used)
                                                        , 0                 //  heartbeat time for the master
                                                        , CANopenMasterAPI6.COP_k_NO_FEATURES);
            if (CANopenMasterAPI6.COP_k_OK != result)
            {
                CANopenMasterAPI6.COP_ReleaseBoard(this.m_boardHandle);
                this.m_boardHandle = 0;
                throw new ApplicationException($"CANopenMasterAPI6: Error COP_InitInterface");
            }

            foreach (var nodeId in nodeList)
            {
                result = CANopenMasterAPI6.COP_AddNode(this.m_boardHandle           //  handle of CAN board
                                                      , (byte)nodeId                //  number of new node 1-127
                                                      , CANopenMasterAPI6.COP_k_HEARTBEAT // heartbeat or node guarding
                                                      , Convert.ToUInt16(this.readTimeoutMilliseconds + 100)//  heartbeat time [ms] incl. Jitter of 20%
                                                      , 0);                    //  lifetime factor (not applicable)

                if (CANopenMasterAPI6.COP_k_OK != result)
                {
                    CANopenMasterAPI6.COP_ReleaseBoard(this.m_boardHandle);
                    this.m_boardHandle = 0;
                    throw new ApplicationException($"CANopenMasterAPI6: Error COP_AddNode {nodeId}");
                }
                result = CANopenMasterAPI6.COP_SearchNode(this.m_boardHandle, (byte)nodeId);
                if (CANopenMasterAPI6.COP_k_OK == result)
                {
                    // set the producer heartbeat time
                    Byte[] txdata = BitConverter.GetBytes((ushort)this.readTimeoutMilliseconds);

                    result = CANopenMasterAPI6.COP_WriteSDO(this.m_boardHandle, (byte)nodeId,
                                                            CANopenMasterAPI6.COP_k_DEFAULT_SDO, CANopenMasterAPI6.COP_k_NO_BLOCKTRANSFER,
                                                            0x1017, 0x00,
                                                            (UInt32)txdata.Length, txdata, out abortcode);

                    // create PDOs
                    result = CANopenMasterAPI6.COP_CreatePDO(this.m_boardHandle         //  handle of CAN board
                                                            , (byte)nodeId              //  number of the node
                                                            , m_cPDO                //  number of the pdo
                                                            , CANopenMasterAPI6.COP_k_PDO_TYP_RX    // type of pdo
                                                            , CANopenMasterAPI6.COP_k_PDO_MODE_ASYNC// transmission type of pdo
                                                            , 8                                     // datalength of pdo (1..8)
                                                            , (UInt16)(CANopenMasterAPI6.COP_k_M_ID_RxPDO1 + nodeId));// CANID of pdo
                    if (CANopenMasterAPI6.COP_k_OK != result)
                    {
                        CANopenMasterAPI6.COP_ReleaseBoard(this.m_boardHandle);
                        this.m_boardHandle = 0;
                        throw new ApplicationException($"CANopenMasterAPI6: Error COP_CreatePDO rx Node {nodeId}");
                    }

                    result = CANopenMasterAPI6.COP_CreatePDO(this.m_boardHandle         //  handle of CAN board
                                                            , (byte)nodeId              //  number of the node
                                                            , m_cPDO                //  number of the pdo
                                                            , CANopenMasterAPI6.COP_k_PDO_TYP_TX    //  type of pdo
                                                            , CANopenMasterAPI6.COP_k_PDO_MODE_ASYNC//  transmission type of pdo
                                                            , 8                                     //  datalength of pdo (1..8)
                                                            , (UInt16)(CANopenMasterAPI6.COP_k_M_ID_TxPDO1 + nodeId));// CANID of pdo
                    if (CANopenMasterAPI6.COP_k_OK != result)
                    {
                        CANopenMasterAPI6.COP_ReleaseBoard(this.m_boardHandle);
                        this.m_boardHandle = 0;
                        throw new ApplicationException($"CANopenMasterAPI6: Error COP_CreatePDO tx Node {nodeId}");
                    }

                    // start node
                    result = CANopenMasterAPI6.COP_StartNode(this.m_boardHandle, 0);
                    if (CANopenMasterAPI6.COP_k_OK != result)
                    {
                        CANopenMasterAPI6.COP_ReleaseBoard(this.m_boardHandle);
                        this.m_boardHandle = 0;
                        throw new ApplicationException($"CANopenMasterAPI6: Error COP_StartNode");
                    }

                    this.m_pdoOutData.Add(nodeId, new byte[8]);
                    this.m_pdoInData.Add(nodeId, new byte[8]);
                    this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
                }
            }
            this.IsConnected = true;
        }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive, out int length)
        {
            throw new NotImplementedException();
        }

        public bool ImplicitMessageWrite(byte[] data, int node)
        {
            var write = false;
            if (this.m_pdoOutData != null)
            {
                this.m_pdoOutData[node] = data;
                write = true;
            }
            return write;
        }

        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        public bool SDOMessage(byte nodeId, ushort index, byte subindex, bool isWriteMessage, byte[] data, out byte[] receive, out int length)
        {
            receive = null;
            Byte[] rxdata = new Byte[4096];
            UInt32 rxlen = (UInt32)rxdata.Length;
            int res = CANopenMasterAPI6.COP_k_NO;
            var isOk = false;
            UInt32 abortcode = 0;
            length = 0;

            if (isWriteMessage && data != null)
            {
                res = CANopenMasterAPI6.COP_WriteSDO(this.m_boardHandle   //  handle of CAN board
                                                      , nodeId         //  number of the node
                                                      , CANopenMasterAPI6.COP_k_DEFAULT_SDO
                                                      , CANopenMasterAPI6.COP_k_NO_BLOCKTRANSFER
                                                      , index           //  index in OV
                                                      , subindex            //  subindex in OV
                                                      , (uint)data.Length           //  length of transmit data
                                                      , data          //  transmit data
                                                      , out abortcode);//  abort code of SDO-transfer
            }
            else if (!isWriteMessage)
            {
                res = CANopenMasterAPI6.COP_ReadSDO(this.m_boardHandle   //  handle of CAN board
                                                    , nodeId         //  number of the node
                                                    , CANopenMasterAPI6.COP_k_DEFAULT_SDO
                                                    , CANopenMasterAPI6.COP_k_NO_BLOCKTRANSFER
                                                    , index           //  index in OV
                                                    , subindex            //  subindex in OV
                                                    , ref rxlen       //  size of buffer / length of received data
                                                    , rxdata          //  received data
                                                    , out abortcode);//  abort code of SDO-transfer
            }
            isOk = CANopenMasterAPI6.COP_k_OK == res;

            if (isOk)
            {
                length = (int)rxlen;
                receive = rxdata;
                this.logger.Trace("Node: " + nodeId
                                + $" object 0x{index:X04}/{subindex}"
                                + (isWriteMessage ? $" tx {BitConverter.ToString(data)} " : $"  rx {BitConverter.ToString(receive)}"));
            }
            else if (CANopenMasterAPI6.COP_k_ABORT == res)
            {
                this.logger.Error("Node: " + nodeId
                                + $" object 0x[{index:X04}/{subindex}] "
                                + $" AbortCode {abortcode:X08}h (" + CANopenMasterAPI6.CopAbortCodeString(abortcode) + ")");
            }
            else
            {
                this.logger.Error("Node: " + nodeId
                                + $" object 0x[{index:X04}/{subindex}] "
                                + CANopenMasterAPI6.CopErrorString(res));
            }

            return isOk;
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, int delay, CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            if (disposing)
            {
                this.implicitTimer?.Dispose();
            }
        }

        private void EmcyCallback(ushort boardhdl, byte que_num, byte canline)
        {
            Int16 result = 0;
            Byte nodeId;
            UInt16 errorValue;
            Byte errorRegister;
            Byte[] errorData = new Byte[5];

            var errors = new List<string>();

            do
            {
                result = CANopenMasterAPI6.COP_GetEmergencyObj(this.m_boardHandle
                                                              , out nodeId
                                                              , out errorValue
                                                              , out errorRegister
                                                              , errorData);
                switch (result)
                {
                    case CANopenMasterAPI6.COP_k_OK:
                        this.logger.Debug($" Node: " + nodeId
                                              + "  ErrVal: " + errorValue.ToString("X04") + "h"
                                              + "  ErrReg: " + errorRegister.ToString("X02") + "h"
                                              + "  Data: "
                                              + errorData[0].ToString("X02") + " "
                                              + errorData[1].ToString("X02") + " "
                                              + errorData[2].ToString("X02") + " "
                                              + errorData[3].ToString("X02") + " "
                                              + errorData[4].ToString("X02") + " hex");
                        break;

                    default:
                        if (CANopenMasterAPI6.COP_k_QUEUE_EMPTY != result)
                        {
                            this.logger.Error(result.ToString());
                        }
                        break;
                }
                // Important: Always read until queue is empty
            } while (CANopenMasterAPI6.COP_k_QUEUE_EMPTY != result);
        }

        private void ImplicitTimer(object state)
        {
            this.implicitTimer?.Change(-1, -1);
            foreach (var nodeId in this.nodeList)
            {
                Int16 result;

                if (this.m_pdoOutData.ContainsKey(nodeId)
                    && this.m_pdoOutData[nodeId] != null
                    && this.m_pdoOutData[nodeId].Length > 0)
                {
                    result = CANopenMasterAPI6.COP_WritePDO(this.m_boardHandle             //  handle of CAN board
                                                            , (byte)nodeId                  //  number of the node
                                                            , m_cPDO                    //  number of the pdo
                                                            , this.m_pdoOutData[nodeId]);           //  data to transmit
                }
            }
            this.implicitTimer?.Change(UdpPollingInterval, UdpPollingInterval);
        }

        private void PDOCallback(ushort boardhdl, byte que_num, byte canline)
        {
            Int16 result;
            Byte node;
            Byte pdo;
            Byte[] rxData = new Byte[8];
            Byte rxLen;
            Byte SyncCounter;

            do
            {
                result = CANopenMasterAPI6.COP_ReadPDO(this.m_boardHandle             //  Handle of CAN board
                                                      , out node                  //  Number of the node
                                                      , out pdo                   //  Number of the pdo
                                                      , out rxLen                 //  Number of received data bytes
                                                      , rxData                    //  Received data bytes
                                                      , out SyncCounter);        //  Sync counter value upon reception
                switch (result)
                {
                    case CANopenMasterAPI6.COP_k_OK:
                        if (m_cPDO == pdo
                            && this.m_pdoInData.ContainsKey(node))
                        {
                            this.m_pdoInData[node] = rxData;
                            var args = new ImplicitReceivedEventArgs();
                            args.receivedMessage = this.m_pdoInData[node];
                            args.isOk = true;
                            args.node = node;
                            this.receivedImplicitTime = DateTime.UtcNow;
                            this.ImplicitReceivedChanged?.Invoke(this, args);
                        }
                        break;

                    case CANopenMasterAPI6.COP_k_QUEUE_EMPTY:
                        // No more data to read
                        if (this.m_pdoInData.ContainsKey(node))
                        {
                            var args = new ImplicitReceivedEventArgs();
                            args.receivedMessage = this.m_pdoInData[node];
                            args.isOk = true;
                            args.node = node;
                            this.receivedImplicitTime = DateTime.UtcNow;
                            this.ImplicitReceivedChanged?.Invoke(this, args);
                        }
                        break;

                    default:
                        // error
                        break;
                }
            } while (CANopenMasterAPI6.COP_k_QUEUE_EMPTY != result);
        }

        private void StatusCallback(ushort boardhdl, byte que_num, byte canline)
        {
            Int16 result;
            Byte eventType;
            Byte eventData1;
            Byte eventData2;
            Byte eventData3;
            var errors = new List<string>();

            do
            {
                result = CANopenMasterAPI6.COP_GetEvent(this.m_boardHandle
                                                        , out eventType
                                                        , out eventData1
                                                        , out eventData2
                                                        , out eventData3);
                switch (result)
                {
                    case CANopenMasterAPI6.COP_k_OK:
                        this.logger.Debug(" EventType: " + eventType.ToString("X02") + "h "
                                                + "(" + CANopenMasterAPI6.CopEventTypeString(eventType) + ")"
                                                + "\tData: "
                                                + eventData1.ToString("X02") + " "
                                                + eventData2.ToString("X02") + " "
                                                + eventData3.ToString("X02") + " hex");
                        break;

                    default:
                        if (CANopenMasterAPI6.COP_k_QUEUE_EMPTY != result)
                        {
                            this.logger.Error(result.ToString());
                        }
                        break;
                }
                // Important: Always read until queue is empty
            } while (CANopenMasterAPI6.COP_k_QUEUE_EMPTY != result);
        }

        #endregion
    }
}
