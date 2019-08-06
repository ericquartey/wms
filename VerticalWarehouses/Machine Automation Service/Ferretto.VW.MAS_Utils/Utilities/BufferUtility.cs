using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ferretto.VW.MAS.Utils.Utilities
{
    public static class BufferUtility
    {
        #region Methods

        public static byte[] AppendArrays(this byte[] a, byte[] b, int bytesRead)
        {
            if (a == null)
            {
                Array.Resize<byte>(ref a, 0);
            }

            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            byte[] c = new byte[a.Length + bytesRead];
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, bytesRead);
            return c;
        }

        public static int ByteIndexOf(byte[] searched, byte[] find, int start)
        {
            for (int index = start; index <= searched.Length - find.Length; ++index)
            {
                var matched = true;
                for (int subIndex = 0; subIndex < find.Length; ++subIndex)
                {
                    if (find[subIndex] == searched[index + subIndex])
                    {
                        continue;
                    }

                    matched = false;
                    break;
                }

                if (matched)
                {
                    return index;
                }
            }
            return -1;
        }

        public static IList<byte[]> GetMessagesWithHeaderLengthToEnqueue(ref byte[] receiveBuffer, int totalHeaderLength, int iLength, int lengthAdjust)
        {
            IList<byte[]> messages = new List<byte[]>();
            if (receiveBuffer.Length >= totalHeaderLength + lengthAdjust)
            {
                int startIndex = 0;
                while (startIndex != -1 && receiveBuffer.Length > 0)
                {
                    int messageLength = receiveBuffer[startIndex + iLength] + lengthAdjust;

                    // check if there is a message to extract
                    if (messageLength > 0 && receiveBuffer.Length >= messageLength)
                    {
                        // Cut message from raw buffer and enqueue it
                        byte[] message = new byte[messageLength];
                        Array.Copy(receiveBuffer, startIndex, message, 0, message.Length);
                        messages.Add(message);

                        // Remove message from raw buffer
                        int count = receiveBuffer.Length - messageLength;
                        byte[] newarray = new byte[count];
                        Buffer.BlockCopy(receiveBuffer, startIndex + messageLength, newarray, 0, count);
                        receiveBuffer = newarray;
                        startIndex = 0;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return messages;
        }

        #endregion
    }
}
