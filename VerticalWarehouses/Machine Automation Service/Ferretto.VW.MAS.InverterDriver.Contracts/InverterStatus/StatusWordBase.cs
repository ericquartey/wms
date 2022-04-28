namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public class StatusWordBase : IStatusWord
    {
        #region Fields

        private readonly object syncRoot = new object();

        private ushort statusWord;

        #endregion

        #region Constructors

        public StatusWordBase()
        {
        }

        public StatusWordBase(ushort statusWordValue)
        {
            this.statusWord = statusWordValue;
        }

        #endregion

        #region Properties

        public bool IsFault
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x0008) > 0;
                }
            }
        }

        public bool IsOperationEnabled
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x0004) > 0;
                }
            }
        }

        public bool IsQuickStopTrue
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x00200) > 0;
                }
            }
        }

        public bool IsReadyToSwitchOn
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x0001) > 0;
                }
            }
        }

        public bool IsRemote
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x0200) > 0;
                }
            }
        }

        public bool IsSwitchedOn
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x0002) > 0;
                }
            }
        }

        public bool IsSwitchOnDisabled
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x0040) > 0;
                }
            }
        }

        public bool IsVoltageEnabled
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x0010) > 0;
                }
            }
        }

        public bool IsWarning
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x0080) > 0;
                }
            }
        }

        public bool IsWarning2
        {
            get
            {
                lock (this.syncRoot)
                {
                    return (this.statusWord & 0x8000) > 0;
                }
            }
        }

        public ushort Value
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.statusWord;
                }
            }

            set
            {
                lock (this.syncRoot)
                {
                    this.statusWord = value;
                }
            }
        }

        #endregion
    }
}
