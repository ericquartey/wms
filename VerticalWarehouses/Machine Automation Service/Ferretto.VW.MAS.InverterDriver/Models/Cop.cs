/*************************************************************************
** HMS Networks AB
**************************************************************************
** Summary : Class CANopenMasterAPI6 declaring constants and function
**           prototypes for IXXAT CANopen Master API 6.
** Compiler: Visual Studio (.NET Standard 2.0)
**************************************************************************
** Copyright (C) 2002-2019 HMS Technology Center Ravensburg GmbH, all rights reserved
**************************************************************************/
using System;
using System.Runtime.InteropServices;

/*##########################################################################
 Name:
  IXXAT

 Description:
  Contains IXXAT product related type definitions.
##########################################################################*/

namespace IXXAT
{
    /// <summary>
    /// Class CANopenMasterAPI6 declares constants and function prototypes of IXXAT CANopen Master API 6.
    /// </summary>
    public class CANopenMasterAPI6
    {
        /// <summary>
        /// Name of the DLL that provides the CANopen Master API
        /// </summary>
        //public const string CANopenMasterAPI6lib = "XatCOP60.dll"; //  Windows 32bit
        public const string CANopenMasterAPI6lib = "XatCOP60-64.dll";//  Windows 64bit

        //public const string CANopenMasterAPI6lib = "libXatCOP60.so";//  ELF 64bit

#pragma warning disable 1591    // Missing XML comment for publicly visible type or member

        #region cop Constants

        /// <summary>
        /// Errorcodes (returnvalues)
        /// </summary>
        public const Int16 BER_k_OK = 0;  //  success

        public const Int16 BER_k_ERR = 1;  //  general error

        public const Int16 COP_k_NO_OBJECTS = 9;  //  compatibility entry for COP_k_QUEUE_EMPTY

        public const Int16 BER_k_EDS_FILENOTFOUND = -44;  //  device description file not found

        public const Int16 BER_k_EDS_CORRUPT = -43;  //  device description file import failed

        public const Int16 BER_k_EDSLIB = -42;  //  EDSLIB4.dll is missing

        public const Int16 BER_k_DATA_CORRUPT = -41;  //  corrupt data detected MC to PC

        public const Int16 BER_k_NOT_SENT = -40;  //  msg not sent; try again

        public const Int16 BER_k_TIMEOUT = -38;  //  timeout in communication PC to MC

        public const Int16 BER_k_BOARD_ALREADY_USED = -37;  //  board is used by another instance

        public const Int16 BER_k_ALL_BOARDS_USED = -36;  //  no free board slots inside DLL

        public const Int16 BER_k_BOARD_NOT_SUPP = -35;  //  the given board is not supported by CANopen Master API

        public const Int16 BER_k_BOARD_NOT_FOUND = -34;  //  the board wasn't found

        public const Int16 BER_k_CANNOT_SEARCH_BOARD = -33;  //  Hardware selection Dialog cancelled by user

        public const Int16 BER_k_WRONG_FW = -32;  //  wrong firmware version

        public const Int16 BER_k_USED_FROM_OTHER_PROCESS = -31;  //  board is used by another application

        public const Int16 BER_k_PC_MC_COMM_ERR = -30;  //  communication error PC to MC

        public const Int16 BER_k_BOARD_DLD_ERR = -29;  //  an error occured while firmware download

        public const Int16 BER_k_BADCALLBACK_PTR = -28;  //  a callbackpointer is invalid

        public const Int16 BER_k_NO_SUCH_CANLINE = -27;  //  given CANline is not available or not supported

        public const Int16 BER_k_CANLINE_USED = -26;  //  CANline is already in use

        public const Int16 BER_k_VCI_INST_ERR = -25;  //  IXXAT CAN driver is missing

        public const Int16 BER_k_BOARD_ERR = -24;  //  unknown board type or can't locate board type

        public const Int16 BER_k_MEM_ALLOC_ERR = -23;  //  memory allocation error (internal) data or OS element couldn't be created

        public const Int16 BER_k_CCI_INST_ERR = -22;  //  CCI installation error (internal)

        public const Int16 BER_k_SDO_INST_ERR = -21;  //  SDO handler installation error (internal)

        public const Int16 BER_k_SDO_THREAD_ERR = -20;  //  SDO thread execution cancelled while waiting for SDO response from firmware

        /// <summary>
        /// Constants for COP_InitBoard()
        /// </summary>
        public static readonly Guid COP_DEFAULTBOARD = new Guid("{0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}");

