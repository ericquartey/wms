using System;

namespace Ferretto.VW.MAS.SocketLink
{
    public class WarehouseNumberException : Exception
    {
        #region Constructors

        public WarehouseNumberException()
        {
        }

        public WarehouseNumberException(string message) : base(message)
        {
        }

        public WarehouseNumberException(string message, Exception inner) : base(message, inner)
        {
        }

        public WarehouseNumberException(string message, string wareHouseNumber) : this(message)
        {
            this.WarehouseNumber = wareHouseNumber;
        }

        #endregion

        #region Properties

        public string WarehouseNumber { get; }

        #endregion
    }
}
