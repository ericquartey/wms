using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Ferretto.VW.InverterDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MAS_InverterDriverUnitTests
{
    [TestClass]
    public class InverterMessageUnitTests
    {
        #region Fields

        private InverterMessage readMessage;

        private InverterMessage writeByteMessage;

        private InverterMessage writeDoubleMessage;

        private InverterMessage writeFloatMessage;

        private InverterMessage writeIntMessage;

        private InverterMessage writeShortMessage;

        private InverterMessage writeStringMessage;

        private InverterMessage writeUnsignedShortMessage;

        #endregion

        #region Methods

        [TestInitialize]
        public void ConfigureMessages()
        {
            this.readMessage = new InverterMessage( 0x01, 0x02, 3 );
            this.writeByteMessage = new InverterMessage( 0x01, 0x02, 3, (byte)16 );
            this.writeShortMessage = new InverterMessage( 0x01, 0x02, 3, (short)16 );
            this.writeUnsignedShortMessage = new InverterMessage( 0x01, 0x02, 3, (ushort)16 );
            this.writeIntMessage = new InverterMessage( 0x01, 0x02, 3, (int)16 );
            this.writeFloatMessage = new InverterMessage( 0x01, 0x02, 3, (float)16 );
            this.writeDoubleMessage = new InverterMessage( 0x01, 0x02, 3, (double)16 );
            this.writeStringMessage = new InverterMessage( 0x01, 0x02, 3, "16" );
        }

        [TestMethod]
        public void GetByteInverterWriteMessage()
        {
            var writeMessage = this.writeByteMessage.GetWriteMessage();

            Assert.AreEqual( 7, writeMessage.Length );
            Assert.AreEqual( 0x80, writeMessage[0] );
            Assert.AreEqual( 0x05, writeMessage[1] );
            Assert.AreEqual( 0x01, writeMessage[2] );
            Assert.AreEqual( 0x02, writeMessage[3] );
            Assert.AreEqual( 0x03, writeMessage[4] );
            Assert.AreEqual( 0x00, writeMessage[5] );
            Assert.AreEqual( 0x10, writeMessage[6] );
        }

        [TestMethod]
        public void GetIntegerInverterWriteMessage()
        {
            var writeMessage = this.writeIntMessage.GetWriteMessage();

            Assert.AreEqual( 10, writeMessage.Length );
            Assert.AreEqual( 0x80, writeMessage[0] );
            Assert.AreEqual( 0x08, writeMessage[1] );
            Assert.AreEqual( 0x01, writeMessage[2] );
            Assert.AreEqual( 0x02, writeMessage[3] );
            Assert.AreEqual( 0x03, writeMessage[4] );
            Assert.AreEqual( 0x00, writeMessage[5] );
            Assert.AreEqual( 0x10, writeMessage[6] );
            Assert.AreEqual( 0x00, writeMessage[7] );
            Assert.AreEqual( 0x00, writeMessage[8] );
            Assert.AreEqual( 0x00, writeMessage[9] );
        }

        [TestMethod]
        public void GetInverterReadMessage()
        {
            var readMessage = this.readMessage.GetReadMessage();

            Assert.AreEqual( 6, readMessage.Length );

            Assert.AreEqual( 0x00, readMessage[0] );
            Assert.AreEqual( 0x04, readMessage[1] );
            Assert.AreEqual( 0x01, readMessage[2] );
            Assert.AreEqual( 0x02, readMessage[3] );
            Assert.AreEqual( 0x03, readMessage[4] );
            Assert.AreEqual( 0x00, readMessage[5] );
        }

        [TestMethod]
        public void ParseErrorRawMessage()
        {
            byte[] rawMessage = new byte[] { 0x20, 0x07, 0x01, 0x02, 0x9A, 0x01, 0x01, 0x00 };  //VALUE 0x9A, 0x01 represent InverterParameterId.ControlWordParam that is a UInt16 type parameter

            InverterMessage parsedMessage = new InverterMessage( rawMessage );

            Assert.IsTrue( parsedMessage.IsError );
            Assert.IsInstanceOfType( parsedMessage.Payload, typeof( UInt16 ) );
            Assert.AreEqual( (ushort)1, parsedMessage.Payload );
        }

        [TestMethod]
        public void ParseRawMessage()
        {
            byte[] rawMessage = new byte[] { 0x40, 0x07, 0x01, 0x02, 0x03, 0x00, 0x01 };

            InverterMessage parsedMessage = new InverterMessage( rawMessage );

            Assert.IsFalse( parsedMessage.IsError );
        }

        #endregion
    }
}
