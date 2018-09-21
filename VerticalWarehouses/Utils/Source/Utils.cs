using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Utils
{
    public class Utils
    { }

    /// <summary>
    /// My class implementation 
    /// </summary>
    public class CMyClass
    {
        const int NITEMS = 10;

        public int m_myVariable;

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public CMyClass()
        {
            this.m_myVariable = 0;
        }
    }
    public class CBufferStream
    {
        /// <summary>
        /// Data buffer 1 to store receiving stream (main)
        /// </summary>
        public byte[] DataBuff1 { get; }

        // Creo una Queue che salva i comandi ricevuti come bytes
        public Queue<string> CommandsStr;

        // Creo una Queue che salva i comandi ricevuti come bytes
        public Queue<byte[]> Commands;

        const int NMAX_SIZE = 100;

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public CBufferStream()
        {
            this.DataBuff1   = new byte[NMAX_SIZE];
            this.CommandsStr = new Queue<string>();
            this.Commands    = new Queue<byte[]>();
        }

    } // class CBufferStream

}
