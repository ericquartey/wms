using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.CanOpenClient
{
    public class MessageSDO
    {
        #region Fields

        private const int COB_ID_TXSDO = 0x600;

        #endregion

        #region Constructors

        public MessageSDO()
        {
        }

        public MessageSDO(byte node, ushort index, byte subindex, byte[] data, ushort dataLength)
        {
            this.Index = index;
            this.Subindex = subindex;
            this.DataLength = dataLength;
            this.TransmittedBytes = 0;
            if (dataLength > 0)
            {
                this.Data = new byte[dataLength];
                Array.Copy(data, 0, this.Data, 0, data.Length);
            }
            this.Id = (uint)(node + COB_ID_TXSDO);
            this.TxData = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                this.TxData[i] = 0;
            }
            int offset = 0;
            byte command;
            if (dataLength > 4)
            {
                command = 0x21;
            }
            else
            {
                command = (byte)(this.IsWrite ? 0x20 + EncodedLength(dataLength) : 0x40);
            }

            this.TxData[offset++] = command;
            Array.Copy(BitConverter.GetBytes(index), 0, this.TxData, offset, 2);
            offset += 2;
            this.TxData[offset++] = subindex;
            if (this.IsWrite)
            {
                if (dataLength > 4)
                {
                    offset += 2;
                    Array.Copy(BitConverter.GetBytes(dataLength), 0, this.TxData, offset, 2);
                }
                else
                {
                    Array.Copy(data, 0, this.TxData, offset, 4);
                }
            }
        }

        #endregion

        #region Properties

        public ulong AbortCode { get; set; }

        public byte Command => this.TxData[0];

        // complete data received from device or transmitted to device
        public byte[] Data { get; set; }

        public ushort DataLength { get; set; }

        public uint Id { get; set; }

        public ushort Index { get; set; }

        public bool IsSegmented { get; set; }

        public bool IsWrite => this.DataLength > 0;

        public ushort ReceivedBytes { get; set; }

        public byte Subindex { get; set; }

        public ushort TransmittedBytes { get; set; }

        // data to be transmitted at next TransmitData call
        public byte[] TxData { get; set; }

        #endregion

        #region Methods

        public bool CheckSegmented(byte command, byte[] data)
        {
            ushort receivedBytes = 0;
            switch (command)
            {
                case 0x4F:
                    receivedBytes = 1;
                    this.IsSegmented = false;
                    break;

                case 0x4B:
                    receivedBytes = 2;
                    this.IsSegmented = false;
                    break;

                case 0x47:
                    receivedBytes = 3;
                    this.IsSegmented = false;
                    break;

                case 0x43:
                    receivedBytes = 4;
                    this.IsSegmented = false;
                    break;

                case 0x60:
                    this.IsSegmented = false;
                    break;

                case 0x41:
                case 0:
                case 0x10:
                    receivedBytes = 4;
                    this.IsSegmented = true;
                    break;

                case 0x20:
                case 0x30:
                    if (this.TransmittedBytes < this.DataLength)
                    {
                        this.IsSegmented = true;
                    }
                    else
                    {
                        // last segment of the write message
                        this.IsSegmented = false;
                    }
                    break;

                default:
                    if ((command & 0xE0) == 0
                        && (command & 0x01) == 1)       // continue bit
                    {
                        // last segment of the read message
                        receivedBytes = (ushort)(4 - ((command & 0x0E) / 2));
                        this.IsSegmented = false;
                    }
                    else
                    {
                        this.IsSegmented = true;
                    }
                    break;
            }
            if (receivedBytes > 0)
            {
                Array.Copy(data, 0, this.Data, this.ReceivedBytes, receivedBytes);
                this.ReceivedBytes += receivedBytes;
            }
            return this.IsSegmented;
        }

        public void ContinueRead()
        {
            if (this.Command == 0x40)
            {
                this.TxData[0] = 0x60;
            }
            else if (this.Command == 0x60)
            {
                this.TxData[0] = 0x70;
            }
            else if (this.Command == 0x70)
            {
                this.TxData[0] = 0x60;
            }
        }

        public void ContinueWrite()
        {
            if (this.Command == 0x21)
            {
                this.TxData[0] = 0x00;
            }
            else if (this.Command == 0x00)
            {
                this.TxData[0] = 0x10;
            }
            else if (this.Command == 0x10)
            {
                this.TxData[0] = 0x00;
            }
            var transmittedBytes = Math.Min(4, this.DataLength - this.TransmittedBytes);
            Array.Copy(this.Data, this.TransmittedBytes, this.TxData, 4, transmittedBytes);
            if (this.TransmittedBytes >= this.DataLength)
            {
                // last segment of the write message
                this.TxData[0] |= (byte)((4 - transmittedBytes) * 2);
                this.TxData[0] |= 0x01;         // continue bit
            }
            this.TransmittedBytes += (ushort)transmittedBytes;
        }

        private static byte EncodedLength(ushort dataLength)
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

        #endregion
    }
}