        public static readonly Guid COP_BOARDDIALOG = new Guid("{0xFFFFFFFF,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_1stBOARD = new Guid("{0x00000000,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_2ndBOARD = new Guid("{0x00000001,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_3rdBOARD = new Guid("{0x00000002,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_4thBOARD = new Guid("{0x00000003,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_5thBOARD = new Guid("{0x00000004,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_6thBOARD = new Guid("{0x00000005,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_7thBOARD = new Guid("{0x00000006,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_8thBOARD = new Guid("{0x00000007,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public static readonly Guid COP_9thBOARD = new Guid("{0x00000008,0xFFFF,0xFFFF,{0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}}");

        public const Int32 COP_FIRSTLINE = 0x00;

        public const Int32 COP_SECONDLINE = 0x01;

        public const Int32 COP_THIRDLINE = 0x02;

        public const Int32 COP_FOURTHLINE = 0x03;

        public const Int32 COP_SINGLELINE = 0xFF;

        public const Int32 COP_VCI3GENERIC = 0x100;

        /// <summary>
        /// Definitions for LSS-Configuration-Modes
        /// </summary>
        public const Byte LSS_k_CLR_MODE_ALL = 0;

        public const Byte LSS_k_SET_MODE_SWITCH_MODE_GLOBAL = 1;

        public const Byte LSS_k_SET_MODE_STORE_CONFIGURATION = 2;

        public const Byte LSS_k_SET_MODE_ACTIVATE_NEW_BAUDRATE = 4;

        /// <summary>
        /// LSS-function return values
        /// </summary>
        public const Int16 LSS_k_MEDIA_ACCESS_ERROR = 4;   //  CAN bus access failed

        public const Int16 LSS_k_PROTOCOL_ERR = 7;   //  invalid device response

        public const Int16 LSS_k_BSY = 11;   //  currently processing a LSS command sequence

        public const Int16 LSS_k_FS_NO_NONCONFIGURED_SLAVE = 16;//  No non-configured slave responded

        public const Int16 LSS_k_FS_NF_NONCONFIGURED_SLAVE = 18;//  Not Found the non-configured slave

        /// <summary>
        /// Definitions of PDO/SDO base identifiers
        /// </summary>
        public const UInt16 COP_k_ID_EMCY = 0x080;

        public const UInt16 COP_k_ID_GUARDING = 0x700;

        //  slave view
        public const UInt16 COP_k_S_ID_TxPDO1 = 0x180;

        public const UInt16 COP_k_S_ID_RxPDO1 = 0x200;

        public const UInt16 COP_k_S_ID_TxPDO2 = 0x280;

        public const UInt16 COP_k_S_ID_RxPDO2 = 0x300;

        public const UInt16 COP_k_S_ID_TxPDO3 = 0x380;

        public const UInt16 COP_k_S_ID_RxPDO3 = 0x400;

        public const UInt16 COP_k_S_ID_TxPDO4 = 0x480;

        public const UInt16 COP_k_S_ID_RxPDO4 = 0x500;

        public const UInt16 COP_k_S_ID_TxSDO = 0x580;

        public const UInt16 COP_k_S_ID_RxSDO = 0x600;

        //  master view
        public const UInt16 COP_k_M_ID_TxPDO1 = 0x200;

        public const UInt16 COP_k_M_ID_RxPDO1 = 0x180;

        public const UInt16 COP_k_M_ID_TxPDO2 = 0x300;

        public const UInt16 COP_k_M_ID_RxPDO2 = 0x280;

        public const UInt16 COP_k_M_ID_TxPDO3 = 0x400;

        public const UInt16 COP_k_M_ID_RxPDO3 = 0x380;

        public const UInt16 COP_k_M_ID_TxPDO4 = 0x500;

        public const UInt16 COP_k_M_ID_RxPDO4 = 0x480;

        public const UInt16 COP_k_M_ID_TxSDO = 0x600;

        public const UInt16 COP_k_M_ID_RxSDO = 0x580;

        #endregion

        #region copcmd Constants

        /// <summary>
        /// Definitions for errorcodes
        /// (some codes are only used in the firmware and not required on PC-side)
        /// </summary>
        public const Byte COP_k_OK = 0x00;   // Success

        public const Byte COP_k_NO = 0x01;   // Common failure

        public const Byte COP_k_CAL_ERR = 0x02;   // Failure occured in CAL

        public const Byte COP_k_IV = 0x03;   // Invalid parameter

        public const Byte COP_k_ABORT = 0x04;   // Transfer aborted

        public const Byte COP_k_NOT_FOUND = 0x05;   // Unknown Node-Id

        public const Byte COP_k_NOT_INIT = 0x06;   // CANopen-Master not initialised

        public const Byte COP_k_INIT = 0x07;   // CANopen-Master initialised

        public const Byte COP_k_QUEUE_EMPTY = 0x09;   // No Objects in Queue

        public const Byte COP_k_TIMEOUT = 0x0a;   // Timeout in CAN communication

        public const Byte COP_k_CANID_IN_USE = 0x0b;   // CAN-Identifier already in use

        public const Byte COP_k_SDO_RUNNING = 0x10;   // SDO transfer in progress, retry later

        public const Byte COP_k_BSY = 0x11;   // Generic process still running (not finished so far)

        public const Byte COP_k_NO_OBJECT = 0x12;   // Object does not exist

        public const Byte COP_k_NO_SUBINDEX = 0x13;   // Subindex does not exist

        public const Byte COP_k_WRITE_ONLY = 0x14;   // Object is write only

        public const Byte COP_k_PRESENT_DEVICE_STATE = 0x15;   // Access actual not possible

        public const Byte COP_k_RANGE_EXCEEDED = 0x16;   // Parameter out of range

        public const Byte COP_k_UNKNOWN = 0x20;   // Unknown command

        public const Byte COP_k_NO_FLY_MASTER_PRESENT = 0x21;   // API/hardware version does not support Flying Master

        public const Byte COP_k_NO_LOWSPEED = 0x22;   // No LowSpeed bus-coupling present or supported

        /// <summary>
        /// Errortype for use with <c>COP_GetEvent</c>:
        /// Node monitoring / Network management event.
        /// </summary>
        public const Byte COP_k_NMT_EVT = 1;

        /// <summary>
        /// Errortype for use with <c>COP_GetEvent</c>:
        /// CAN data link layer event.
        /// </summary>
        public const Byte COP_k_DLL_EVT = 2;

        /// <summary>
        /// Errortype for use with <c>COP_GetEvent</c>:
        /// WritePDO event.
        /// </summary>
        public const Byte COP_k_WPDO_EVT = 3;

        /// <summary>
        /// Errortype for use with <c>COP_GetEvent</c>:
        /// ReadPDO event.
        /// </summary>
        public const Byte COP_k_RPDO_EVT = 4;

        /// <summary>
        /// Errortype for use with <c>COP_GetEvent</c>:
        /// Queue overrun event in between Firmware and Master (MC and PC).
        /// </summary>
        public const Byte COP_k_QUEUE_OVRUN_EVT = 5;

        /// <summary>
        /// Errortype for use with <c>COP_GetEvent</c>:
        /// Flying master event.
        /// </summary>
        public const Byte COP_k_FLY_EVT = 6;

        /// <summary>
        /// Errortype for use with <c>COP_GetEvent</c>:
        /// Resources limitation event.
        /// </summary>
        public const Byte COP_k_RESOURCE_EVT = 7;

        /// <summary>
        /// Bit-coded errorcode for <c>COP_GetEvent</c> of type COP_k_DLL_EVT:
        /// Error-free and fully able to communicate. This is informative.
        /// </summary>
        public const Byte COP_k_DLL_NOERR = 0;

        /// <summary>
        /// Bit-coded errorcode for <c>COP_GetEvent</c> of type COP_k_DLL_EVT:
        /// Software overrun (CAN receive queue). This is informative.
        /// </summary>
        public const Byte COP_k_DLL_RXOVR = 1;

        /// <summary>
        /// Bit-coded errorcode for <c>COP_GetEvent</c> of type COP_k_DLL_EVT:
        /// CAN overrun. This is informative.
        /// </summary>
        public const Byte COP_k_DLL_COVR = 2;

        /// <summary>
        /// Bit-coded errorcode for <c>COP_GetEvent</c> of type COP_k_DLL_EVT:
        /// CAN bus-off. This is informative.
        /// </summary>
        public const Byte COP_k_DLL_BOFF = 4;

        /// <summary>
        /// Bit-coded errorcode for <c>COP_GetEvent</c> of type COP_k_DLL_EVT:
        /// CAN error-status-bit set. This is informative.
        /// </summary>
        public const Byte COP_k_DLL_ESET = 8;

        /// <summary>
        /// Bit-coded errorcode for <c>COP_GetEvent</c> of type COP_k_DLL_EVT:
        /// CAN error-status-bit reset. This is informative.
        /// </summary>
        public const Byte COP_k_DLL_ERESET = 16;   // CAN: error-status-bit reset

        /// <summary>
        /// Bit-coded errorcode for <c>COP_GetEvent</c> of type COP_k_DLL_EVT:
        /// Software overrun (CAN transmit queue). This is informative.
        /// </summary>
        public const Byte COP_k_DLL_TXOVR = 32;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> of type COP_k_NMT_EVT:
        /// Guard error. Device has not responded or the signaled node-state is unexpected.
        /// </summary>
        public const Byte COP_k_NMT_GUARDERR = 1;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> of type COP_k_NMT_EVT:
        /// Device has sent Bootup message. This is informative.
        /// </summary>
        public const Byte COP_k_NMT_BOOTIND = 2;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> of type COP_k_NMT_EVT:
        /// Heartbeat error. Device has transmitted nothing or signaled node-state is unexpected.
        /// </summary>
        public const Byte COP_k_NMT_HEARTBEATERR = 3;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> (E) of type COP_k_FLY_EVT, and also
        /// Returncode for Flying Master status (F) in <c>COP_GetStatusFlyMasterNeg</c>:
        /// E,F received mastership.
        /// </summary>
        public const Byte COP_k_FLY_MASTER = 4;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> (E) of type COP_k_FLY_EVT, and also
        /// Returncode for Flying Master status (F) in <c>COP_GetStatusFlyMasterNeg</c>:
        /// E,F lost master negotiation.
        /// </summary>
        public const Byte COP_k_FLY_NOT_MASTER = 5;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> (E) of type COP_k_FLY_EVT:
        /// E   high prior node kicked us.
        /// </summary>
        public const Byte COP_k_FLY_LOST_MASTERSHIP = 6;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> (E) of type COP_k_FLY_EVT:
        /// E   lost active master.
        /// </summary>
        public const Byte COP_k_FLY_LOST_ACTIVE_MASTER = 7;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> (E) of type COP_k_FLY_EVT:
        /// E   unknown event.
        /// </summary>
        public const Byte COP_k_FLY_UNKNOWN = 8;

        /// <summary>
        /// Returncode for Flying Master status (F) in <c>COP_GetStatusFlyMasterNeg</c>:
        /// F   waiting for busconnection.
        /// </summary>
        public const Byte COP_k_FLY_WAIT_BUSCONNECTION = 9;

        /// <summary>
        /// Returncode for Flying Master status (F) in <c>COP_GetStatusFlyMasterNeg</c>:
        /// F   negotiation in progress.
        /// </summary>
        public const Byte COP_k_FLY_NEGOTIATION_RUNNING = 10;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> of type COP_k_WPDO_EVT, COP_k_TPDO_EVT:
        /// Invalid parameter in <c>COP_WritePDO</c>.
        /// </summary>
        public const Byte COP_k_ERR_PDO_IV = 1;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> of type COP_k_WPDO_EVT, COP_k_TPDO_EVT:
        /// Overrun of the firmware-internal transmit- or receive queue. This means
        /// that the corresponding PDO was lost.
        /// </summary>
        public const Byte COP_k_ERR_PDO_OVR = 2;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> of type COP_k_RESOURCE_EVT:
        /// CAN-Identifier already in use.
        /// </summary>
        public const Byte COP_k_RESOURCE_CANID = 1;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> of type COP_k_RESOURCE_EVT:
        /// Too many synchronous Transmit PDOs.
        /// </summary>
        public const Byte COP_k_RESOURCE_SYNCTPDO = 2;

        /// <summary>
        /// Errorcode for <c>COP_GetEvent</c> of type COP_k_RESOURCE_EVT:
        /// Too many synchronous Receive PDOs.
        /// </summary>
        public const Byte COP_k_RESOURCE_SYNCRPDO = 3;

        /// <summary>
        /// CAN-baudrate table for use with e.g. <c>COP_InitInterface</c>:
        /// Standard CAN-in-Automation baudrates (default).
        /// </summary>
        public const Byte COP_k_BAUD_CIA = 0;

        /// <summary>
        /// CAN-baudrate table for use with e.g. <c>COP_InitInterface</c>:
        /// User defined baudrates. Requires also <c>COP_SetUserBittiming</c>.
        /// </summary>
        public const Byte COP_k_BAUD_USER = 0x80;

        public const Byte COP_k_1000_KB = 0;

        public const Byte COP_k_800_KB = 1;

        public const Byte COP_k_500_KB = 2;

        public const Byte COP_k_250_KB = 3;

        public const Byte COP_k_125_KB = 4;

        public const Byte COP_k_100_KB = 5;

        public const Byte COP_k_50_KB = 6;

        public const Byte COP_k_20_KB = 7;

        public const Byte COP_k_10_KB = 8;

        /// <summary>
        /// Additional firmware features for use with <c>COP_InitInterface</c>:
        /// None (default).
        /// </summary>
        public const Byte COP_k_NO_FEATURES = 0;

        /// <summary>
        /// Additional firmware features for use with <c>COP_InitInterface</c>:
        /// Activate CANopen Flying Master acc.to CiA-302.2
        /// </summary>
        public const Byte COP_k_FEATURE_FLYING_MASTER = 2;

        /// <summary>
        /// Additional firmware features for use with <c>COP_InitInterface</c>:
        /// Activate Multiple Masters based on CiA-302.2
        /// </summary>
        public const Byte COP_k_FEATURE_MULTI_MASTER = 6;

        /// <summary>
        /// Additional firmware features for use with <c>COP_InitInterface</c>:
        /// Activate low speed bus coupling.
        /// </summary>
        public const Byte COP_k_FEATURE_LOWSPEED = 17;

        /// <summary>
        /// Node monitoring feature to use with <c>COP_AddNode</c>:
        /// Use legacy Node guarding.
        /// </summary>
        public const Byte COP_k_NODE_GUARDING = 0;

        /// <summary>
        /// Node monitoring feature to use with <c>COP_AddNode</c>:
        /// Use Heartbeat.
        /// </summary>
        public const Byte COP_k_HEARTBEAT = 1;

        /// <summary>
        /// SDO channel to use with e.g. <c>COP_ReadSDO</c> and <c>COP_WriteSDO</c>:
        /// Use default Server SDO channel according to Predefined Connection Set.
        /// </summary>
        public const Byte COP_k_DEFAULT_SDO = 0x01;

        /// <summary>
        /// SDO channel to use with e.g. <c>COP_ReadSDO</c> and <c>COP_WriteSDO</c>:
        /// Use Server SDO channel that has been declared in <c>COP_CreateSDO</c>.
        /// </summary>
        public const Byte COP_k_USERDEFINED_SDO = 0x02;

        /// <summary>
        /// SDO mode for use with <c>COP_ReadSDO</c> and <c>COP_WriteSDO</c>.
        /// Use segmented transfer (regular).
        /// </summary>
        public const Byte COP_k_NO_BLOCKTRANSFER = 0x00;

        /// <summary>
        /// SDO mode for use with <c>COP_ReadSDO</c> and <c>COP_WriteSDO</c>.
        /// Use block transfer (advanced).
        /// </summary>
        public const Byte COP_k_BLOCKTRANSFER = 0x01;

        /// <summary>
        /// SDO access direction for use with <c>COP_ReadSDO</c> and <c>COP_WriteSDO</c>:
        /// Download SDO data. Write data to node.
        /// </summary>
        public const Byte COP_k_SDO_DOWNLOAD = 0x00;

        /// <summary>
        /// SDO access direction for use with <c>COP_ReadSDO</c> and <c>COP_WriteSDO</c>:
        /// Upload SDO data. Read data from node.
        /// </summary>
        public const Byte COP_k_SDO_UPLOAD = 0x01;

        /// <summary>
        /// PDO type (Master's point of view) for use with e.g. <c>COP_CreatePDO</c>:
        /// Receive PDO, RPDO. Node transmits, Master receives.
        /// </summary>
        public const Byte COP_k_PDO_TYP_RX = 0;

        /// <summary>
        /// PDO type (Master's point of view) for use with e.g. <c>COP_CreatePDO</c>:
        /// Transmit PDO, TPDO. Master transmits, node receives.
        /// </summary>
        public const Byte COP_k_PDO_TYP_TX = 1;

        /// <summary>
        /// PDO mode (Transmission Type) for use with <c>COP_CreatePDO</c>:
        /// Synchronous PDO updated with each SYNC object.
        /// </summary>
        public const Byte COP_k_PDO_MODE_SYNC = 1;

        /// <summary>
        /// PDO mode (Transmission Type) for use with <c>COP_CreatePDO</c>:
        /// Asynchronous PDO updated with each object value change.
        /// </summary>
        public const Byte COP_k_PDO_MODE_ASYNC = 254;

        /// <summary>
        /// Node state return code for use with e.g. <c>COP_GetNodeState</c>:
        /// Node is booting up and will enter pre-operational state in an instant.
        /// </summary>
        public const Byte COP_k_NS_BOOTUP = 0;

        /// <summary>
        /// Node state return code for use with e.g. <c>COP_GetNodeState</c>:
        /// A monitoring error with the node has been encountered.
        /// </summary>
        public const Byte COP_k_NS_DISCONNECTED = 1;

        /// <summary>
        /// Node state return code for use with e.g. <c>COP_GetNodeState</c>:
        /// Node is in stopped state.
        /// </summary>
        public const Byte COP_k_NS_STOPPED = 4;

        /// <summary>
        /// Node state return code for use with e.g. <c>COP_GetNodeState</c>:
        /// Node is in operational state.
        /// </summary>
        public const Byte COP_k_NS_OPERATIONAL = 5;

        /// <summary>
        /// Node state return code for use with e.g. <c>COP_GetNodeState</c>:
        /// Node is in stopped state.
        /// </summary>
        public const Byte COP_k_NS_PREOPERATIONAL = 127;

        /// <summary>
        /// Node state return code for use with e.g. <c>COP_GetNodeState</c>:
        /// Node state is unclear.
        /// </summary>
        public const Byte COP_k_NS_UNKNOWN = 255;

        /// <summary>
        /// TimeStamp control for use with <c>COP_StartStopTSObj</c>:
        /// Start cyclic transmission of timestamp objects.
        /// </summary>
        public const Byte COP_k_TS_START = 0;

        /// <summary>
        /// TimeStamp control for use with <c>COP_StartStopTSObj</c>:
        /// Stop cyclic transmission of timestamp objects.
        /// </summary>
        public const Byte COP_k_TS_STOP = 1;

        /// <summary>
        /// Mode for synchronisation object. (compatibility entry)
        /// </summary>
        public const Byte COP_k_BOTH_LINES = 2;

        /// <summary>
        /// Mode for synchronisation object for use with <c>COP_EnableSync</c> and <c>COP_DisableSync</c>:
        /// Command is applied to all lines.
        /// </summary>
        public const Byte COP_k_ALL_LINES = 2;

        /// <summary>
        /// Mode for synchronisation object for use with <c>COP_EnableSync</c> and <c>COP_DisableSync</c>:
        /// Command is applied to line addressed by boardhandle.
        /// </summary>
        public const Byte COP_k_SINGLE_LINE = 3;

        /// <summary>
        /// For use with <c>COP_ClockTick1ms</c>
        /// </summary>
        public enum COP_e_CLOCKSOURCE
        {
            /// <summary>
            /// Definition of clock tick source switch. Injects external clock tick signal.
            /// </summary>
            COP_CLOCKSOURCE_EXTERNAL,

            /// <summary>
            /// Definition of clock tick source switch. Reactivates internal clock generator.
            /// </summary>
            COP_CLOCKSOURCE_INTERNAL
        };

        /// <summary>
        /// Definitions of CCI queue numbers
        /// </summary>
        // PC to microcontroller CAN0
        public const Byte COP_P2M_QUEUE_COMMAND0 = 0;

        public const Byte COP_P2M_QUEUE_SDO0 = 1;

        public const Byte COP_P2M_QUEUE_PDO0 = 2;

        public const Byte COP_P2M_QUEUE_SETTIME0 = 3;

        // PC to microcontroller CAN1
        public const Byte COP_P2M_QUEUE_COMMAND1 = 4;

        public const Byte COP_P2M_QUEUE_SDO1 = 5;

        public const Byte COP_P2M_QUEUE_PDO1 = 6;

        public const Byte COP_P2M_QUEUE_SETTIME1 = 3;    // same value as COP_P2M_QUEUE_SETTIME0

        // PC to microcontroller CAN2
        public const Byte COP_P2M_QUEUE_COMMAND2 = 7;

        public const Byte COP_P2M_QUEUE_SDO2 = 8;

        public const Byte COP_P2M_QUEUE_PDO2 = 9;

        public const Byte COP_P2M_QUEUE_SETTIME2 = 3;    //  same value as COP_P2M_QUEUE_SETTIME0

        // PC to microcontroller CAN3
        public const Byte COP_P2M_QUEUE_COMMAND3 = 10;

        public const Byte COP_P2M_QUEUE_SDO3 = 11;

        public const Byte COP_P2M_QUEUE_PDO3 = 12;

        public const Byte COP_P2M_QUEUE_SETTIME3 = 3;    //  same value as COP_P2M_QUEUE_SETTIME0

        // microcontroller to PC CAN0
        public const Byte COP_M2P_QUEUE_COMMAND0 = 0;

        public const Byte COP_M2P_QUEUE_SDO0 = 1;

        public const Byte COP_M2P_QUEUE_PDO0 = 2;

        public const Byte COP_M2P_QUEUE_EMERGENCY0 = 3;

        public const Byte COP_M2P_QUEUE_EVENT0 = 4;

        public const Byte COP_M2P_QUEUE_SYNC0 = 5;

        // microcontroller to PC CAN1
        public const Byte COP_M2P_QUEUE_COMMAND1 = 6;

        public const Byte COP_M2P_QUEUE_SDO1 = 7;

        public const Byte COP_M2P_QUEUE_PDO1 = 8;

        public const Byte COP_M2P_QUEUE_EMERGENCY1 = 9;

        public const Byte COP_M2P_QUEUE_EVENT1 = 10;

        public const Byte COP_M2P_QUEUE_SYNC1 = 11;

        // microcontroller to PC CAN2
        public const Byte COP_M2P_QUEUE_COMMAND2 = 12;

        public const Byte COP_M2P_QUEUE_SDO2 = 13;

        public const Byte COP_M2P_QUEUE_PDO2 = 14;

        public const Byte COP_M2P_QUEUE_EMERGENCY2 = 15;

        public const Byte COP_M2P_QUEUE_EVENT2 = 16;

        public const Byte COP_M2P_QUEUE_SYNC2 = 17;

        // microcontroller to PC CAN3
        public const Byte COP_M2P_QUEUE_COMMAND3 = 18;

        public const Byte COP_M2P_QUEUE_SDO3 = 19;

        public const Byte COP_M2P_QUEUE_PDO3 = 20;

        public const Byte COP_M2P_QUEUE_EMERGENCY3 = 21;

        public const Byte COP_M2P_QUEUE_EVENT3 = 22;

        public const Byte COP_M2P_QUEUE_SYNC3 = 23;

        /// <summary>
        /// Definitions of command opcodes
        /// </summary>
        /// <remarks>
        /// <![CDATA[                         +-+---+                       ]]>
        /// <![CDATA[ Assembly of opcodes:    |f|fff|                       ]]>
        /// <![CDATA[                         +-+---+                       ]]>
        /// <![CDATA[                          |  |                         ]]>
        /// <![CDATA[                  +-------+  +------+                  ]]>
        /// <![CDATA[                  |                 |                  ]]>
        /// <![CDATA[                  V                 V                  ]]>
        /// <![CDATA[          module identifier | service opcode           ]]>
        /// <![CDATA[                                    |                  ]]>
        /// <![CDATA[                                    V                  ]]>
        /// <![CDATA[                       client       |     server       ]]>
        /// <![CDATA[                   -----------------+---------------   ]]>
        /// <![CDATA[                   request      ----|---> indication   ]]>
        /// <![CDATA[                   confirmation <---|---- response     ]]>
        /// <![CDATA[                                                       ]]>
        /// <![CDATA[                   base no + 0 -> request              ]]>
        /// <![CDATA[                   base no + 1 -> indication           ]]>
        /// <![CDATA[                   base no + 2 -> response             ]]>
        /// <![CDATA[                   base no + 3 -> confirmation         ]]>
        /// </remarks>
        // basic interface opcodes
        public const UInt16 COP_k_TESTCMD_REQ = 0x0000;

        public const UInt16 COP_k_TESTCMD_CON = 0x0003;

        public const UInt16 COP_k_STATUS_REQ = 0x0004;

        public const UInt16 COP_k_STATUS_CON = 0x0007;

        public const UInt16 COP_k_INIT_INTERFACE_REQ = 0x0008;

        public const UInt16 COP_k_INIT_INTERFACE_CON = 0x000b;

        public const UInt16 COP_k_FW_INFO_REQ = 0x000c;

        public const UInt16 COP_k_FW_INFO_CON = 0x000f;

        public const UInt16 COP_k_SHUTDOWN_REQ = 0x0010;

        public const UInt16 COP_k_SHUTDOWN_CON = 0x0013;

        public const UInt16 COP_k_SET_USERBITTIMING_REQ = 0x0014;

        public const UInt16 COP_k_SET_USERBITTIMING_CON = 0x0017;

        public const UInt16 COP_k_GET_INIT_INFO_REQ = 0x0018;  //  NEW7

        public const UInt16 COP_k_GET_INIT_INFO_CON = 0x001b;  //  NEW7

        // network management opcodes
        public const UInt16 COP_k_ADD_NODE_REQ = 0x1000;

        public const UInt16 COP_k_ADD_NODE_CON = 0x1003;

        public const UInt16 COP_k_SEARCH_NODE_REQ = 0x1004;

        public const UInt16 COP_k_SEARCH_NODE_CON = 0x1007;

        public const UInt16 COP_k_DELETE_NODE_REQ = 0x1008;

        public const UInt16 COP_k_DELETE_NODE_CON = 0x100b;

        public const UInt16 COP_k_SET_OPERATIONAL_REQ = 0x100c;

        public const UInt16 COP_k_SET_OPERATIONAL_CON = 0x100f;

        public const UInt16 COP_k_SET_PREOPERTNL_REQ = 0x1010;

        public const UInt16 COP_k_SET_PREOPERTNL_CON = 0x1013;

        public const UInt16 COP_k_SET_PREPARED_REQ = 0x1018;

        public const UInt16 COP_k_SET_PREPARED_CON = 0x101b;

        public const UInt16 COP_k_RESET_COMM_REQ = 0x101c;

        public const UInt16 COP_k_RESET_COMM_CON = 0x101f;

        public const UInt16 COP_k_RESET_NODE_REQ = 0x1020;

        public const UInt16 COP_k_RESET_NODE_CON = 0x1023;

        public const UInt16 COP_k_GET_NODE_STATE_REQ = 0x1024;

        public const UInt16 COP_k_GET_NODE_STATE_CON = 0x1027;

        public const UInt16 COP_k_CHANGE_NODE_PARAM_REQ = 0x102c;

        public const UInt16 COP_k_CHANGE_NODE_PARAM_CON = 0x102f;

        public const UInt16 COP_k_EVENT_IND = 0x1031;

        public const UInt16 COP_k_GET_NODE_INFO_REQ = 0x1034;  //  NEW6

        public const UInt16 COP_k_GET_NODE_INFO_CON = 0x1037;  //  NEW6

        public const UInt16 COP_k_CONFIG_FLY_MASTER_REQ = 0x1050;

        public const UInt16 COP_k_CONFIG_FLY_MASTER_CON = 0x1053;

        public const UInt16 COP_k_START_MASTER_NEG_REQ = 0x1054;

        public const UInt16 COP_k_START_MASTER_NEG_CON = 0x1057;

        public const UInt16 COP_k_GET_STATUS_MASTER_NEG_REQ = 0x1058;

        public const UInt16 COP_k_GET_STATUS_MASTER_NEG_CON = 0x105b;

        public const UInt16 COP_k_CONFIG_SDM_REQ = 0x105c;

        public const UInt16 COP_k_CONFIG_SDM_CON = 0x105f;

        public const UInt16 COP_k_START_SDM_REQ = 0x1060;

        public const UInt16 COP_k_START_SDM_CON = 0x1063;

        // data object management
        public const UInt16 COP_k_CREATE_PDO_REQ = 0x3000;

        public const UInt16 COP_k_CREATE_PDO_CON = 0x3003;

        public const UInt16 COP_k_DELETE_PDO_REQ = 0x3004;  //  NEW6

        public const UInt16 COP_k_DELETE_PDO_CON = 0x3007;  //  NEW6

        public const UInt16 COP_k_DEF_SYNCOBJ_REQ = 0x3008;

        public const UInt16 COP_k_DEF_SYNCOBJ_CON = 0x300b;

        public const UInt16 COP_k_GET_SYNC_INFO_REQ = 0x300c;  //  NEW6

        public const UInt16 COP_k_GET_SYNC_INFO_CON = 0x300f;  //  NEW6

        public const UInt16 COP_k_ENABLE_SYNC_REQ = 0x3014;

        public const UInt16 COP_k_ENABLE_SYNC_CON = 0x3017;

        public const UInt16 COP_k_DISABLE_SYNC_REQ = 0x3018;

        public const UInt16 COP_k_DISABLE_SYNC_CON = 0x301b;

        public const UInt16 COP_k_CREATE_SPDTMOBJ_REQ = 0x302c;

        public const UInt16 COP_k_CREATE_SPDTMOBJ_CON = 0x302f;

        public const UInt16 COP_k_SET_SPEEDTIME_REQ = 0x3030;

        public const UInt16 COP_k_SET_SPEEDTIME_CON = 0x3033;

        public const UInt16 COP_k_EN_DIS_SPDTMOBJ_REQ = 0x3034;

        public const UInt16 COP_k_EN_DIS_SPDTMOBJ_CON = 0x3037;

        public const UInt16 COP_k_EN_DIS_TS_OBJ_REQ = 0x303c;

        public const UInt16 COP_k_EN_DIS_TS_OBJ_CON = 0x303f;

        public const UInt16 COP_k_SET_SDO_TMOUT_REQ = 0x3040;

        public const UInt16 COP_k_SET_SDO_TMOUT_CON = 0x3043;

        public const UInt16 COP_k_CREATE_SDO_REQ = 0x3044;

        public const UInt16 COP_k_CREATE_SDO_CON = 0x3047;

        public const UInt16 COP_k_SET_SYNCDIVISOR_REQ = 0x3048;

        public const UInt16 COP_k_SET_SYNCDIVISOR_CON = 0x304b;

        public const UInt16 COP_k_GET_TS_OBJ_REQ = 0x3050;  //  NEW6

        public const UInt16 COP_k_GET_TS_OBJ_CON = 0x3053;  //  NEW6

        public const UInt16 COP_k_GET_PDO_INFO_REQ = 0x3054;  //  NEW6

        public const UInt16 COP_k_GET_PDO_INFO_CON = 0x3057;  //  NEW6

        public const UInt16 COP_k_GET_SDO_INFO_REQ = 0x3058;  //  NEW6

        public const UInt16 COP_k_GET_SDO_INFO_CON = 0x305b;  //  NEW6

        public const UInt16 COP_k_SET_EMCY_ID_REQ = 0x3060;  //  NEW6

        public const UInt16 COP_k_SET_EMCY_ID_CON = 0x3063;  //  NEW6

        // basic data communication opcodes
        public const UInt16 COP_k_READ_SDO_REQ = 0x2000;

        public const UInt16 COP_k_READ_SDO_CON = 0x2003;

        public const UInt16 COP_k_WRITE_SDO_REQ = 0x2004;

        public const UInt16 COP_k_WRITE_SDO_CON = 0x2007;

        public const UInt16 COP_k_BLOCKREAD_SDO_REQ = 0x2020;

        public const UInt16 COP_k_BLOCKREAD_SDO_CON = 0x2023;

        public const UInt16 COP_k_BLOCKWRITE_SDO_REQ = 0x2024;

        public const UInt16 COP_k_BLOCKWRITE_SDO_CON = 0x2027;

        public const UInt16 COP_k_CANCEL_SDO_REQ = 0x2028;

        public const UInt16 COP_k_CANCEL_SDO_CON = 0x202B;

        public const UInt16 COP_k_RX_PDO_IND = 0x2005;

        public const UInt16 COP_k_WRITE_PDO_REQ = 0x0000;  //  Dummy

        public const UInt16 COP_k_EMERGENCY_OBJ_IND = 0x2011;

        public const UInt16 COP_k_REQUEST_PDO_REQ = 0x2014;

        public const UInt16 COP_k_REQUEST_PDO_CON = 0x2017;

        // layer management opcodes
        public const UInt16 COP_k_REQ_LMT_INQUIRE_ADDRESS_MACRO = 0x4000;

        public const UInt16 COP_k_CON_LMT_INQUIRE_ADDRESS_MACRO = 0x4003;

        public const UInt16 COP_k_REQ_LMT_CONFIG_NODE_ID_MACRO = 0x4004;

        public const UInt16 COP_k_CON_LMT_CONFIG_NODE_ID_MACRO = 0x4007;

        public const UInt16 COP_k_REQ_LMT_CONFIG_BIT_TIMING_MACRO = 0x4008;

        public const UInt16 COP_k_CON_LMT_CONFIG_BIT_TIMING_MACRO = 0x400b;

        public const UInt16 COP_k_REQ_LMT_IDENTIFY_SLAVE_MACRO = 0x400c;

        public const UInt16 COP_k_CON_LMT_IDENTIFY_SLAVE_MACRO = 0x400f;

        // layer setting sevices opcodes (LSS)
        public const UInt16 COP_k_REQ_LSS_CONFIG_NODE_ID_MACRO = 0x4020;

        public const UInt16 COP_k_CON_LSS_CONFIG_NODE_ID_MACRO = 0x4023;

        public const UInt16 COP_k_REQ_LSS_CONFIG_BIT_TIMING_MACRO = 0x4024;

        public const UInt16 COP_k_CON_LSS_CONFIG_BIT_TIMING_MACRO = 0x4027;

        public const UInt16 COP_k_REQ_LSS_ACTIVATE_BIT_TIMING_MACRO = 0x4028;

        public const UInt16 COP_k_CON_LSS_ACTIVATE_BIT_TIMING_MACRO = 0x402b;

        public const UInt16 COP_k_REQ_LSS_IDENTIFY_SLAVE_MACRO = 0x402c;

        public const UInt16 COP_k_CON_LSS_IDENTIFY_SLAVE_MACRO = 0x402f;

        public const UInt16 COP_k_REQ_LSS_INQUIRE_ADDRESS_MACRO = 0x4030;

        public const UInt16 COP_k_CON_LSS_INQUIRE_ADDRESS_MACRO = 0x4033;

        public const UInt16 COP_k_REQ_LSS_INQUIRE_NODE_ID_MACRO = 0x4034;

        public const UInt16 COP_k_CON_LSS_INQUIRE_NODE_ID_MACRO = 0x4037;

        public const UInt16 COP_k_REQ_LSS_IDENTIFY_NON_CONFIG_SLAVE_MACRO = 0x4038;

        public const UInt16 COP_k_CON_LSS_IDENTIFY_NON_CONFIG_SLAVE_MACRO = 0x403b;

        public const UInt16 COP_k_REQ_LSS_SET_TIMEOUT = 0x403c;

        public const UInt16 COP_k_CON_LSS_SET_TIMEOUT = 0x403f;

        public const UInt16 COP_k_REQ_LSS_FASTSCAN = 0x4040;  //  NEW6

        public const UInt16 COP_k_CON_LSS_FASTSCAN = 0x4043;  //  NEW6

        #endregion

#pragma warning restore 1591    // Missing XML comment for publicly visible type or member

        #region copcmd Structures

        /// <summary>
        /// For use with <c>COP_ReadPDO_S</c>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct COP_t_RX_PDO
        {
            /// <summary>
            /// number of the node
            /// </summary>
            public Byte node_no;

            /// <summary>
            /// number of the PDO
            /// </summary>
            public Byte pdo_no;

            /// <summary>
            /// length of received data
            /// </summary>
            public Byte length;

            /// <summary>
            /// sync counter value upon reception
            /// </summary>
            public Byte SyncCounter;

            /// <summary>
            /// buffer for received data, must be 8 bytes size
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] a_data;
        };

        /// <summary>
        /// For use with <c>COP_WritePDO_S</c>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct COP_t_TX_PDO
        {
            /// <summary>
            /// number of the node
            /// </summary>
            public Byte node_no;

            /// <summary>
            /// number of the PDO
            /// </summary>
            public Byte pdo_no;

            /// <summary>
            /// data to transmit, must be 8 bytes size
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] a_data;
        };

        /// <summary>
        /// For use with <c>COP_GetEmergencyObj_S</c>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct COP_t_EMERGENCY_OBJ
        {
            /// <summary>
            /// standardised error code of emergency object (Byte 0 and 1 of EMCY message).
            /// </summary>
            public UInt16 err_value;

            /// <summary>
            /// error register of emergency object (Byte 2 of EMCY message).
            /// corresponds to OD entry [1001sub0]
            /// </summary>
            public Byte err_reg;

            /// <summary>
            /// manufacturer specific error data of emergency object (5 bytes).
            /// (Byte 3 to 7 of EMCY message)
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public Byte[] err_data;

            /// <summary>
            /// number of the sending node
            /// </summary>
            public Byte node_no;
        };

        #endregion

        #region cop Structures

        /// <summary>
        /// For use with <c>COP_GetBoardInfo</c>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct COP_BOARD_INFO
        {
            /// <summary>
            /// Hardware version e.g.: 01.00 --> HEX value: 0x0100
            /// </summary>
            public UInt16 hw_version;

            /// <summary>
            /// Firmware version
            /// </summary>
            public UInt16 fw_version;

            /// <summary>
            /// CANopen Master API version
            /// </summary>
            public UInt16 sw_version;

            /// <summary>
            /// Memory segment of board
            /// </summary>
            public UInt32 board_seg;

            /// <summary>
            /// IRQ of board
            /// </summary>
            public UInt16 irq_num;

            /// <summary>
            /// Number of CAN lines
            /// </summary>
            public UInt16 canlines;

            /// <summary>
            /// string e.g.: "HW0123456789"
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string serial_num;

            /// <summary>
            /// string e.g.: "iPCI320_PCI"
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
            public string str_hw_type;
        };

        #endregion

#if NETCOREAPP2_0
#pragma warning disable CA1401  // P/Invoke method '' should not be visible
#endif

        #region cop Export Function Delegates

        ///*************************************************************************
        /// <summary>
        /// Prototype for user callbackfunctions for signalization of the receive
        /// queues. <para>
        /// RxPDO: que_num = COP_M2P_QUEUE_PDO0 or COP_M2P_QUEUE_PDO1 </para><para>
        /// Emergency: que_num = COP_M2P_QUEUE_EMERGENCY0 or COP_M2P_QUEUE_EMERGENCY1 </para><para>
        /// Network/Status Event: que_num = COP_M2P_QUEUE_EVENT0 or COP_M2P_QUEUE_EVENT1 </para><para>
        /// Sync message: que_num = COP_M2P_QUEUE_SYNC0 or COP_M2P_QUEUE_SYNC1 </para>
        /// </summary>
        /// <param name="boardhdl">
        /// handle of signaling board+line
        /// </param>
        /// <param name="que_num">
        /// queue identifier
        /// </param>
        /// <param name="canline">
        /// absolute CAN line number (0..3)
        /// </param>

        public delegate void COP_t_EventCallback([In] UInt16 boardhdl
                                                , [In] Byte que_num
                                                , [In] Byte canline);

        ///*************************************************************************
        /// <summary>
        /// Assign event callback functions to the different receive queues.
        /// </summary>
        /// <remarks>
        /// If a function shouldn't be called, use NULL as parameter.
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="fp_rx_pdo">
        /// this function will be called when a RxPDO was received
        /// (que_num = COP_M2P_QUEUE_PDO0 or COP_M2P_QUEUE_PDO1)
        /// </param>
        /// <param name="fp_emergency">
        /// this function will be called when a emergency message was received
        /// (que_num = COP_M2P_QUEUE_EMERGENCY0 or COP_M2P_QUEUE_EMERGENCY1)
        /// </param>
        /// <param name="fp_net_event">
        /// this function will be called when a network event occurs
        /// (que_num = COP_M2P_QUEUE_EVENT0 or COP_M2P_QUEUE_EVENT1)
        /// </param>
        /// <param name="fp_sync">
        /// this function will be called when a synchronisation message was sent
        /// (que_num = COP_M2P_QUEUE_SYNC0 or COP_M2P_QUEUE_SYNC1)
        /// </param>
        /// <returns><para>
        /// BER_k_OK              : success </para><para>
        /// BER_k_ERR             : error </para><para>
        /// BER_k_BADCALLBACK_PTR : Callback function incorrect </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DefineCallbacks([In] UInt16 boardhdl
                                                      , [In] COP_t_EventCallback fp_rx_pdo
                                                      , [In] COP_t_EventCallback fp_emergency
                                                      , [In] COP_t_EventCallback fp_net_event
                                                      , [In] COP_t_EventCallback fp_sync);

#if !NETCOREAPP2_0
        ///*************************************************************************
        /// <summary>
        /// Assign user defined windows messages or thread messages to the different
        /// receive queues. <para> When an object is received by a particular queue,
        /// you have the option to get a windows message, a thread message or both. </para><para>
        /// If a receive queue shouldn't be handled (no more), use 0 as hWnd or idThread
        /// argument value. </para><para>
        /// There's a separate function for each receive queue. When an change is
        /// detected, WINAPI function PostMessage() resp. PostThreadMessage() will
        /// be called by MasterAPI DLL. The event message carries the boardhandle as
        /// wParam. The event message carries the queue number as lParam: </para><para>
        /// (COP_M2P_QUEUE_PDO0 or COP_M2P_QUEUE_PDO1) </para><para>
        /// (COP_M2P_QUEUE_EVENT0 or COP_M2P_QUEUE_EVENT1) </para><para>
        /// (COP_M2P_QUEUE_EMERGENCY0 or COP_M2P_QUEUE_EMERGENCY1) </para><para>
        /// (COP_M2P_QUEUE_SYNC0 or COP_M2P_QUEUE_SYNC1) </para><para>
        /// The message values must be above WM_USER. </para>
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="hWnd">
        /// window handle of client application
        /// </param>
        /// <param name="idThread">
        /// thread id of sink thread
        /// </param>
        /// <param name="idThread">
        /// message identifier <para>
        /// COP_DefineMsgRPDO - this message will be posted when a RxPDO was received </para><para>
        /// COP_DefineMsgEvent - this message will be posted when a network event occurs </para><para>
        /// COP_DefineMsgEmergency - this message will be posted when an emergency message was received </para><para>
        /// COP_DefineMsgSync - this message will be posted when a synchronisation message was transmitted </para>
        /// </param>
        /// <returns><para>
        /// COP_k_IV        : invalid parameter </para><para>
        /// BER_k_OK        : success </para><para>
        /// BER_k_ERR       : error </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DefineMsgRPDO([In] UInt16 boardhdl
                                                      , [In] IntPtr hWnd
                                                      , [In] UInt32 idThread
                                                      , [In] UInt32 Msg);

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DefineMsgEvent([In] UInt16 boardhdl
                                                      , [In] IntPtr hWnd
                                                      , [In] UInt32 idThread
                                                      , [In] UInt32 Msg);

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DefineMsgEmergency([In] UInt16 boardhdl
                                                      , [In] IntPtr hWnd
                                                      , [In] UInt32 idThread
                                                      , [In] UInt32 Msg);

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DefineMsgSync([In] UInt16 boardhdl
                                                      , [In] IntPtr hWnd
                                                      , [In] UInt32 idThread
                                                      , [In] UInt32 Msg);

#endif

        ///*************************************************************************
        /// <summary>
        /// Return the Thread Identifiers of the internal queue poll threads. This
        /// identifier can be used by the application to gain access to the thread
        /// e.g. using Windows API function OpenThread()
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="pPdoThreadId">
        /// identifier of the PDO queues poller
        /// (COP_M2P_QUEUE_PDO0 and COP_M2P_QUEUE_PDO1)
        /// </param>
        /// <param name="pEmcyThreadId">
        /// identifier of the Emergency queues poller
        /// (COP_M2P_QUEUE_EMERGENCY0 and COP_M2P_QUEUE_EMERGENCY1)
        /// </param>
        /// <param name="pEventThreadId">
        /// identifier of the network event queues poller
        /// (COP_M2P_QUEUE_EVENT0 and COP_M2P_QUEUE_EVENT1)
        /// </param>
        /// <param name="pSyncThreadId">
        /// identifier of the synchronisation message queues poller
        /// (COP_M2P_QUEUE_SYNC0 and COP_M2P_QUEUE_SYNC1)
        /// </param>
        /// <returns><para>
        /// BER_k_OK          : success </para><para>
        /// BER_k_ERR         : invalid boardhandle </para><para>
        /// BER_k_CCI_INST_ERR: board hasn't been initialised correctly </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetThreadIds([In] UInt16 boardhdl
                                                    , [Out] out UInt32 pPdoThreadId
                                                    , [Out] out UInt32 pEmcyThreadId
                                                    , [Out] out UInt32 pEventThreadId
                                                    , [Out] out UInt32 pSyncThreadId);

        ///*************************************************************************
        /// <summary>
        /// Inject the main clock tick.
        /// </summary>
        /// <remarks>
        /// CANopen firmware relies on a periodical millisecond clock tick.
        /// Typically, it is generated internally, but it can be replaced by an
        /// externally generated clock tick in case a high precision (hardware)
        /// clock signal is both available and necessary for the application. <para>
        /// The clock tick is required to generate the Sync object, to handle
        /// node monitoring, and to measure SDO and LSS timeout. </para><para>
        /// Calling this function each millisecond serves as a replacement
        /// clock tick for CANopen firmware. </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="clocksource">
        /// Switch between internal and external clock tick. <para>
        /// COP_CLOCKSOURCE_EXTERNAL shuts down the internal clock generator
        /// when called for the first time.
        /// Subsequently, COP_ClockTick1ms(COP_CLOCKSOURCE_EXTERNAL) needs to be
        /// called periodically by the application as long as it is running. </para><para>
        /// COP_CLOCKSOURCE_INTERNAL reactivates the internal clock generator so
        /// COP_ClockTick1ms() doesn't need to be called periodically any longer. </para>
        /// </param>
        /// <returns><para>
        /// BER_k_OK            : success </para><para>
        /// BER_k_ERR           : invalid boardhandle </para><para>
        /// BER_k_CCI_INST_ERR  : board hasn't been initialised correctly </para><para>
        /// BER_k_BOARD_NOT_SUPP: not supported, firmware clock tick can not
        ///                       be replaced for particular CAN board </para><para>
        /// COP_k_IV            : invalid clock source switch </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ClockTick1ms([In] UInt16 boardhdl
                                                    , [In] COP_e_CLOCKSOURCE clocksource);

        ///*************************************************************************
        /// <summary>
        /// Completely reset the DLL.
        /// </summary>
        /// <remarks>
        /// This is useful for programming with interpreters such as Visual Basic.
        /// If you stop debugging inside interpreted code, the automatic cleanup
        /// won't be called. So you have to use COP_Reset_DLL to perform a post
        /// clean up.
        /// Internally, COP_ReleaseBoard() is being called.
        /// </remarks>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern void COP_Reset_DLL();

        ///*************************************************************************
        /// <summary>
        /// Initialize CAN board, COP firmware, and select CAN line to use.
        /// </summary>
        /// <remarks>
        /// When you feed COP_DEFAULTBOARD to pBoardtype, the default board
        /// according to IXXAT control panel applet will be used. <para>
        /// When you feed COP_BOARDDIALOG to pBoardtype, the IXXAT hardware
        /// selection dialog will open up on Windows. </para><para>
        /// You may feed COP_1stBOARD, COP_2ndBOARD... to pBoardID in order
        /// to select the n-th instance of a specific board type. </para>
        /// </remarks>
        /// <param name="pBoardhdl">
        /// pointer to handle of CAN board/line
        /// </param>
        /// <param name="pBoardtype">
        /// Type of CAN board <para>
        /// (ECI)   DeviceClass acc.to vciguid.h </para><para>
        /// (VCI3)  DeviceClass acc.to vciguid.h </para><para>
        /// (VCI2)  The type acc.to XatBrds.h shall be in pBoardtype->Data1 </para>
        /// </param>
        /// <param name="pBoardID">
        /// Unique global identifier of board <para>
        /// (ECI)   ECI_HW_INFO.szHwSerial as GUID </para><para>
        /// (VCI3)  UniqueHardwareId.AsGuid </para><para>
        /// (VCI2)  Former Regkey shall be in pBoardID->Data1 </para>
        /// </param>
        /// <param name="canLine">
        /// Number of the CAN line to use: <para>
        /// COP_FIRSTLINE - default (first CAN line) </para><para>
        /// COP_SECONDLINE - second CAN line </para><para>
        /// COP_THIRDLINE - third CAN line </para><para>
        /// COP_FOURTHLINE - fourth CAN line </para><para>
        /// COP_SINGLELINE - default (first CAN line), and no need for further CAN
        ///                  lines. Utilising faster alternative firmware. </para>
        /// </param>
        /// <returns><para>
        /// BER_k_OK                     : Success </para><para>
        /// BER_k_ERR                    : General error </para><para>
        /// BER_k_BOARD_ALREADY_USED     : Board is already in use </para><para>
        /// BER_k_ALL_BOARDS_USED        : No more free board slot in DLL </para><para>
        /// BER_k_CANNOT_SEARCH_BOARD    : IXXAT Hardware selection Dialog
        ///                                cancelled by user </para><para>
        /// BER_k_BOARD_NOT_FOUND        : Given pBoardtype and pRegkey didn't
        ///                                match any local CAN board </para><para>
        /// BER_k_BOARD_NOT_SUPP         : Local Boardtype isn't capable of
        ///                                running CANopen firmware </para><para>
        /// BER_k_WRONG_FW               : Wrong firmware version or initial
        ///                                communication with firmware failed </para><para>
        /// BER_k_USED_FROM_OTHER_PROCESS: Board is in use by another CAN application </para><para>
        /// BER_k_PC_MC_COMM_ERR         : Communication between PC and CAN board failed </para><para>
        /// BER_k_BOARD_DLD_ERR          : An error occured during firmware download </para><para>
        /// BER_k_NO_SUCH_CANLINE        : CANline is not available or not supported </para><para>
        /// BER_k_CANLINE_USED           : CANline is already in use </para><para>
        /// BER_k_VCI_INST_ERR           : IXXAT CAN driver is missing </para><para>
        /// BER_k_BOARD_ERR              : Unknown board type or can't locate board type </para><para>
        /// BER_k_CCI_INST_ERR           : CCI installation error (internal) </para><para>
        /// BER_k_SDO_INST_ERR           : SDO handler installation error (internal) </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_InitBoard([Out] out UInt16 pBoardhdl
                                                , [In][Out] ref Guid pBoardtype
                                                , [In][Out] ref Guid pBoardID
                                                , [In] Int32 canLine);

        ///*************************************************************************
        /// <summary>
        /// Free resources for a board inside the DLL
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern void COP_ReleaseBoard([In] UInt16 boardhdl);

        //************************************************************************
        //
        //    Function      : COP_SendMsg
        //
        //    Description   : Place request message in transmit command queue
        //
        //    Parameter     : boardhdl   (in) : handle of CAN board
        //                    sp_message (in) : msg to transmit
        //
        //    Returnvalues  : BER_k_OK        : message sent
        //                    BER_k_ERR       : boardhandle not valid
        //                    BER_k_NOT_SENT  : message couldn't be handed over
        //
        //************************************************************************
        /* Todo

        [DllImport(CANopenMasterAPIlib)]
        public static extern Int16 COP_SendMsg( [In] UInt16          boardhdl
                                              , [In] COP_t_Message*  sp_message );
        */

        //************************************************************************
        //
        //    Function      : COP_GetMsg
        //
        //    Description   : Get response message from receive command queue
        //
        //    Parameter     : boardhdl   (in) : handle of CAN board
        //                    sp_message (out): buffer for msg
        //
        //    Returnvalues  : BER_k_OK        : message retrieved
        //                    BER_k_ERR       : boardhandle not valid
        //                    BER_k_TIMEOUT   : no confirmation received
        //                    BER_k_PC_MC_COMM_ERR:
        //                                      Communication error PC to MC
        //                    BER_k_DATA_CORRUPT:
        //                                      Sequence number incorrect
        //
        //************************************************************************
        /* Todo

        [DllImport(CANopenMasterAPIlib)]
        public static extern Int16 COP_GetMsg( [In]      UInt16          boardhdl
                                             , [Out] out COP_t_Message*  sp_message);
        */

        ///*************************************************************************
        /// <summary>
        /// Intialize the CANopen-Master firmware
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// COP_k_BAUD_CIA (standard) or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB,
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB
        /// </param>
        /// <param name="node_no">
        /// node number of the master (0: feature not used)
        /// </param>
        /// <param name="hbTime">
        /// heartbeat time for the master
        /// </param>
        /// <param name="addFeatures">
        /// Flagfield to switch several additional features in firmware,
        /// default value is COP_k_NO_FEATURES
        /// </param>
        /// <returns><para>
        /// BER_k_ERR                  : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT             : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT              : no confirmation received </para><para>
        /// BER_k_CANLINE_USED         : CANline is already initialised </para><para>
        /// COP_k_OK                   : success </para><para>
        /// COP_k_NOT_INIT             : CAN Controller could not be started.
        ///                              For SocketCAN, make sure the CAN board is up
        ///                              and is configured as e.g.
        ///                              <c>sudo ip link set can0 up type can bitrate 250000</c>
        /// COP_k_CAL_ERR              : CAL-Error </para><para>
        /// COP_k_IV                   : Invalid parameter </para><para>
        /// COP_k_NO_FLY_MASTER_PRESENT: Flying Master not supported </para><para>
        /// COP_k_NO_LOWSPEED          : No LowSpeed bus-coupling present or supported </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_InitInterface([In] UInt16 boardhdl
                                                    , [In] Byte baudtable
                                                    , [In] Byte baudrate
                                                    , [In] Byte node_no
                                                    , [In] UInt16 hbTime
                                                    , [In] UInt16 addFeatures);

        ///*************************************************************************
        /// <summary>
        /// Function to test the communication between Master API DLL
        /// and Master API firmware
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT    : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT     : no confirmation received </para><para>
        /// BER_k_DATA_CORRUPT: corrupt data received from firmware </para><para>
        /// COP_k_OK          : communication ok </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_TestCommand([In] UInt16 boardhdl);

        ///*************************************************************************
        /// <summary>
        /// Returns the state of the CANopen Master API firmware
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="state_master">
        /// state of master
        /// </param>
        /// <param name="state_err_dll">
        /// state of master firmware data link layer
        /// </param>
        /// <returns><para>
        /// BER_k_OK      : success </para><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetStatus([In] UInt16 boardhdl
                                                , [Out] out Byte state_master
                                                , [Out] out Byte state_err_dll);

