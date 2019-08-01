using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.StatusWord
{
    public class StatusWordBase : IStatusWord
    {
        #region Fields

        private readonly object statusWordLockObject;

        private ushort statusWord;

        #endregion

        #region Constructors

        public StatusWordBase()
        {
            this.statusWord = 0x0000;
            this.statusWordLockObject = new object();
        }

        public StatusWordBase(ushort statusWordValue)
        {
            this.statusWord = statusWordValue;
            this.statusWordLockObject = new object();
        }

        public StatusWordBase(StatusWordBase otherStatusWord)
        {
            this.statusWord = otherStatusWord.statusWord;
            this.statusWordLockObject = new object();
        }

        #endregion

        #region Properties

        public bool IsFault
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0008) > 0;
                }
            }
        }

        public bool IsOperationEnabled
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0004) > 0;
                }
            }
        }

        public bool IsQuickStopTrue
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0020) > 0;
                }
            }
        }

        public bool IsReadyToSwitchOn
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0001) > 0;
                }
            }
        }

        public bool IsRemote
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0200) > 0;
                }
            }
        }

        public bool IsSwitchedOn
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0002) > 0;
                }
            }
        }

        public bool IsSwitchOnDisabled
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0040) > 0;
                }
            }
        }

        public bool IsVoltageEnabled
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0010) > 0;
                }
            }
        }

        public bool IsWarning
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x0080) > 0;
                }
            }
        }

        public bool IsWarning2
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return (this.statusWord & 0x8000) > 0;
                }
            }
        }

        public ushort Value
        {
            get
            {
                lock (this.statusWordLockObject)
                {
                    return this.statusWord;
                }
            }
            set
            {
                lock (this.statusWordLockObject)
                {
                    this.statusWord = value;
                }
            }
        }

        #endregion
    }
}
