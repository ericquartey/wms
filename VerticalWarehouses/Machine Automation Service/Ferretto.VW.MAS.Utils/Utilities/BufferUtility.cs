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
                Array.Resize(ref a, 0);
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            var c = new byte[a.Length + bytesRead];
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, bytesRead);
            return c;
        }

        public static int ByteIndexOf(byte[] searched, byte[] find, int start)
        {
            if (searched is null)
            {
                throw new ArgumentNullException(nameof(searched));
            }

            if (find is null)
            {
                throw new ArgumentNullException(nameof(find));
            }

            for (var index = start; index <= searched.Length - find.Length; ++index)
            {
                var matched = true;
                for (var subIndex = 0; subIndex < find.Length; ++subIndex)
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

        public static IList<string> GetMessagesToEnqueue(ref byte[] receiveBuffer, byte[] messageStartPattern, byte[] messageEndPattern)
        {
            if (messageEndPattern == null)
            {
                throw new ArgumentNullException(nameof(messageEndPattern));
            }

            int startIndex = 0;

            bool useStartPattern = false;
            if (messageStartPattern != null)
            {
                useStartPattern = messageStartPattern.Any();
            }
            else
            {
                messageStartPattern = new byte[0];
            }

            IList<string> messages = new List<string>();
            while (startIndex != -1 && startIndex < receiveBuffer.Length)
            {
                if (receiveBuffer.Length > 0)
                {
                    if (useStartPattern)
                    {
                        startIndex = ByteIndexOf(receiveBuffer, messageStartPattern, startIndex);
                    }

                    if (!useStartPattern || (startIndex != -1 && receiveBuffer.Length >= (startIndex + messageStartPattern.Length)))
                    {
                        int endIndex = ByteIndexOf(receiveBuffer, messageEndPattern, startIndex + 1);
                        if (endIndex != -1)
                        {
                            // Cut message from raw buffer and enqueue it
                            byte[] message = new byte[endIndex - startIndex - messageStartPattern.Length];
                            Array.Copy(receiveBuffer, startIndex + messageStartPattern.Length, message, 0, message.Length);
                            messages.Add(Encoding.ASCII.GetString(message));

                            // Remove message from raw buffer
                            int count = receiveBuffer.Length - endIndex - messageEndPattern.Length;
                            byte[] newarray = new byte[count];
                            Buffer.BlockCopy(receiveBuffer, endIndex + messageEndPattern.Length, newarray, 0, count);
                            receiveBuffer = newarray;
                            startIndex = 0;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return messages;
        }

        public static IList<byte[]> GetMessagesWithHeaderLengthToEnqueue(ref byte[] receiveBuffer, int totalHeaderLength, int iLength, int lengthAdjust)
        {
            if (receiveBuffer is null)
            {
                throw new ArgumentNullException(nameof(receiveBuffer));
            }

            IList<byte[]> messages = new List<byte[]>();
            if (receiveBuffer.Length >= totalHeaderLength + lengthAdjust)
            {
                var startIndex = 0;
                while (startIndex != -1 && receiveBuffer.Length > 0)
                {
                    var messageLength = receiveBuffer[startIndex + iLength] + lengthAdjust;

                    // check if there is a message to extract
                    if (messageLength > 0 && receiveBuffer.Length >= messageLength)
                    {
                        // Cut message from raw buffer and enqueue it
                        var message = new byte[messageLength];
                        Array.Copy(receiveBuffer, startIndex, message, 0, message.Length);
                        messages.Add(message);

                        // Remove message from raw buffer
                        var count = receiveBuffer.Length - messageLength;
                        var newarray = new byte[count];
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