        ///*************************************************************************
        /// <summary>
        /// Change the timeout for response messages from command queue in
        /// milliseconds.
        /// </summary>
        /// <remarks>
        /// When attempting to retrieve a message using <c>COP_GetMsg</c> this
        /// timeout value determines how long to wait. <c>COP_GetMsg</c> is used
        /// internally in nearly all CANopen Master API functions.
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="w_timeout">
        /// new timeoutvalue in milliseconds (lowest possible value 55 ms)
        /// </param>
        /// <returns><para>
        /// BER_k_OK  : success </para><para>
        /// BER_k_ERR : wrong board handle </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_SetCommTimeOut([In] UInt16 boardhdl
                                                     , [In] UInt16 w_timeout);

        ///*************************************************************************
        /// <summary>
        /// Returns information about HW and SW
        /// </summary>
        /// <remarks>
        /// COP_BOARD_INFO:
        /// <list type="bullet">
        /// <item><description> hardware version of board. </description></item>
        /// <item><description> version of board firmware. </description></item>
        /// <item><description> PC software version. </description></item>
        /// <item><description> memory segment of board (legacy).
        ///                     A value of 0x100 signals usage of VCI3 generic firmware. </description></item>
        /// <item><description> IRQ of board (legacy). </description></item>
        /// <item><description> number of CAN controllers. </description></item>
        /// <item><description> serial number of board e.g.: "HW123456" (16 characters). </description></item>
        /// <item><description> HW identification e.g. "USB-to-CAN compact" (40 characters). </description></item>
        /// </list>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="sp_info">
        /// pointer to information record
        /// </param>
        /// <returns><para>
        /// BER_k_OK  : success </para><para>
        /// BER_k_ERR : error </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetBoardInfo([In] UInt16 boardhdl
                                                   , [Out] out COP_BOARD_INFO sp_info);

        ///*************************************************************************
        /// <summary>
        /// Create a new PDO
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node (1 .. 127)
        /// </param>
        /// <param name="pdo_no">
        /// number of the PDO, 1-based
        /// </param>
        /// <param name="type">
        /// type of PDO (direction) (COP_k_PDO_TYP_RX, .._TX)
        /// from Master's point of view, i.e. Node TX = Master RX
        /// </param>
        /// <param name="mode">
        /// transmission mode of PDO (COP_k_PDO_MODE_SYNC, .._ASYNC)
        /// (0 .. 255)
        /// 0..240: synchronous
        /// 254:    asynchronous
        /// </param>
        /// <param name="length">
        /// datalength of PDO (0 .. 8)
        /// </param>
        /// <param name="canid">
        /// CANID of PDO
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT    : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT     : no confirmation received </para><para>
        /// COP_k_OK          : success </para><para>
        /// COP_k_CAL_ERR     : CAL-Error </para><para>
        /// COP_k_CANID_IN_USE: CAN-Identifier already in use </para><para>
        /// COP_k_IV          : invalid parameter </para><para>
        /// COP_k_NOT_FOUND   : node is undeclared </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_CreatePDO([In] UInt16 boardhdl
                                                , [In] Byte node_no
                                                , [In] Byte pdo_no
                                                , [In] Byte type
                                                , [In] Byte mode
                                                , [In] Byte length
                                                , [In] UInt16 canid);

        ///*************************************************************************
        /// <summary>
        /// Delete a PDO
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node (1 .. 127)
        /// </param>
        /// <param name="pdo_no">
        /// number of the PDO, 1-based
        /// </param>
        /// <param name="type">
        /// type of PDO (direction) (COP_k_PDO_TYP_RX, .._TX) from Master's point of view
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_IV        : invalid parameter </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DeletePDO([In] UInt16 boardhdl
                                                , [In] Byte node_no
                                                , [In] Byte pdo_no
                                                , [In] Byte type);

        ///*************************************************************************
        /// <summary>
        /// Deliver attributes of a PDO
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node (1 .. 127)
        /// </param>
        /// <param name="pdo_no">
        /// number of the PDO, 1-based
        /// </param>
        /// <param name="type">
        /// type of PDO (direction) (COP_k_PDO_TYP_RX, .._TX) from Master's point of view
        /// </param>
        /// <param name="mode">
        /// transmission mode of PDO (COP_k_PDO_MODE_SYNC, .._ASYNC)
        /// (0 .. 255)
        /// 0..240: synchronous
        /// 254:    asynchronous
        /// </param>
        /// <param name="length">
        /// datalength of PDO (0 .. 8)
        /// </param>
        /// <param name="canid">
        /// CANID of PDO
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_IV        : invalid parameter </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetPDOInfo([In] UInt16 boardhdl
                                                  , [In] Byte node_no
                                                  , [In] Byte pdo_no
                                                  , [In] Byte type
                                                  , [Out] out Byte mode
                                                  , [Out] out Byte length
                                                  , [Out] out UInt16 canid);

        ///*************************************************************************
        /// <summary>
        /// Create a new SDO channel (direct communication link to a node)
        /// </summary>
        /// <remarks>
        /// The Server-SDO channels of the Predefined Connection Set exist
        /// by default, so only additional SDO channels must be created.
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="sdo_no">
        /// number of the SDO channel, always = COP_k_USERDEFINED_SDO
        /// </param>
        /// <param name="clientcanid">
        /// CANID for SDO request (Master is client)
        /// </param>
        /// <param name="servercanid">
        /// CANID for SDO response (Node is server)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT    : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT     : no confirmation received </para><para>
        /// COP_k_OK          : success </para><para>
        /// COP_k_CAL_ERR     : CAL-Error </para><para>
        /// COP_k_CANID_IN_USE: CAN-Identifier already in use </para><para>
        /// COP_k_IV          : invalid parameter </para><para>
        /// COP_k_NOT_FOUND   : node is undeclared </para><para>
        /// COP_k_SDO_RUNNING : SDO transfer in progress, retry later </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_CreateSDO([In] UInt16 boardhdl
                                                , [In] Byte node_no
                                                , [In] Byte sdo_no
                                                , [In] UInt16 clientcanid
                                                , [In] UInt16 servercanid);

        ///*************************************************************************
        /// <summary>
        /// Deliver attributes of a SDO channel
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="sdo_no">
        /// number of the SDO channel: COP_k_DEFAULT_SDO or COP_k_USERDEFINED_SDO
        /// </param>
        /// <param name="clientcanid">
        /// CANID for SDO request (Master is client)
        /// </param>
        /// <param name="servercanid">
        /// CANID for SDO response (Node is server)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_IV        : invalid parameter </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetSDOInfo([In] UInt16 boardhdl
                                                  , [In] Byte node_no
                                                  , [In] Byte sdo_no
                                                  , [Out] out UInt16 clientcanid
                                                  , [Out] out UInt16 servercanid);

        ///*************************************************************************
        /// <summary>
        /// Initialize the synchronisation object of the CAN board
        /// </summary>
        /// <remarks>
        /// <![CDATA[ --++-------------+---------++-------------+--->t ]]>
        /// <![CDATA[   || sync window |         || sync window |      ]]>
        /// <![CDATA[   ||-------------|         ||-------------|      ]]>
        /// <![CDATA[   || divisor * sync period ||                    ]]>
        /// <![CDATA[   ||-----------------------||                    ]]>
        /// Please note that all CAN lines of a board share the same <c>sync_period</c>.
        /// Thus, for different sync intervals on the lines, a so-called divisor
        /// must be set for each line (see also <c>COP_SetSyncDivisor</c>).
        /// Hence, the sync_period given here is the greatest common divisor gcd
        /// of all lines' sync intervals. <para>
        /// Contrary to the <c>sync_period</c>, the <c>counteroverflow</c> value is
        /// individual to each CAN line. </para><para>
        /// Calling this function turns OFF a possibly already running sync
        /// object of the CAN line. </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="sync_period">
        /// base interval of synchronisation message (in ms)
        /// (2 .. 65280)
        /// default value is 1000
        /// </param>
        /// <param name="sync_window">
        /// width of the synchronisation window (in ms)
        /// (2 .. sync_period)
        /// Reserved for future use
        /// </param>
        /// <param name="counteroverflow">
        /// sync counter overflow value of the CAN line, acc.to. [1019sub0]
        /// Value range: 0; (2 .. 240)
        /// default value is 0
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_CAL_ERR : CAL-Error </para><para>
        /// COP_k_IV      : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DefSyncObj([In] UInt16 boardhdl
                                                 , [In] UInt16 sync_period
                                                 , [In] UInt16 sync_window
                                                 , [In] Byte counteroverflow);

        ///*************************************************************************
        /// <summary>
        /// Deliver attributes of the synchronisation object of the CAN line
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="sync_period">
        /// base interval of synchronisation message (in ms)
        /// </param>
        /// <param name="sync_window">
        /// width of the synchronisation window (in ms)
        /// </param>
        /// <param name="counteroverflow">
        /// sync counter overflow value, acc.to. [1019sub0]
        /// </param>
        /// <param name="divisor">
        /// Factor of common sync_period for the individual CAN line
        /// (see also <c>COP_SetSyncDivisor</c>)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetSyncInfo([In] UInt16 boardhdl
                                                  , [Out] out UInt16 sync_period
                                                  , [Out] out UInt16 sync_window
                                                  , [Out] out Byte counteroverflow
                                                  , [Out] out Byte divisor);

        ///*************************************************************************
        /// <summary>
        /// Define a divisor for the synchronisation objects' frequency.
        /// </summary>
        /// <remarks>
        /// Because all CAN lines on one board share the same sync_period as
        /// defined in <c>COP_DefSyncObj</c>, the sync divisor is useful to
        /// generate several sync intervals on the different CAN lines.
        /// Given a sync_period of e.g. 10ms, a divisor 10 on CAN line 0 would
        /// trigger the sync object every 100 ms, whereas as divisor of 3 on CAN
        /// line 1 would trigger the sync object in a 30 ms interval on CAN line 1.
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="divisor">
        /// Factor of common sync_period for the individual CAN line.
        /// default value is 1
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_CAL_ERR : CAL-Error </para><para>
        /// COP_k_IV      : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_SetSyncDivisor([In] UInt16 boardhdl
                                                     , [In] Byte divisor);

        ///*************************************************************************
        /// <summary>
        /// Enable cyclic transmission of synchronization objects of the CAN board
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="mode">
        /// operating modes for non-single CAN configurations:
        /// COP_k_SINGLE_LINE / COP_k_ALL_LINES
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_CAL_ERR : CAL-Error </para><para>
        /// COP_k_IV      : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_EnableSync([In] UInt16 boardhdl
                                                 , [In] Byte mode);

        ///*************************************************************************
        /// <summary>
        /// Disable cyclic transmission of synchronization objects of the CAN board
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="mode">
        /// operating modes for non-single CAN configurations:
        /// COP_k_SINGLE_LINE / COP_k_ALL_LINES
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_CAL_ERR : CAL-Error </para><para>
        /// COP_k_IV      : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DisableSync([In] UInt16 boardhdl
                                                  , [In] Byte mode);

        ///*************************************************************************
        /// <summary>
        /// Initialize the timestamp object of the CAN board
        /// </summary>
        /// <remarks>
        /// The time basis is the same for all CAN lines
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="ms">
        /// ms after midnight
        /// </param>
        /// <param name="days">
        /// days from 1. January  1984
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_IV      : invalid time </para><para>
        /// COP_k_BSY     : Queue is full </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_InitTimeStampObj([In] UInt16 boardhdl
                                                       , [In] UInt32 ms
                                                       , [In] UInt16 days);

        ///*************************************************************************
        /// <summary>
        /// Deliver attributes and current value of the timestamp object of the CAN board
        /// </summary>
        /// <remarks>
        /// The time basis is the same for all CAN lines
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="startstop">
        /// COP_k_TS_START or COP_k_TS_STOP
        /// </param>
        /// <param name="cycle">
        /// cycle time of transmission in ms
        /// </param>
        /// <param name="ms">
        /// ms after midnight.
        /// Value is 0 if <c>COP_InitTimeStampObj</c> had not been called yet
        /// </param>
        /// <param name="days">
        /// days from 1. January 1984.
        /// Value is 0 if <c>COP_InitTimeStampObj</c> had not been called yet
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetTimeStampObj([In] UInt16 boardhdl
                                                      , [Out] out Byte startstop
                                                      , [Out] out UInt16 cycle
                                                      , [Out] out UInt32 ms
                                                      , [Out] out UInt16 days);

        ///*************************************************************************
        /// <summary>
        /// Start or stop cyclic transmission of timestamp objects
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="startstop">
        /// COP_k_TS_START or COP_k_TS_STOP
        /// </param>
        /// <param name="cycle">
        /// cycle time of transmission in ms
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_CAL_ERR : CAL-Error </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_StartStopTSObj([In] UInt16 boardhdl
                                                     , [In] Byte startstop
                                                     , [In] UInt16 cycle);

        ///*************************************************************************
        /// <summary>
        /// Change the SDO timeout value. Value in milliseconds.
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="w_timeout">
        /// new timeout in ms.
        /// default value is 200
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_CAL_ERR : CAL-Error </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_SetSDOTimeOut([In] UInt16 boardhdl
                                                    , [In] UInt16 w_timeout);

        ///*************************************************************************
        /// <summary>
        /// Declare a new node
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of new node
        /// </param>
        /// <param name="NgOrHb">
        /// node guarding or heartbeat:
        /// COP_k_NODE_GUARDING or COP_k_HEARTBEAT
        /// </param>
        /// <param name="GuardHeartbeatTime">
        /// time between two guard requests in ms  resp.
        /// time between two heartbeat transmissions in ms
        /// </param>
        /// <param name="lifetimefactor">
        /// only for node guarding: how many guard reqests may
        /// remain unanswered without error indication
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT    : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT     : no confirmation received </para><para>
        /// COP_k_OK          : success </para><para>
        /// COP_k_CAL_ERR     : CAL-Error </para><para>
        /// COP_k_CANID_IN_USE: CAN-Identifier already in use </para><para>
        /// COP_k_IV          : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_AddNode([In] UInt16 boardhdl
                                              , [In] Byte node_no
                                              , [In] Byte NgOrHb
                                              , [In] UInt16 GuardHeartbeatTime
                                              , [In] Byte lifetimefactor);

        ///*************************************************************************
        /// <summary>
        /// Removes a node from the masters network management
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node to be removed
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_DeleteNode([In] UInt16 boardhdl
                                                 , [In] Byte node_no);

        ///*************************************************************************
        /// <summary>
        /// Import a CANopen electronic data sheet that describes a node.
        /// Its PDO, EMCY and Monitoring information will be extracted and used
        /// to configure CANopen Master API.
        /// This way the application no longer needs to call <c>COP_CreatePDO</c>,
        /// <c>COP_SetEmcyIdentifier</c> etc.
        /// Any existing node configuration will be overwritten, but not cleared
        /// in advance.
        /// </summary>
        /// <remarks>
        /// A number of file formats is accepted: CANopen .EDS and .DCF files
        /// (WIN.INI text format), and CANopen binary concise device configuration
        /// (.CDC) files. <para>
        /// CANopen XML device description is not supported. </para><para>
        /// A fully automated network configuration in one step can be done if a
        /// .CDC file which is including embedded node CDC data is imported under
        /// the <c>node_no</c> of the Master API itself as given in
        /// <c>COP_InitInterface</c>. </para><para>
        /// EDS/DCF import is handled by means of external library EDSLIB4.dll
        /// on Windows. </para><para>
        /// In case of an error while configuring the EMCY or PDO or Monitoring
        /// objects of CANopen Master API, the function will not fail. It might
        /// rather return the faulty OD address in idx and subidx. </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node to be described
        /// </param>
        /// <param name="filename">
        /// full absolute filename and path of a device description file.
        /// The file format is determined by its file extension: EDS, DCF resp CDC
        /// </param>
        /// <param name="rules">
        /// bit coded rules to control the behavior of the function.
        /// Reserved for future use, shall be 0
        /// </param>
        /// <param name="idx">
        /// in case of a configuration error, the OD entry index that broke the
        /// import may be delivered in this optional argument
        /// </param>
        /// <param name="subidx">
        /// in case of a configuration error, the OD entry subindex that broke the
        /// import may be delivered in this optional argument
        /// </param>
        /// <returns><para>
        /// BER_k_ERR             : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT        : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT         : no confirmation received </para><para>
        /// BER_k_EDS_FILENOTFOUND: given device description file not found </para><para>
        /// BER_k_EDS_CORRUPT     : device description file import failed </para><para>
        /// BER_k_EDSLIB          : EDSLIB4.dll is missing </para><para>
        /// COP_k_OK              : success </para><para>
        /// COP_k_IV              : invalid parameter (e.g. file not found) </para><para>
        /// COP_k_NOT_INIT        : Master not initialised </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib, CharSet = CharSet.Unicode)]
        public static extern Int16 COP_ImportEDS([In] UInt16 boardhdl
                                                , [In] Byte node_no
                                                , [In] string filename
                                                , [In] UInt32 rules
                                                , [Out] out UInt16 idx
                                                , [Out] out Byte subidx);

        ///*************************************************************************
        /// <summary>
        /// Check whether a declared node is present in network
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node to be searched
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT    : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT     : no confirmation received </para><para>
        /// COP_k_OK          : success </para><para>
        /// COP_k_CAL_ERR     : CAL-Error </para><para>
        /// COP_k_IV          : invalid parameter </para><para>
        /// COP_k_NOT_FOUND   : node is undeclared </para><para>
        /// COP_k_BSY         : SDO engine is currently working to capacity </para><para>
        /// COP_k_SDO_RUNNING : Default SDO channel to given node
        ///                     is already in use by another transfer </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_SearchNode([In] UInt16 boardhdl
                                                 , [In] Byte node_no);

        ///*************************************************************************
        /// <summary>
        /// Change attributes of a node already declared with <c>COP_AddNode</c>  resp.
        /// Change heartbeat time of the Master
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node (1 .. 127)
        /// </param>
        /// <param name="NgOrHb">
        /// node guarding or heartbeat:
        /// COP_k_NODE_GUARDING or COP_k_HEARTBEAT
        /// </param>
        /// <param name="GuardHeartbeatTime">
        /// time between two guard requests in ms  resp.
        /// time between two heartbeat transmissions in ms
        /// </param>
        /// <param name="lifetimefactor">
        /// only for node guarding: how many guard reqests may
        /// remain unanswered without error indication
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_IV        : invalid parameter </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ChangeNodeParameter([In] UInt16 boardhdl
                                                          , [In] Byte node_no
                                                          , [In] Byte NgOrHb
                                                          , [In] UInt16 GuardHeartbeatTime
                                                          , [In] Byte lifetimefactor);

        ///*************************************************************************
        /// <summary>
        /// Deliver attributes of a node already declared with <c>COP_AddNode</c>
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node (1..127)
        /// </param>
        /// <param name="NgOrHb">
        /// node guarding or heartbeat:
        /// COP_k_NODE_GUARDING or COP_k_HEARTBEAT
        /// </param>
        /// <param name="GuardHeartbeatTime">
        /// time between two guard requests in ms  resp.
        /// time between two heartbeat transmissions in ms
        /// </param>
        /// <param name="lifetimefactor">
        /// only for node guarding: how many guard requests may
        /// remain unanswered without error indication
        /// </param>
        /// <param name="EmcyIdentifier">
        /// CAN Identifier of Emergency object
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_IV        : invalid parameter </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetNodeInfo([In] UInt16 boardhdl
                                                  , [In] Byte node_no
                                                  , [Out] out Byte NgOrHb
                                                  , [Out] out UInt16 GuardHeartbeatTime
                                                  , [Out] out Byte lifetimefactor
                                                  , [Out] out UInt16 EmcyIdentifier);

        ///*************************************************************************
        /// <summary>
        /// Configure the CAN identifier used by a node for its Emergency object
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node (1 .. 127)
        /// </param>
        /// <param name="EmcyIdentifier">
        /// CAN Identifier of Emergency object
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT    : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT     : no confirmation received </para><para>
        /// COP_k_OK          : success </para><para>
        /// COP_k_IV          : invalid parameter </para><para>
        /// COP_k_CAL_ERR     : CAL-Error </para><para>
        /// COP_k_CANID_IN_USE: CAN-Identifier already in use </para><para>
        /// COP_k_NOT_FOUND   : node is undeclared </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_SetEmcyIdentifier([In] UInt16 boardhdl
                                                        , [In] Byte node_no
                                                        , [In] UInt16 EmcyIdentifier);

        ///*************************************************************************
        /// <summary>
        /// Start one or all nodes
        /// </summary>
        /// <remarks>
        /// NMT command 'Start Remote Node'
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the target node (0 = entire network)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para><para>
        /// COP_k_IV        : invalid nodeID </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_StartNode([In] UInt16 boardhdl
                                                , [In] Byte node_no);

        ///*************************************************************************
        /// <summary>
        /// Stop one or all nodes
        /// </summary>
        /// <remarks>
        /// NMT command 'Stop Remote Node'
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the target node (0 = entire network)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para><para>
        /// COP_k_IV        : invalid nodeID </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_StopNode([In] UInt16 boardhdl
                                               , [In] Byte node_no);

        ///*************************************************************************
        /// <summary>
        /// Change the state of the node(s) to Pre-Operational
        /// </summary>
        /// <remarks>
        /// NMT command 'Enter Pre-Operational'
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the target node (0 = entire network)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para><para>
        /// COP_k_IV        : invalid nodeID </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_EnterPreOperational([In] UInt16 boardhdl
                                                          , [In] Byte node_no);

        ///*************************************************************************
        /// <summary>
        /// Reset communication profile of a node
        /// </summary>
        /// <remarks>
        /// NMT command 'Reset Communication'
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the target node (0 = entire network)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para><para>
        /// COP_k_IV        : invalid nodeID </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ResetComm([In] UInt16 boardhdl
                                                , [In] Byte node_no);

        ///*************************************************************************
        /// <summary>
        /// Reset application and communication profile of a node
        /// </summary>
        /// <remarks>
        /// NMT command 'Reset Node'
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the target node (0 = entire network)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_NOT_FOUND : node is undeclared </para><para>
        /// COP_k_IV        : invalid nodeID </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ResetNode([In] UInt16 boardhdl
                                                , [In] Byte node_no);

        ///*************************************************************************
        /// <summary>
        /// Return the state of a node
        /// </summary>
        /// <remarks>
        /// Possible node states are: <para>
        /// COP_k_NS_BOOTUP </para><para>
        /// COP_k_NS_DISCONNECTED </para><para>
        /// COP_k_NS_STOPPED </para><para>
        /// COP_k_NS_OPERATIONAL </para><para>
        /// COP_k_NS_PREOPERATIONAL </para><para>
        /// COP_k_NS_UNKNOWN </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="node_state">
        /// state of the node
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_CAL_ERR   : CAL-Error </para><para>
        /// COP_k_IV        : invalid nodeID </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetNodeState([In] UInt16 boardhdl
                                                  , [In] Byte node_no
                                                  , [Out] out UInt16 node_state);

        ///*************************************************************************
        /// <summary>
        /// Fetch an event from the event queue
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="evt_type">
        /// kind of event. <para>
        /// COP_k_NMT_EVT: network event </para><para>
        /// COP_k_DLL_EVT: data link layer event </para><para>
        /// COP_k_WPDO_EVT: COP_WritePDO() event </para><para>
        /// COP_k_RPDO_EVT: RxPDO-queue event </para><para>
        /// COP_k_QUEUE_OVRUN_EVT: queue overrun </para><para>
        /// COP_k_FLY_EVT: Flying Master event </para><para>
        /// COP_k_RESOURCE_EVT: Resources limitation event </para>
        /// </param>
        /// <param name="evt_data1">
        /// depending on <c>evt_type</c>.<para>
        /// COP_k_NMT_EVT -> event cause; one of COP_k_NMT_aaaa </para><para>
        /// COP_k_DLL_EVT -> current status; set of COP_k_DLL_aaaa </para><para>
        /// COP_k_WPDO_EVT, COP_k_RPDO_EVT -> event cause; one of COP_k_ERR_PDO_aaaa </para><para>
        /// COP_k_QUEUE_OVRUN_EVT -> EMCY overrun count </para><para>
        /// COP_k_FLY_EVENT -> event cause; one of COP_k_FLY_aaaa </para><para>
        /// COP_k_RESOURCE_EVT -> event cause; one of COP_k_RESOURCE_aaaa </para>
        /// </param>
        /// <param name="evt_data2">
        /// depending on <c>evt_type</c>.<para>
        /// COP_k_NMT_EVT -> node id </para><para>
        /// OP_k_WPDO_EVT, COP_k_RPDO_EVT -> node id of request </para><para>
        /// COP_k_QUEUE_OVRUN_EVT -> RxPDO overrun count </para><para>
        /// COP_k_RESOURCE_EVENT -> resource description </para>
        /// </param>
        /// <param name="evt_data3">
        /// depending on <c>evt_type</c>.<para>
        /// COP_k_NMT_EVT -> actual node state </para><para>
        /// COP_k_WPDO_EVT, COP_k_RPDO_EVT -> pdo number of request </para><para>
        /// COP_k_QUEUE_OVRUN_EVT -> Event overrun count </para><para>
        /// COP_k_RESOURCE_EVENT -> resource description </para>
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_OK          : success </para><para>
        /// COP_k_QUEUE_EMPTY : no entry in queue </para><para>
        /// COP_k_CAL_ERR     : General failure on CCI access </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetEvent([In] UInt16 boardhdl
                                               , [Out] out Byte evt_type
                                               , [Out] out Byte evt_data1
                                               , [Out] out Byte evt_data2
                                               , [Out] out Byte evt_data3);

        ///*************************************************************************
        /// <summary>
        /// Request a PDO transmission from a node.
        /// </summary>
        /// <remarks>
        /// The PDO must be read with <c>COP_ReadPDO</c>.
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="pdo_no">
        /// number of the PDO
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT   : no confirmation received </para><para>
        /// COP_k_OK        : success </para><para>
        /// COP_k_BSY       : Transmit queue for CAN is full </para><para>
        /// COP_k_CAL_ERR   : CAL-Error </para><para>
        /// COP_k_IV        : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_RequestPDO([In] UInt16 boardhdl
                                                 , [In] Byte node_no
                                                 , [In] Byte pdo_no);

        ///*************************************************************************
        /// <summary>
        /// Get a PDO entry from the PDO receive queue in separate parameters
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="pdo_no">
        /// number of the PDO
        /// </param>
        /// <param name="rxlen">
        /// length of received data
        /// </param>
        /// <param name="rxdata">
        /// buffer for received data, must be 8 bytes size
        /// </param>
        /// <param name="SyncCounter">
        /// sync counter value upon reception
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_OK          : success </para><para>
        /// COP_k_QUEUE_EMPTY : No Objects in queue </para><para>
        /// COP_k_IV          : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ReadPDO([In] UInt16 boardhdl
                                              , [Out] out Byte node_no
                                              , [Out] out Byte pdo_no
                                              , [Out] out Byte rxlen
                                              , [Out] Byte[] rxdata
                                              , [Out] out Byte SyncCounter);

        ///*************************************************************************
        /// <summary>
        /// Get a PDO entry from the PDO receive queue as a single record
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="sp_pdo">
        /// received PDO
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_OK          : success </para><para>
        /// COP_k_QUEUE_EMPTY : No Objects in queue </para><para>
        /// COP_k_IV          : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ReadPDO_S([In] UInt16 boardhdl
                                                , [Out] out COP_t_RX_PDO sp_pdo);

        ///*************************************************************************
        /// <summary>
        /// Fetch an emergency object from the emergency queue in separate parameters
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the sending node
        /// </param>
        /// <param name="err_value">
        /// standardised error code of emergency object
        /// </param>
        /// <param name="err_register">
        /// error register of emergency object
        /// </param>
        /// <param name="err_data">
        /// manufacturer specific error data of emergency object (5 bytes)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_OK          : success </para><para>
        /// COP_k_QUEUE_EMPTY : No emergency objects in queue </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetEmergencyObj([In] UInt16 boardhdl
                                                      , [Out] out Byte node_no
                                                      , [Out] out UInt16 err_value
                                                      , [Out] out Byte err_register
                                                      , [Out] Byte[] err_data);

        ///*************************************************************************
        /// <summary>
        /// Fetch an emergency object from the EMCY queue as a single record
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="sp_emergency">
        /// received emergency object
        /// </param>
        /// <returns><para>
        /// BER_k_ERR         : boardhandle not valid </para><para>
        /// BER_k_OK          : success </para><para>
        /// COP_k_QUEUE_EMPTY : No emergency objects in queue </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetEmergencyObj_S([In] UInt16 boardhdl
                                                        , [Out] out COP_t_EMERGENCY_OBJ sp_emergency);

        ///*************************************************************************
        /// <summary>
        /// Check whether firmware has signaled a sync
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="SyncCounter">
        /// sync counter value
        /// </param>
        /// <returns><para>
        /// BER_k_ERR           : boardhandle not valid </para><para>
        /// BER_k_PC_MC_COMM_ERR: Communication error PC to MC </para><para>
        /// COP_k_OK            : sync signaled </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_CheckSync([In] UInt16 boardhdl
                                                , [Out] out Byte SyncCounter);

        ///*************************************************************************
        /// <summary>
        /// Place a PDO entry in the PDO transmit queue in separate parameters
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="pdo_no">
        /// number of the PDO
        /// </param>
        /// <param name="txdata">
        /// data to transmit, must be 8 bytes size
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_OK      : success </para><para>
        /// COP_k_IV      : invalid txdata </para><para>
        /// COP_k_BSY     : Queue is full </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_WritePDO([In] UInt16 boardhdl
                                               , [In] Byte node_no
                                               , [In] Byte pdo_no
                                               , [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)]
                                             [In] Byte[] txdata);

        ///*************************************************************************
        /// <summary>
        /// Place a PDO entry in the PDO transmit queue as a single record
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="sp_pdo">
        /// PDO to transmit
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT  : command msg couldn't be handed over </para><para>
        /// BER_k_OK        : success </para><para>
        /// COP_k_IV        : invalid txdata buffer </para><para>
        /// COP_k_BSY       : Queue is full </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_WritePDO_S([In] UInt16 boardhdl
                                                 , [In] COP_t_TX_PDO sp_pdo);

        ///*************************************************************************
        /// <summary>
        /// Initiate and execute a SDO upload from a node.
        /// </summary>
        /// <remarks>
        /// In case given buffer size rxlen is insufficient, the buffer will be
        /// filled up to it's capacity limit, and the total number of necessary
        /// bytes will be returned in rxlen.
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="sdo_no">
        /// number of the SDO channel: COP_k_DEFAULT_SDO or COP_k_USERDEFINED_SDO.
        /// Shall the user defined SDO channel be used, it must already have been
        /// created with <c>COP_CreateSDO</c>.
        /// </param>
        /// <param name="mode">
        /// COP_k_NO_BLOCKTRANSFER or COP_k_BLOCKTRANSFER
        /// </param>
        /// <param name="idx">
        /// index in node's OD
        /// </param>
        /// <param name="subidx">
        /// subindex in node's OD
        /// </param>
        /// <param name="rxlen">
        /// IN: size of buffer for received data.
        /// OUT: number of received data bytes.
        /// </param>
        /// <param name="rxdata">
        /// received data (max 2^32 bytes)
        /// </param>
        /// <param name="abortcode">
        /// abort code of SDO-transfer
        /// </param>
        /// <returns><para>
        /// BER_k_ERR           : boardhandle not valid </para><para>
        /// BER_k_MEM_ALLOC_ERR : internal data couldn't be created </para><para>
        /// BER_k_SDO_THREAD_ERR: Thread execution cancelled </para><para>
        /// BER_k_OK            : success </para><para>
        /// BER_k_PC_MC_COMM_ERR: Communication error PC to MC </para><para>
        /// BER_k_TIMEOUT       : Firmware did not take on the SDO
        ///                       task within communication timeout </para><para>
        /// COP_k_IV            : invalid parameter </para><para>
        /// COP_k_NOT_FOUND     : node is undeclared </para><para>
        /// COP_k_TIMEOUT       : SDO response timeout expired </para><para>
        /// COP_k_ABORT         : SDO transfer aborted </para><para>
        /// COP_k_BSY           : SDO transfer discarded
        ///                       (Transmit queue of the CAN is full) </para><para>
        /// COP_k_QUEUE_EMPTY   : No SDO response from firmware </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ReadSDO([In] UInt16 boardhdl
                                              , [In] Byte node_no
                                              , [In] Byte sdo_no
                                              , [In] Byte mode
                                              , [In] UInt16 idx
                                              , [In] Byte subidx
                                              , [In][Out] ref UInt32 rxlen
                                              , [Out] Byte[] rxdata
                                              , [Out] out UInt32 abortcode);

        ///*************************************************************************
        /// <summary>
        /// Initiate and execute a SDO download to a node.
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="sdo_no">
        /// number of the SDO channel: COP_k_DEFAULT_SDO or COP_k_USERDEFINED_SDO.
        /// Shall the user defined SDO chanel be used, it must already have been
        /// created with <c>COP_CreateSDO</c>.
        /// </param>
        /// <param name="mode">
        /// COP_k_NO_BLOCKTRANSFER or COP_k_BLOCKTRANSFER
        /// </param>
        /// <param name="idx">
        /// index in node's OD
        /// </param>
        /// <param name="subidx">
        /// subindex in node's OD
        /// </param>
        /// <param name="txlen">
        /// length of data to be transmitted
        /// </param>
        /// <param name="txdata">
        /// transmit data (max 2^32 bytes)
        /// </param>
        /// <param name="abortcode">
        /// abort code of SDO-transfer
        /// </param>
        /// <returns><para>
        /// BER_k_ERR           : boardhandle not valid </para><para>
        /// BER_k_MEM_ALLOC_ERR:  internal data couldn't be created </para><para>
        /// BER_k_SDO_THREAD_ERR: Thread execution cancelled </para><para>
        /// BER_k_OK            : success </para><para>
        /// BER_k_PC_MC_COMM_ERR: Communication error PC to MC </para><para>
        /// BER_k_TIMEOUT       : Firmware did not take on the SDO
        ///                       task within communication timeout </para><para>
        /// COP_k_IV            : invalid parameter </para><para>
        /// COP_k_NOT_FOUND     : node is undeclared </para><para>
        /// COP_k_TIMEOUT       : SDO response timeout expired </para><para>
        /// COP_k_ABORT         : SDO transfer aborted </para><para>
        /// COP_k_BSY           : SDO transfer discarded
        ///                       (Transmit queue of the CAN is full) </para><para>
        /// COP_k_QUEUE_EMPTY   : No SDO response from firmware </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_WriteSDO([In] UInt16 boardhdl
                                               , [In] Byte node_no
                                               , [In] Byte sdo_no
                                               , [In] Byte mode
                                               , [In] UInt16 idx
                                               , [In] Byte subidx
                                               , [In] UInt32 txlen
                                               , [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6)]
                                             [In]      Byte[] txdata
                                               , [Out] out UInt32 abortcode);

        ///*************************************************************************
        /// <summary>
        /// Initiate and execute a SDO-transfer (download or upload) with a node
        /// </summary>
        /// <remarks>
        /// It's the asynchronous, i.e. non blocking, way to do an SDO transfer.
        /// To retrieve the response and result of the transfer, call <c>COP_GetSDO</c>
        /// This is a legacy function.
        /// To effectively make use of the parallel SDO engine, call sibling function
        /// <c>COP_ParallelPutSDO</c>.
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="sdo_no">
        /// number of the SDO channel: COP_k_DEFAULT_SDO or COP_k_USERDEFINED_SDO.
        /// Shall the user defined SDO channel be used, it must already have been
        /// created with <c>COP_CreateSDO</c>.
        /// </param>
        /// <param name="mode">
        /// COP_k_NO_BLOCKTRANSFER or COP_k_BLOCKTRANSFER
        /// </param>
        /// <param name="rwAccess">
        /// COP_k_SDO_DOWNLOAD or COP_k_SDO_UPLOAD
        /// </param>
        /// <param name="idx">
        /// index in node's OD
        /// </param>
        /// <param name="subidx">
        /// subindex in node's OD
        /// </param>
        /// <param name="length">
        /// length of SDO data
        /// </param>
        /// <param name="data">
        /// SDO data (max 2^32 bytes)
        /// </param>
        /// <param name="h_Event">
        /// Event to signal completion of the SDO-transfer. Caller can wait for
        /// the event or poll <c>COP_GetSDO</c>.
        /// Optional argument, can be set to NULL.
        /// On Windows, it is expected to be a so-called event (WinBase::CreateEvent).
        /// On Linux, it is expected to be a COP_t_SDO_EVENT*
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_OK        : success </para><para>
        /// COP_k_IV        : invalid parameter </para><para>
        /// BER_k_TIMEOUT   : Firmware did not take on the SDO task
        ///                   within communication timeout </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_PutSDO([In] UInt16 boardhdl
                                             , [In] Byte node_no
                                             , [In] Byte sdo_no
                                             , [In] Byte mode
                                             , [In] Byte rwAccess
                                             , [In] UInt16 idx
                                             , [In] Byte subidx
                                             , [In] UInt32 length
                                             , [In] Byte[] data
                                             , [In] IntPtr h_Event);

        ///*************************************************************************
        /// <summary>
        /// Read the data/abort code of a SDO transfer started using <c>COP_PutSDO</c>
        /// </summary>
        /// <remarks>
        /// The calling process and thread must be the same as in the belonging
        /// <c>COP_PutSDO</c> call
        /// This is a legacy function.
        /// To effectively make use of the parallel SDO engine, call sibling function
        /// <c>COP_ParallelGetSDO</c>.
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="length">
        /// IN: size of buffer for received data.
        /// OUT: number of received data bytes.
        /// </param>
        /// <param name="data">
        /// received data
        /// </param>
        /// <param name="abortcode">
        /// abort code of SDO-transfer
        /// </param>
        /// <returns><para>
        /// BER_k_ERR           : boardhandle not valid </para><para>
        /// BER_k_OK            : success </para><para>
        /// BER_k_TIMEOUT       : timeout in communication PC to MC </para><para>
        /// BER_k_SDO_THREAD_ERR: Thread execution cancelled </para><para>
        /// BER_k_DATA_CORRUPT  : Corrupt data detected MC to PC,
        ///                       SDO job still pending </para><para>
        /// BER_k_PC_MC_COMM_ERR: Communication error PC to MC </para><para>
        /// COP_k_IV            : invalid parameter </para><para>
        /// COP_k_NOT_FOUND     : node is undeclared </para><para>
        /// COP_k_SDO_RUNNING   : SDO transfer currently running, the approximate
        ///                       progress in permille is included in <c>length</c> </para><para>
        /// COP_k_TIMEOUT       : SDO response timeout expired </para><para>
        /// COP_k_ABORT         : SDO transfer aborted </para><para>
        /// COP_k_BSY           : SDO transfer discarded
        ///                       (Transmit queue of the CAN is full) </para><para>
        /// COP_k_QUEUE_EMPTY   : No data available </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetSDO([In] UInt16 boardhdl
                                             , [In][Out] ref UInt32 length
                                             , [Out] Byte[] data
                                             , [Out] out UInt32 abortcode);

        ///*************************************************************************
        /// <summary>
        /// Initiate and execute a SDO-transfer (download or upload) with a node.
        /// </summary>
        /// <remarks>
        /// It's the asynchronous, i.e. non blocking, way to do an SDO transfer.
        /// To retrieve the response and result of the transfer, call
        /// <c>COP_ParallelGetSDO</c>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="cookie">
        /// Unique identifier of the SDO-transfer
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="sdo_no">
        /// number of the SDO channel: COP_k_DEFAULT_SDO or COP_k_USERDEFINED_SDO.
        /// Shall the user defined SDO channel be used, it must already have been
        /// created with <c>COP_CreateSDO</c>.
        /// </param>
        /// <param name="mode">
        /// COP_k_NO_BLOCKTRANSFER or COP_k_BLOCKTRANSFER
        /// </param>
        /// <param name="rwAccess">
        /// COP_k_SDO_DOWNLOAD or COP_k_SDO_UPLOAD
        /// </param>
        /// <param name="idx">
        /// index in node's OD
        /// </param>
        /// <param name="subidx">
        /// subindex in node's OD
        /// </param>
        /// <param name="length">
        /// length of SDO data
        /// </param>
        /// <param name="data">
        /// SDO data (max 2^32 bytes)
        /// </param>
        /// <param name="h_Event">
        /// Event to signal completion of the SDO-transfer. Caller can wait for
        /// the event or poll <c>COP_ParallelGetSDO</c>.
        /// Optional argument, can be set to NULL.
        /// On Windows, it is expected to be a so-called event (WinBase::CreateEvent).
        /// On Linux, it is expected to be a COP_t_SDO_EVENT*
        /// </param>
        /// <returns><para>
        /// BER_k_ERR       : boardhandle not valid </para><para>
        /// BER_k_OK        : success </para><para>
        /// COP_k_IV        : invalid parameter </para><para>
        /// BER_k_TIMEOUT   : Firmware did not take on the SDO task
        ///                   within communication timeout </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ParallelPutSDO([In] UInt16 boardhdl
                                                     , [Out] out UInt32 cookie
                                                     , [In] Byte node_no
                                                     , [In] Byte sdo_no
                                                     , [In] Byte mode
                                                     , [In] Byte rwAccess
                                                     , [In] UInt16 idx
                                                     , [In] Byte subidx
                                                     , [In] UInt32 length
                                                     , [In] Byte[] data
                                                     , [In] IntPtr h_Event);

        ///*************************************************************************
        /// <summary>
        /// Read the data/abort code of a SDO transfer started using
        /// <c>COP_ParallelPutSDO</c>
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="cookie">
        /// Unique identifier of the SDO-transfer as returned by <c>COP_ParallelPutSDO</c>
        /// </param>
        /// <param name="length">
        /// IN: size of buffer for received data.
        /// OUT: number of received data bytes.
        /// </param>
        /// <param name="data">
        /// received data
        /// </param>
        /// <param name="abortcode">
        /// abort code of SDO-transfer
        /// </param>
        /// <returns><para>
        /// BER_k_ERR           : boardhandle not valid </para><para>
        /// BER_k_OK            : success </para><para>
        /// BER_k_TIMEOUT       : timeout in communication PC to MC </para><para>
        /// BER_k_SDO_THREAD_ERR: Thread execution cancelled </para><para>
        /// BER_k_DATA_CORRUPT  : Corrupt data detected MC to PC,
        ///                       SDO job still pending </para><para>
        /// BER_k_PC_MC_COMM_ERR: Communication error PC to MC </para><para>
        /// COP_k_IV            : invalid parameter </para><para>
        /// COP_k_NOT_FOUND     : node is undeclared </para><para>
        /// COP_k_SDO_RUNNING   : SDO transfer currently running, the approximate
        ///                       progress in permille is included in <c>length</c> </para><para>
        /// COP_k_TIMEOUT       : SDO response timeout expired </para><para>
        /// COP_k_ABORT         : SDO transfer aborted </para><para>
        /// COP_k_BSY           : SDO transfer discarded
        ///                       (Transmit queue of the CAN is full) </para><para>
        /// COP_k_QUEUE_EMPTY   : No data available </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ParallelGetSDO([In] UInt16 boardhdl
                                                      , [In] UInt32 cookie
                                                      , [In][Out] ref UInt32 length
                                                      , [Out] Byte[] data
                                                      , [Out] out UInt32 abortcode);

        ///*************************************************************************
        /// <summary>
        /// Cancel a running SDO transfer with a node
        /// </summary>
        /// <remarks>
        /// This function only applies when <c>COP_PutSDO</c> resp
        /// <c>COP_ParallelPutSDO</c> has been utilized for SDO access.
        /// This function will not work with <c>COP_ReadSDO</c> nor <c>COP_WriteSDO</c>.
        /// <para> The particular value pair <c>node_no=0</c> and <c>sdo_no=0</c>
        /// is used to cancel all currently running SDO transfers, and thus reset
        /// the SDO engine. </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="node_no">
        /// number of the node
        /// </param>
        /// <param name="sdo_no">
        /// number of the SDO channel: COP_k_DEFAULT_SDO or COP_k_USERDEFINED_SDO.
        /// Shall the user defined SDO channel be used, it must already have been
        /// created with <c>COP_CreateSDO</c>.
        /// </param>
        /// <param name="idx">
        /// index in node's OD (currently unused)
        /// </param>
        /// <param name="subidx">
        /// subindex in node's OD (currently unused)
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// BER_k_OK      : success </para><para>
        /// COP_k_IV      : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_CancelSDO([In] UInt16 boardhdl
                                                , [In] Byte node_no
                                                , [In] Byte sdo_no
                                                , [In] UInt16 idx
                                                , [In] Byte subidx);

        ///*************************************************************************
        /// <summary>
        /// Deliver the manufacturer name, the product name and
        /// the serial number of the connected device.
        /// </summary>
        /// <remarks>
        /// Attn: There must be only one device connected. <para>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. </para><para>
        /// The internal protocol sequence is as follows: </para><para>
        /// - SwitchModeGlobal() </para><para>
        /// - InquireManufacturerName() </para><para>
        /// - InquireProductName() </para><para>
        /// - InquireSerialNumber() </para><para>
        /// - SwitchModeGlobal() </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration (CiA timing table):
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB,
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if COP_InitInterface() hasn't been called yet.
        /// </param>
        /// <param name="sz_mname">
        /// manufacturer name string. 7 bytes and terminating '0'.
        /// </param>
        /// <param name="sz_pname">
        /// product name string. 7 bytes and terminating '0'.
        /// </param>
        /// <param name="sz_sno">
        /// serial number string. 14 bytes and terminating '0'.
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid <para></para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_NO      : general error </para><para>
        /// COP_k_IV      : invalid parameter </para><para>
        /// COP_k_TIMEOUT : node not present </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib, CharSet = CharSet.Ansi)]
        public static extern Int16 COP_LMT_GetAddress([In] UInt16 boardhdl
                                                     , [In] Byte baudrate
                                                     , [MarshalAs(UnmanagedType.LPArray , SizeConst = 8)]
                                                   [Out] out char[] sz_mname
                                                     , [MarshalAs(UnmanagedType.LPArray , SizeConst = 8)]
                                                   [Out] out char[] sz_pname
                                                     , [MarshalAs(UnmanagedType.LPArray , SizeConst = 15)]
                                                   [Out] out char[] sz_sno);

        ///*************************************************************************
        /// <summary>
        /// Reconfigure a present node in the network
        /// </summary>
        /// <remarks>
        /// Parameter <c>access_baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// - SwitchModeSelective(sz_mname, sz_pname, sz_sno) </para><para>
        /// - ConfigureModuleID(new_node_no) </para><para>
        /// - ConfigureBitTimingParameters(new_baudrate) </para><para>
        /// - StoreConfiguration() </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="sz_mname">
        /// manufacturer name string (7 chars)
        /// </param>
        /// <param name="sz_pname">
        /// product name string (7 chars)
        /// </param>
        /// <param name="sz_sno">
        /// serial number string (14 chars)
        /// </param>
        /// <param name="new_node_no">
        /// new node number
        /// </param>
        /// <param name="access_baudrate">
        /// baudrate to access the node for configuration (CiA timing table).
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// (for values see parameter new_baudrate)
        /// </param>
        /// <param name="new_baudrate">
        /// new baudrate for operation after configuration (CiA timing table):
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_NO      : general error </para><para>
        /// COP_k_IV      : invalid parameter </para><para>
        /// COP_k_TIMEOUT : node not present </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib, CharSet = CharSet.Ansi)]
        public static extern Int16 COP_LMT_ConfigNode([In] UInt16 boardhdl
                                                     , [MarshalAs(UnmanagedType.LPArray, SizeConst = 7)]
                                                   [In] char[] sz_mname
                                                     , [MarshalAs(UnmanagedType.LPArray, SizeConst = 7)]
                                                   [In] char[] sz_pname
                                                     , [MarshalAs(UnmanagedType.LPArray, SizeConst = 14)]
                                                   [In] char[] sz_sno
                                                     , [In] Byte new_node_no
                                                     , [In] UInt16 access_baudrate
                                                     , [In] UInt16 new_baudrate);

        ///*************************************************************************
        /// <summary>
        /// Configure the NodeID of a present node in the network
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// - SwitchModeSelective(sz_mname, sz_pname, sz_sno) </para><para>
        /// - ConfigureModuleID(new_node_no) </para><para>
        /// - StoreConfiguration() </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration (CiA timing table):
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="sz_mname">
        /// manufacturer name string (7 chars)
        /// </param>
        /// <param name="sz_pname">
        /// product name string (7 chars)
        /// </param>
        /// <param name="sz_sno">
        /// serial number string (14 chars)
        /// </param>
        /// <param name="new_node_no">
        /// new node number
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_NO      : general error </para><para>
        /// COP_k_IV      : invalid parameter </para><para>
        /// COP_k_TIMEOUT : node not present </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib, CharSet = CharSet.Ansi)]
        public static extern Int16 COP_LMT_ConfigModuleID([In] UInt16 boardhdl
                                                         , [In] Byte baudrate
                                                         , [MarshalAs(UnmanagedType.LPArray, SizeConst = 7)]
                                                       [In] char[] sz_mname
                                                         , [MarshalAs(UnmanagedType.LPArray, SizeConst = 7)]
                                                       [In] char[] sz_pname
                                                         , [MarshalAs(UnmanagedType.LPArray, SizeConst = 14)]
                                                       [In] char[] sz_sno
                                                         , [In] Byte new_node_no);

        ///*************************************************************************
        /// <summary>
        /// Search for a Node in a specified range
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// - IdentifyRemoteSlaves(sz_mname, sz_pname, sz_snolow, sz_snohigh) </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration (CiA timing table):
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="sz_mname">
        /// manufacturer name (7 chars)
        /// </param>
        /// <param name="sz_pname">
        /// product name (7 chars)
        /// </param>
        /// <param name="sz_snolow">
        /// serial number (14 chars), low boundary
        /// </param>
        /// <param name="sz_snohigh">
        /// serial number (14 chars), high boundary
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success, at least one node found </para><para>
        /// COP_k_NO      : general error </para><para>
        /// COP_k_IV      : invalid parameter </para><para>
        /// COP_k_TIMEOUT : no node found in specified range </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib, CharSet = CharSet.Ansi)]
        public static extern Int16 COP_LMT_IdentifyRemoteSlaves([In] UInt16 boardhdl
                                                               , [In] Byte baudrate
                                                               , [MarshalAs (UnmanagedType.LPArray, SizeConst = 7)]
                                                             [In] char[] sz_mname
                                                               , [MarshalAs (UnmanagedType.LPArray, SizeConst = 7)]
                                                             [In] char[] sz_pname
                                                               , [MarshalAs (UnmanagedType.LPArray, SizeConst = 14)]
                                                             [In] char[] sz_snolow
                                                               , [MarshalAs (UnmanagedType.LPArray, SizeConst = 14)]
                                                             [In] char[] sz_snohigh);

        ///*************************************************************************
        /// <summary>
        /// Deliver the Vendor-ID, the Product-Code, the Revision-Number and the
        /// Serial-Number of the connected device.
        /// </summary>
        /// <remarks>
        /// Attn: There must be only one device connected. <para>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. </para><para>
        /// The internal protocol sequence is as follows: </para><para>
        /// - SwitchModeGlobal() </para><para>
        /// - InquireIdentityVendorID() </para><para>
        /// - InquireIdentityProductCode() </para><para>
        /// - InquireIdentityRevisionNumber() </para><para>
        /// - InquireIdentitySerialNumber() </para><para>
        /// - SwitchModeGlobal() </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// baudrate table selector to access the node for configuration:
        /// COP_k_BAUD_CIA or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="VendorId">
        /// device Vendor-ID
        /// </param>
        /// <param name="ProductCode">
        /// device Product-Code
        /// </param>
        /// <param name="RevisionNo">
        /// device Revision-Number
        /// </param>
        /// <param name="SerialNo">
        /// device Serial-Number
        /// </param>
        /// <returns><para>
        /// BER_k_ERR               : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT          : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT           : no confirmation received </para><para>
        /// COP_k_NO                : general error </para><para>
        /// COP_k_TIMEOUT           : device not present or lost </para><para>
        /// LSS_k_MEDIA_ACCESS_ERROR: CAN bus access failed </para><para>
        /// LSS_k_IV_PARAMETER      : invalid parameter </para><para>
        /// LSS_k_PROTOCOL_ERR      : invalid device response </para><para>
        /// LSS_k_BSY               : currently processing a LSS command  sequence </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_LSS_InquireAddress([In] UInt16 boardhdl
                                                         , [In] Byte baudtable
                                                         , [In] Byte baudrate
                                                         , [Out] out UInt32 VendorId
                                                         , [Out] out UInt32 ProductCode
                                                         , [Out] out UInt32 RevisionNo
                                                         , [Out] out UInt32 SerialNo);

        ///*************************************************************************
        /// <summary>
        /// Deliver the NodeID of a present node in the network.
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// -* SwitchModeGlobal() </para><para>
        /// -* SwitchModeSelective(VendorId, ProductCode, RevisionNo, SerialNo) </para><para>
        /// - InquireNodeID() </para><para>
        /// - SwitchModeGlobal() </para><para>
        /// * in case corresponding <c>>mode</c> flag is set </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// baudrate table selector to access the node for configuration:
        /// COP_k_BAUD_CIA or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="mode">
        /// flags for working mode:
        /// - LSS_k_SET_MODE_SWITCH_MODE_GLOBAL
        /// </param>
        /// <param name="VendorId">
        /// device Vendor-ID
        /// </param>
        /// <param name="ProductCode">
        /// device Product-Code
        /// </param>
        /// <param name="RevisionNo">
        /// device Revision-Number
        /// </param>
        /// <param name="SerialNo">
        /// device Serial-Number
        /// </param>
        /// <param name="node_id">
        /// device Node-ID
        /// </param>
        /// <returns><para>
        /// BER_k_ERR               : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT          : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT           : no confirmation received </para><para>
        /// COP_k_NO                : general error </para><para>
        /// COP_k_TIMEOUT           : device not present or lost </para><para>
        /// LSS_k_MEDIA_ACCESS_ERROR: CAN bus access failed </para><para>
        /// LSS_k_IV_PARAMETER      : invalid parameter </para><para>
        /// LSS_k_PROTOCOL_ERR      : invalid device response </para><para>
        /// LSS_k_BSY               : currently processing a LSS command sequence </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_LSS_InquireNodeID([In] UInt16 boardhdl
                                                        , [In] Byte baudtable
                                                        , [In] Byte baudrate
                                                        , [In] Byte mode
                                                        , [In] UInt32 VendorId
                                                        , [In] UInt32 ProductCode
                                                        , [In] UInt32 RevisionNo
                                                        , [In] UInt32 SerialNo
                                                        , [Out] out Byte node_id);

        ///*************************************************************************
        /// <summary>
        /// Configure the NodeID of a present node in the network.
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// -* SwitchModeGlobal() </para><para>
        /// -* SwitchModeSelective(VendorId, ProductCode, RevisionNo, SerialNo) </para><para>
        /// - ConfigureNodeID(new_node_no) </para><para>
        /// -* StoreConfiguration() </para><para>
        /// - SwitchModeGlobal() </para><para>
        /// * in case corresponding <c>>mode</c> flag is set </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// baudrate table selector to access the node for configuration:
        /// COP_k_BAUD_CIA or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="mode">
        /// flags for working mode:
        /// - LSS_k_SET_MODE_SWITCH_MODE_GLOBAL
        /// - LSS_k_SET_MODE_STORE_CONFIGURATION
        /// </param>
        /// <param name="VendorId">
        /// device Vendor-ID
        /// </param>
        /// <param name="ProductCode">
        /// device Product-Code
        /// </param>
        /// <param name="RevisionNo">
        /// device Revision-Number
        /// </param>
        /// <param name="SerialNo">
        /// device Serial-Number
        /// </param>
        /// <param name="new_node_no">
        /// new node number
        /// </param>
        /// <returns><para>
        /// BER_k_ERR               : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT          : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT           : no confirmation received </para><para>
        /// COP_k_NO                : general error </para><para>
        /// COP_k_TIMEOUT           : device not present or lost </para><para>
        /// LSS_k_MEDIA_ACCESS_ERROR: CAN bus access failed </para><para>
        /// LSS_k_IV_PARAMETER      : invalid parameter </para><para>
        /// LSS_k_PROTOCOL_ERR      : invalid device response </para><para>
        /// LSS_k_BSY               : currently processing a LSS command sequence </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_LSS_ConfigNodeID([In] UInt16 boardhdl
                                                       , [In] Byte baudtable
                                                       , [In] Byte baudrate
                                                       , [In] Byte mode
                                                       , [In] UInt32 VendorId
                                                       , [In] UInt32 ProductCode
                                                       , [In] UInt32 RevisionNo
                                                       , [In] UInt32 SerialNo
                                                       , [In] Byte new_node_no);

        ///*************************************************************************
        /// <summary>
        /// Configure the bit timing (baudrate) of a present node in the network.
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// -* SwitchModeGlobal() </para><para>
        /// -* SwitchModeSelective(VendorId, ProductCode, RevisionNo, SerialNo) </para><para>
        /// - ConfigureBitTimingParameters(new_baudrate) </para><para>
        /// -* ActivateBitTimingParameters(switch_delay) </para><para>
        /// -* StoreConfiguration() </para><para>
        /// - SwitchModeGlobal() </para><para>
        /// * in case corresponding <c>>mode</c> flag is set </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// baudrate table selector to access the node for configuration:
        /// COP_k_BAUD_CIA or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="mode">
        /// flags for working mode:
        /// - LSS_k_SET_MODE_SWITCH_MODE_GLOBAL
        /// - LSS_k_SET_MODE_STORE_CONFIGURATION
        /// - LSS_k_SET_MODE_ACTIVATE_NEW_BAUDRATE
        /// </param>
        /// <param name="VendorId">
        /// device Vendor-ID
        /// </param>
        /// <param name="ProductCode">
        /// device Product-Code
        /// </param>
        /// <param name="RevisionNo">
        /// device Revision-Number
        /// </param>
        /// <param name="SerialNo">
        /// device Serial-Number
        /// </param>
        /// <param name="new_baudtable">
        /// new baudrate table selector for operation after configuration
        /// </param>
        /// <param name="new_baudrate">
        /// new baudrate for operation after configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB,
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB
        /// </param>
        /// <param name="switch_delay">
        /// delay time in ms before transmitting any CAN message at new baudrate
        /// after performing the baudrate switch
        /// </param>
        /// <returns><para>
        /// BER_k_ERR               : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT          : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT           : no confirmation received </para><para>
        /// COP_k_NO                : general error </para><para>
        /// COP_k_TIMEOUT           : device not present or lost </para><para>
        /// LSS_k_MEDIA_ACCESS_ERROR: CAN bus access failed </para><para>
        /// LSS_k_IV_PARAMETER      : invalid parameter </para><para>
        /// LSS_k_PROTOCOL_ERR      : invalid device response </para><para>
        /// LSS_k_BSY               : currently processing a LSS command sequence </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_LSS_ConfigBitTiming([In] UInt16 boardhdl
                                                          , [In] Byte baudtable
                                                          , [In] Byte baudrate
                                                          , [In] Byte mode
                                                          , [In] UInt32 VendorId
                                                          , [In] UInt32 ProductCode
                                                          , [In] UInt32 RevisionNo
                                                          , [In] UInt32 SerialNo
                                                          , [In] Byte new_baudtable
                                                          , [In] Byte new_baudrate
                                                          , [In] UInt16 switch_delay);

        ///*************************************************************************
        /// <summary>
        /// Activate the bit timing (baudrate) of the network.
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// - SwitchModeGlobal() </para><para>
        /// - ActivateBitTimingParameters(switch_delay) </para><para>
        /// - SwitchModeGlobal() </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// baudrate table selector to access the node for configuration:
        /// COP_k_BAUD_CIA or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="new_baudtable">
        /// new baudrate table selector for operation after configuration
        /// </param>
        /// <param name="new_baudrate">
        /// new baudrate for operation after configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB,
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB
        /// </param>
        /// <param name="switch_delay">
        /// delay time in ms before transmitting any CAN message at new baudrate
        /// after performing the baudrate switch
        /// </param>
        /// <returns><para>
        /// BER_k_ERR               : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT          : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT           : no confirmation received </para><para>
        /// COP_k_NO                : general error </para><para>
        /// COP_k_TIMEOUT           : device not present or lost </para><para>
        /// LSS_k_MEDIA_ACCESS_ERROR: CAN bus access failed </para><para>
        /// LSS_k_IV_PARAMETER      : invalid parameter </para><para>
        /// LSS_k_PROTOCOL_ERR      : invalid device response </para><para>
        /// LSS_k_BSY               : currently processing a LSS command sequence </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_LSS_ActivateBitTiming([In] UInt16 boardhdl
                                                            , [In] Byte baudtable
                                                            , [In] Byte baudrate
                                                            , [In] Byte new_baudtable
                                                            , [In] Byte new_baudrate
                                                            , [In] UInt16 switch_delay);

        ///*************************************************************************
        /// <summary>
        /// Search for a Node in a specified range
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// - IdentifyRemoteSlaves(VendorId, ProductCode, RevisionNoLow,
        ///     RevisionNoHigh, SerialNoLow, SerialNoHigh) </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// baudrate table selector to access the node for configuration:
        /// COP_k_BAUD_CIA or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="VendorId">
        /// device Vendor-ID
        /// </param>
        /// <param name="ProductCode">
        /// device Product-Code
        /// </param>
        /// <param name="RevisionNoLow">
        /// device Revision-Number, lower boundary
        /// </param>
        /// <param name="RevisionNoHigh">
        /// device Revision-Number, higher boundary
        /// </param>
        /// <param name="SerialNoLow">
        /// device Serial-Number, lower boundary
        /// </param>
        /// <param name="SerialNoHigh">
        /// device Serial-Number, higher boundary
        /// </param>
        /// <returns><para>
        /// BER_k_ERR               : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT          : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT           : no confirmation received </para><para>
        /// COP_k_NO                : general error </para><para>
        /// COP_k_TIMEOUT           : device not present or lost </para><para>
        /// LSS_k_MEDIA_ACCESS_ERROR: CAN bus access failed </para><para>
        /// LSS_k_IV_PARAMETER      : invalid parameter </para><para>
        /// LSS_k_PROTOCOL_ERR      : invalid device response </para><para>
        /// LSS_k_BSY               : currently processing a LSS command sequence </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_LSS_IdentifyRemoteSlaves([In] UInt16 boardhdl
                                                               , [In] Byte baudtable
                                                               , [In] Byte baudrate
                                                               , [In] UInt32 VendorId
                                                               , [In] UInt32 ProductCode
                                                               , [In] UInt32 RevisionNoLow
                                                               , [In] UInt32 RevisionNoHigh
                                                               , [In] UInt32 SerialNoLow
                                                               , [In] UInt32 SerialNoHigh);

        ///*************************************************************************
        /// <summary>
        /// Search for any present node in the network whose Node-ID is not configured.
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// The internal protocol sequence is as follows: </para><para>
        /// - IdentifyNonConfiguredRemoteSlaves() </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// baudrate table selector to access the node for configuration:
        /// COP_k_BAUD_CIA or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <returns><para>
        /// BER_k_ERR               : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT          : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT           : no confirmation received </para><para>
        /// COP_k_NO                : general error </para><para>
        /// COP_k_TIMEOUT           : device not present or lost </para><para>
        /// LSS_k_MEDIA_ACCESS_ERROR: CAN bus access failed </para><para>
        /// LSS_k_IV_PARAMETER      : invalid parameter </para><para>
        /// LSS_k_PROTOCOL_ERR      : invalid device response </para><para>
        /// LSS_k_BSY               : currently processing a LSS command sequence </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_LSS_IdentifyNonConfRemoteSlaves([In] UInt16 boardhdl
                                                                      , [In] Byte baudtable
                                                                      , [In] Byte baudrate);

        ///*************************************************************************
        /// <summary>
        /// Change the LSS/LMT timeout value. Value in milliseconds.
        /// </summary>
        /// <remarks>
        /// Default value is 100 ms
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="w_timeout">
        /// new timeout in ms
        /// </param>
        /// <returns><para>
        /// BER_k_ERR     : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT: command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT : no confirmation received </para><para>
        /// COP_k_OK      : success </para><para>
        /// COP_k_IV      : invalid parameter </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_SetLSSTimeOut([In] UInt16 boardhdl
                                                     , [In] UInt16 w_timeout);

        ///*************************************************************************
        /// <summary>
        /// Search non-configured LSS slaves
        /// </summary>
        /// <remarks>
        /// Parameter <c>baudrate</c> is considered when firmware hasn't already
        /// been initialised using <c>COP_InitInterface</c> only. <para>
        /// This function provides a means to find the first non-configured slave
        /// and returns its identity attributes.
        /// By those, the slave might be configured using the other LSS commands,
        /// and then Fastscan could be repeated until no further unconfigured slave
        /// is found. </para><para>
        /// Another use case is find a non-configured device by a (partially) given
        /// LSS number. For this, the partial LSS numbers shall be given as input
        /// arguments, together with the lowest bit to match of a LSS number. Thus,
        /// comparison is performed for the count of high-order bits that range just
        /// from MSB down to the individual Bit number given: </para><para>
        /// <c>*VendorId    = 0xA0000000 (10100000.00000000.00000....) </c></para><para>
        /// VendorIdBits = 29  (32-29 = 3;  i.e. 3 high-order bits) </para><para>
        /// In this example, any device whose Vendor-ID begins with binary bit pattern
        /// <c>101xxx</c> (i.e. 0xAxx, 0xBxx) will be found. </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="baudtable">
        /// baudrate table selector to access the node for configuration:
        /// COP_k_BAUD_CIA or COP_k_BAUD_USER
        /// </param>
        /// <param name="baudrate">
        /// baudrate to access the node for configuration:
        /// COP_k_10_KB, COP_k_20_KB, COP_k_50_KB, COP_k_100_KB, COP_k_125_KB
        /// COP_k_250_KB, COP_k_500_KB, COP_k_800_KB, COP_k_1000_KB.
        /// Applies only if <c>COP_InitInterface</c> hasn't been called yet.
        /// </param>
        /// <param name="VendorId">
        /// IN: device Vendor-ID. Typically, this value should be set to 0 by the
        /// caller for a full range scan. Alternatively, it can be set to a bit
        /// pattern to match, i.e. to find a particular device or a group of
        /// devices. <para>
        /// OUT: Exact (first) found Vendor-ID </para>
        /// </param>
        /// <param name="VendorIdBits">
        /// bits to be checked of given VendorId. (0 .. 31).
        /// Typically, this value should be set to 31, not to 0, by the caller
        /// for a full range scan. Any value below 31 determines the lowest bit
        /// position of a range starting at MSB to be checked for a bit pattern
        /// match: VendorIdBits up to 31 to be compared.
        /// </param>
        /// <param name="ProductCode">
        /// IN: device Product-Code. Typically, this value should be set to 0 by
        /// the caller for a full range scan. Alternatively, it can be set to a
        /// bit pattern to match, i.e. to find a particular device or a group of
        /// devices. <para>
        /// OUT: Exact (first) found Product-Code within given range </para><para>
        /// To skip scanning of the Product-Code, the Revision-Number and the
        /// Serial-Number, the argument can also be omitted (NULL) </para>
        /// </param>
        /// <param name="ProductCodeBits">
        /// bits to be checked of given ProductCode (0 .. 31 ).
        /// Typically, this value should be set to 31, not to 0, by the caller for
        /// a full range scan. Any value below 31 determines the lowest bit position
        /// of a range starting at MSB to be checked for a bit pattern match:
        /// ProductCodeBits up to 31 to be compared.
        /// </param>
        /// <param name="RevisionNo">
        /// IN: device Revision-Number. Typically, this value should be set to 0
        /// by the caller for a full range scan. Alternatively, it can be set to
        /// a bit pattern to match, i.e. to find a particular device or a group of
        /// devices. <para>
        /// OUT: Exact (first) found Revision-Number within given range </para><para>
        /// To skip scanning of the Revision-Number and the Serial-Number, the
        /// argument can also be omitted (NULL) </para>
        /// </param>
        /// <param name="RevisionNoBits">
        /// bits to be checked of given RevisionNo (0 .. 31).
        /// Typically, this value should be set to 31, not to 0, by the caller for
        /// a full range scan. Any value below 31 determines the lowest bit position
        /// of a range starting at MSB to be checked for a bit pattern match:
        /// RevisionNoBits up to 31 to be compared.
        /// </param>
        /// <param name="SerialNo">
        /// IN: device Serial-Number. Typically, this value should be set to 0
        /// by the caller for a full range scan. Alternatively, it can be set to
        /// a bit pattern to match, i.e. to find a particular device or a group of
        /// devices. <para>
        /// OUT: Exact (first) found Serial-Number within given range </para><para>
        /// To skip scanning of the Serial-Number, the argument can also be omitted
        /// (NULL) </para>
        /// </param>
        /// <param name="SerialNoBits">
        /// bits to be checked of given SerialNo (0 .. 31).
        /// Typically, this value should be set to 31, not to 0, by the caller for
        /// a full range scan. Any value below 31 determines the lowest bit position
        /// of a range starting at MSB to be checked for a bit pattern match:
        /// SerialNoBits up to 31 to be compared.
        /// </param>
        /// <returns><para>
        /// COP_k_OK                        : Non-configured slave found </para><para>
        /// BER_k_ERR                       : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT                  : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT                   : no confirmation received </para><para>
        /// COP_k_NO                        : general error </para><para>
        /// LSS_k_MEDIA_ACCESS_ERROR        : CAN bus access failed </para><para>
        /// LSS_k_IV_PARAMETER              : invalid parameter </para><para>
        /// LSS_k_PROTOCOL_ERR              : invalid device response </para><para>
        /// LSS_k_BSY                       : currently processing a LSS command sequence </para><para>
        /// LSS_k_FS_NO_NONCONFIGURED_SLAVE : No (non-configured) slave found </para><para>
        /// LSS_k_FS_NF_NONCONFIGURED_SLAVE : No slave found within given range </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_LSS_Fastscan([In] UInt16 boardhdl
                                                    , [In] Byte baudtable
                                                    , [In] Byte baudrate
                                                    , [In][Out] ref UInt32 VendorId
                                                    , [In] Byte VendorIdBits
                                                    , [In][Out] ref UInt32 ProductCode
                                                    , [In] Byte ProductCodeBits
                                                    , [In][Out] ref UInt32 RevisionNo
                                                    , [In] Byte RevisionNoBits
                                                    , [In][Out] ref UInt32 SerialNo
                                                    , [In] Byte SerialNoBits);

        ///*************************************************************************
        /// <summary>
        /// Initial parameterization of Flying Master
        /// </summary>
        /// <remarks>
        /// Flying Master engine will not work if this function has not been called. <para>
        /// Only valid if <c>COP_InitInterface</c> was called with parameter
        /// AddFeatures=COP_k_FEATURE_FLYING_MASTER resp.
        /// AddFeatures=COP_k_FEATURE_MULTI_MASTER </para>
        /// </remarks>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="wDetectionTimeout">
        /// Contents of [1F90sub1] <para>
        /// Recommended default: 100 </para>
        /// </param>
        /// <param name="wNegotiationDelay">
        /// Contents of [1F90sub2] <para>
        /// Recommended default: 500 </para>
        /// </param>
        /// <param name="wPriority">
        /// Contents of [1F90sub3] <para>
        /// 0 - High priority </para><para>
        /// 1 - Medium priority </para><para>
        /// 2 - Low Priority </para>
        /// </param>
        /// <param name="wPriorityTimeslot">
        /// Contents of [1F90sub4] <para>
        /// Recommended default: 1500 </para>
        /// </param>
        /// <param name="wNodeTimeslot">
        /// Contents of [1F90sub5] <para>
        /// Recommended default: 10 </para>
        /// </param>
        /// <param name="wCycletimeCd">
        /// Contents of [1F90sub6] <para>
        /// Cycle time of collision detection with another active master. </para><para>
        /// Recommended default: 4000 + (10 * own NodeID) </para>
        /// </param>
        /// <param name="wCycletimeTimeoutHbeat">
        /// Timeout for heartbeat monitoring when API is slave and other node is
        /// active master
        /// </param>
        /// <returns><para>
        /// BER_k_ERR                   : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT              : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT               : no confirmation received </para><para>
        /// COP_k_OK                    : success </para><para>
        /// COP_k_IV                    : invalid argument </para><para>
        /// COP_k_UNKNOWN               : Firmware does not support Flying Master
        ///                               or Function already successfully called </para><para>
        /// COP_k_NO_FLY_MASTER_PRESENT : Flying Master feature is not activated </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_ConfigFlyMaster([In] UInt16 boardhdl
                                                      , [In] UInt16 wDetectionTimeout
                                                      , [In] UInt16 wNegotiationDelay
                                                      , [In] UInt16 wPriority
                                                      , [In] UInt16 wPriorityTimeslot
                                                      , [In] UInt16 wNodeTimeslot
                                                      , [In] UInt16 wCycletimeCd
                                                      , [In] UInt16 wCycletimeTimeoutHbeat);

        ///*************************************************************************
        /// <summary>
        /// Start configured Flying Master <para>
        /// Only valid if <c>COP_InitInterface</c> was called with parameter
        /// AddFeatures=COP_k_FEATURE_FLYING_MASTER resp.
        /// AddFeatures=COP_k_FEATURE_MULTI_MASTER </para>
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <returns><para>
        /// BER_k_ERR                   : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT              : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT               : no confirmation received </para><para>
        /// COP_k_OK                    : success </para><para>
        /// COP_k_UNKNOWN               : Function already successfully called </para><para>
        /// COP_k_PRESENT_DEVICE_STATE  : Flying Master is not configured </para><para>
        /// COP_k_NO_FLY_MASTER_PRESENT : Flying Master feature is not activated </para><para>
        /// COP_k_CANID_IN_USE          : CAN-Identifier already in use </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_StartFlyMaster([In] UInt16 boardhdl);

        ///*************************************************************************
        /// <summary>
        /// Return status of Flying Master negotiation <para>
        /// Only valid if <c>COP_InitInterface</c> was called with parameter
        /// AddFeatures=COP_k_FEATURE_FLYING_MASTER resp.
        /// AddFeatures=COP_k_FEATURE_MULTI_MASTER </para>
        /// </summary>
        /// <param name="boardhdl">
        /// handle of CAN board
        /// </param>
        /// <param name="status">
        /// Status of negotiation
        /// </param>
        /// <param name="masterid">
        /// Node-ID of master
        /// </param>
        /// <param name="masterprio">
        /// Priority of master
        /// </param>
        /// <returns><para>
        /// BER_k_ERR                   : boardhandle not valid </para><para>
        /// BER_k_NOT_SENT              : command msg couldn't be handed over </para><para>
        /// BER_k_TIMEOUT               : no confirmation received </para><para>
        /// COP_k_OK                    : success </para><para>
        /// COP_k_IV                    : invalid argument </para><para>
        /// COP_k_UNKNOWN               : Firmware does not support Flying Master </para><para>
        /// COP_k_PRESENT_DEVICE_STATE  : Flying Master is not started </para><para>
        /// COP_k_NO_FLY_MASTER_PRESENT : Flying Master feature is not activated </para>
        /// </returns>

        [DllImport(CANopenMasterAPI6lib)]
        public static extern Int16 COP_GetStatusFlyMasterNeg([In] UInt16 boardhdl
                                                            , [Out] out Byte status
                                                            , [Out] out Byte masterid
                                                            , [Out] out Byte masterprio);

        #endregion

