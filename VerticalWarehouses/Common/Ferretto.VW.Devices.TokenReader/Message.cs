using System;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.VW.Devices.TokenReader
{
    internal class Message
    {
        #region Fields

        private const byte DataLinkEscape = 0x10;

        private const int DeviceAddressIndex = 3;

        private const byte EndOfText = 0x03;

        private const int MinPayloadByteCount = 7;

        private const int PayloadByteCountIndex = 0;

        private const int UserDataByteCountIndex = 6;

        private const int UserDataStartAddressIndex = 5;

        #endregion

        #region Constructors

        public Message(Command command, int deviceAddress, int userDataByteCount, int userDataStartAddress)
        {
            if (deviceAddress < 0)
            {
                throw new ApplicationException("The device address cannot be negative.");
            }

            if (userDataByteCount < 0)
            {
                throw new ApplicationException($"The user data byte count cannot be negative.");
            }

            if (userDataStartAddress < 0)
            {
                throw new ApplicationException($"The user data start address cannot be negative.");
            }

            this.Command = command;
            this.DeviceAddress = deviceAddress;
            this.PayloadByteCount = MinPayloadByteCount;
            this.UserDataByteCount = userDataByteCount;
            this.UserDataStartAddress = userDataStartAddress;
        }

        #endregion

        #region Properties

        public static int SerialNumberByteCount => 8;

        public static int SerialNumberDataAddress => 116;

        public Command Command { get; private set; }

        public int DeviceAddress { get; private set; }

        public int PayloadByteCount { get; private set; }

        public int UserDataByteCount { get; private set; }

        public int UserDataStartAddress { get; private set; }

        #endregion

        #region Methods

        public static Message FromBytes(byte[] messageBytes, int startIndex, int byteCount)
        {
            if (messageBytes is null)
            {
                throw new ArgumentNullException(nameof(messageBytes));
            }

            messageBytes = messageBytes.Skip(startIndex).Take(byteCount).ToArray();

            ValidateBytes(messageBytes);

            var payloadByteCount = messageBytes[PayloadByteCountIndex];
            var command = GetCommandFromBytes(messageBytes[1], messageBytes[2]);
            var deviceAddress = messageBytes[DeviceAddressIndex];
            var userDataByteCount = messageBytes[UserDataByteCountIndex];
            var userDataStartAddress = messageBytes[UserDataStartAddressIndex];

            if (userDataByteCount > 0)
            {
                var userData = GetUserData(messageBytes, userDataByteCount);

                return new DataMessage(
                    command,
                    deviceAddress,
                    userDataByteCount,
                    userDataStartAddress,
                    userData)
                {
                    PayloadByteCount = payloadByteCount
                };
            }
            else
            {
                return new Message(
                    command,
                    deviceAddress,
                    userDataByteCount,
                    userDataStartAddress)
                {
                    PayloadByteCount = payloadByteCount
                };
            }
        }

        public byte[] ToByteArray()
        {
            if (this.Command is Command.TL)
            {
                var message = new byte[] {
                    (byte)this.PayloadByteCount,
                    (byte)'T',
                    (byte)'L',
                    (byte)this.DeviceAddress,
                    0,
                    (byte)this.UserDataStartAddress,
                    (byte)this.UserDataByteCount,
                    DataLinkEscape,
                    EndOfText,
                    0 // BCC
                };

                message[message.Length - 1] = GetBlockCheckCharacter(message);

                return message;
            }

            return null;
        }

        private static byte GetBlockCheckCharacter(byte[] message)
        {
            byte blockCheckCharacter = 0;
            for (int i = 0; i < message.Length - 1; i++)
            {
                blockCheckCharacter ^= message[i];
            }

            return blockCheckCharacter;
        }

        private static Command GetCommandFromBytes(byte byte1, byte byte2)
        {
            if (byte1 == 'T' && byte2 == 'L')
            {
                return Command.TL;
            }
            else if (byte1 == 'T' && byte2 == 'P')
            {
                return Command.TP;
            }
            else if (byte1 == 'R' && byte2 == 'F')
            {
                return Command.RF;
            }
            else if (byte1 == 'R' && byte2 == 'L')
            {
                return Command.RL;
            }

            throw new ArgumentException("The command bytes do not represent a valid command.");
        }

        private static byte[] GetUserData(byte[] messageBytes, byte userDataByteCount)
        {
            if (userDataByteCount <= 0)
            {
                return null;
            }
            else
            {
                var dataBytes = new List<byte>();
                var skipByte = false;
                foreach (var element in messageBytes.Skip(MinPayloadByteCount))
                {
                    if (element is DataLinkEscape)
                    {
                        skipByte = !skipByte;
                    }

                    if (!skipByte)
                    {
                        dataBytes.Add(element);
                    }
                }

                return dataBytes.Take(userDataByteCount).ToArray();
            }
        }

        private static void ValidateBytes(byte[] messageBytes)
        {
            if (messageBytes[messageBytes.Length - 2] != EndOfText)
            {
                throw new ArgumentException("The message does not finish with the End-of-Text character.");
            }

            if (messageBytes[messageBytes.Length - 3] != DataLinkEscape)
            {
                throw new ArgumentException("The message does not finish with the Data-Link-Escape character.");
            }

            var bCC = GetBlockCheckCharacter(messageBytes);

            if (messageBytes[messageBytes.Length - 1] != bCC)
            {
                throw new ArgumentException("The block check character validation failed.");
            }
        }

        #endregion
    }
}