#if NETCOREAPP2_0
#pragma warning restore CA1401  // P/Invoke method '' should not be visible
#endif

        #region Errorcode Description Methods

        ///*************************************************************************
        /// <summary>
        /// Return description of given CANopen Abort code
        /// </summary>
        /// <param name="copAbortCode">
        /// CANopen SDO Abort code according to DS-401 page 9-26f
        /// </param>
        /// <returns>
        /// CANopen Abort code description
        /// </returns>

        public static string CopAbortCodeString(UInt32 copAbortCode)
        {
            string description;

            switch (copAbortCode)
            {
                case 0x00000000: description = "No Abort, SDO transfer successful"; break;

                case 0x05030000: description = "Toggle bit not alternated."; break;
                case 0x05040000: description = "SDO protocol timed out."; break;
                case 0x05040001: description = "Client/server command specifier not valid or unknown."; break;
                case 0x05040002: description = "Invalid block size (block mode only)."; break;
                case 0x05040003: description = "Invalid sequence number (block mode only)."; break;
                case 0x05040004: description = "CRC error (block mode only)."; break;
                case 0x05040005: description = "Out of memory."; break;

                case 0x06010000: description = "Unsupported access to an object."; break;
                case 0x06010001: description = "Attempt to read a write only object."; break;
                case 0x06010002: description = "Attempt to write a read only object."; break;
                case 0x06020000: description = "Object does not exist in the object dictionary."; break;
                case 0x06040041: description = "Object cannot be mapped to the PDO."; break;
                case 0x06040042: description = "The number and length of the objects to be mapped would exceed PDO length."; break;
                case 0x06040043: description = "General parameter incompatibility reason."; break;
                case 0x06040047: description = "General internal incompatibility in the device."; break;
                case 0x06060000: description = "Access failed due to an hardware error."; break;
                case 0x06070010: description = "Data type does not match, length of service parameter does not match"; break;
                case 0x06070012: description = "Data type does not match, length of service parameter too high"; break;
                case 0x06070013: description = "Data type does not match, length of service parameter too low"; break;
                case 0x06090011: description = "Sub-index does not exist."; break;
                case 0x06090030: description = "Value range of parameter exceeded (only for write access)."; break;
                case 0x06090031: description = "Value of parameter written too high."; break;
                case 0x06090032: description = "Value of parameter written too low."; break;
                case 0x06090036: description = "Maximum value is less than minimum value."; break;
                case 0x060A0023: description = "Resource not available: SDO connection."; break;

                case 0x08000000: description = "general error"; break;
                case 0x08000020: description = "Data cannot be transferred or stored to the application."; break;
                case 0x08000021: description = "Data cannot be transferred or stored to the application because of local control."; break;
                case 0x08000022: description = "Data cannot be transferred or stored to the application because of the present device state."; break;
                case 0x08000023: description = "Object dictionary dynamic generation fails or no object dictionary is present (e.g. object dictionary is generated from file and generation fails because of an file error)."; break;
                case 0x08000024: description = "No data available."; break;

                default: description = "unknown AbortCode"; break;
            }

            return description;
        }

        ///*************************************************************************
        /// <summary>
        /// Return description of given COP error code
        /// </summary>
        /// <param name="copErrorCode">
        /// Returnvalue of any CANopen Master API function
        /// </param>
        /// <returns>
        /// CANopen Master API error code description
        /// </returns>
        public static string CopErrorString(Int32 copErrorCode)
        {
            string description;

            switch (copErrorCode)
            {
                case BER_k_OK: description = "success"; break;
                case BER_k_ERR: description = "general error"; break;
                case BER_k_EDS_FILENOTFOUND: description = "device description file not found"; break;
                case BER_k_EDS_CORRUPT: description = "device description file import failed"; break;
                case BER_k_EDSLIB: description = "EDSLIB4.dll is missing"; break;
                case BER_k_DATA_CORRUPT: description = "corrupt data detected MC to PC"; break;
                case BER_k_NOT_SENT: description = "msg not sent; try again"; break;
                case BER_k_TIMEOUT: description = "timeout in communication PC to MC"; break;
                case BER_k_BOARD_ALREADY_USED: description = "board is used by another instance"; break;
                case BER_k_ALL_BOARDS_USED: description = "no free board slots inside DLL"; break;
                case BER_k_BOARD_NOT_SUPP: description = "the given board is not supported by CANopen Master API"; break;
                case BER_k_BOARD_NOT_FOUND: description = "the board wasn't found"; break;
                case BER_k_CANNOT_SEARCH_BOARD: description = "Hardware selection Dialog cancelled by user"; break;
                case BER_k_WRONG_FW: description = "wrong firmware version"; break;
                case BER_k_USED_FROM_OTHER_PROCESS: description = "board is used by another application"; break;
                case BER_k_PC_MC_COMM_ERR: description = "communication error PC to MC"; break;
                case BER_k_BOARD_DLD_ERR: description = "an error occured while firmware download"; break;
                case BER_k_BADCALLBACK_PTR: description = "a callbackpointer is invalid"; break;
                case BER_k_NO_SUCH_CANLINE: description = "given CANline is not available or not supported"; break;
                case BER_k_CANLINE_USED: description = "CANline is already in use"; break;
                case BER_k_VCI_INST_ERR: description = "IXXAT CAN driver is missing"; break;
                case BER_k_BOARD_ERR: description = "unknown board type or can't locate board type"; break;
                case BER_k_MEM_ALLOC_ERR: description = "memory allocation error (internal) data or OS element couldn't be created"; break;
                case BER_k_CCI_INST_ERR: description = "CCI installation error (internal)"; break;
                case BER_k_SDO_INST_ERR: description = "SDO handler installation error (internal)"; break;
                case BER_k_SDO_THREAD_ERR: description = "SDO thread execution cancelled while waiting for SDO response from firmware"; break;

                case COP_k_CAL_ERR: description = "failure in CAL"; break;
                case COP_k_IV: description = "invalid parameter or service not allowed"; break;
                case COP_k_ABORT: description = "transfer aborted"; break;
                case COP_k_NOT_FOUND: description = "node is undeclared"; break;
                case COP_k_NOT_INIT: description = "CANopen-Master not initialised"; break;
                case COP_k_INIT: description = "CANopen-Master already initialised"; break;
                case COP_k_QUEUE_EMPTY: description = "no objects in queue"; break;
                case COP_k_TIMEOUT: description = "timeout in CAN communication"; break;
                case COP_k_SDO_RUNNING: description = "SDO transfer in progress, retry later"; break;
                case COP_k_BSY: description = "generic process still running"; break;
                case COP_k_NO_OBJECT: description = "object does not exist"; break;
                case COP_k_NO_SUBINDEX: description = "subindex does not exist"; break;
                case COP_k_WRITE_ONLY: description = "object is write only"; break;
                case COP_k_PRESENT_DEVICE_STATE: description = "access currently not possible"; break;
                case COP_k_RANGE_EXCEEDED: description = "parameter out of range"; break;
                case COP_k_UNKNOWN: description = "unknown command"; break;
                case COP_k_NO_FLY_MASTER_PRESENT: description = "API/hardware version does not support Flying Master"; break;
                case COP_k_NO_LOWSPEED: description = "No LowSpeed bus-coupling present or supported"; break;
                default: description = "unknown ErrorCode"; break;
            }

            return description;
        }

        ///*************************************************************************
        /// <summary>
        /// Return description of given COP status event type
        /// </summary>
        /// <param name="copEventType">
        /// Value of any CANopen Master API status event (COP_k_aaa_EVT)
        /// </param>
        /// <returns>
        /// CANopen MAster API status event type description
        /// </returns>

        public static string CopEventTypeString(Byte copEventType)
        {
            string description;

            switch (copEventType)
            {
                case COP_k_NMT_EVT: description = "NMT event"; break;
                case COP_k_DLL_EVT: description = "API/DLL event"; break;
                case COP_k_WPDO_EVT: description = "WritePDO event"; break;
                case COP_k_RPDO_EVT: description = "ReadPDO event"; break;
                case COP_k_QUEUE_OVRUN_EVT: description = "Queue Overrun event"; break;
                case COP_k_FLY_EVT: description = "Flying Master event"; break;
                default: description = "Unknown EventType"; break;
            }

            return description;
        }

        #endregion
    }
}
