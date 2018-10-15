using System;

namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    /// The adapter type code.
    /// It is related to the hardware device.
    /// Hardware device supported: { Bonfiglioli, ... }
    /// The inverter uses a proprietary interface (Vectron) with well-defined commands/parameters.
    /// </summary>
    public enum AdapterType
    {
        /// <summary>
        /// Transparent adapter (Use with Bonfiglioli inverter).
        /// </summary>
        Vectron = 0x00,

        /// <summary>
        /// PLC + inverter.
        /// </summary>
        CustomAdapter = 0x01,
    }

    /// <summary>
    /// Command Id codes.
    /// </summary>
    public enum CommandId
    {
        /// <summary>
        /// Set Vertical axis origin.
        /// </summary>
        SetVerticalAxisOrigin = 0x00,

        /// <summary>
        /// Move along vertical axis to point.
        /// </summary>
        MoveAlongVerticalAxisToPoint = 0x01,

        /// <summary>
        /// Select type of motor movement (horizontal/vertical).
        /// </summary>
        SetTypeOfMotorMovement = 0x02,

        /// <summary>
        /// Move with given profile along horizontal axis.
        /// </summary>
        MoveAlongHorizontalAxisWithProfile = 0x03,

        /// <summary>
        /// Run the bay shutter (open/close).
        /// </summary>
        RunShutter = 0x05,

        /// <summary>
        /// Run weight detection routine.
        /// </summary>
        RunDrawerWeightRoutine = 0x06,

        /// <summary>
        /// Get drawer weight.
        /// </summary>
        GetDrawerWeight = 0x07,

        /// <summary>
        /// Stop.
        /// </summary>
        Stop = 0x08,

        /// <summary>
        /// Get main state of inverter.
        /// </summary>
        GetMainState = 0x09,

        /// <summary>
        /// Get IO sensors state.
        /// </summary>
        GetIOState = 0x0A,

        /// <summary>
        /// Get IO emergency sensors state.
        /// </summary>
        GetIOEmergencyState = 0x0B,

        /// <summary>
        /// Set custom command.
        /// </summary>
        Set = 0x0C,

        /// <summary>
        /// None.
        /// </summary>
        None = 0xFF
    }

    /// <summary>
    /// Exit status codes for command.
    /// </summary>
    public enum ExitStatus
    {
        /// <summary>
        /// Successful operation
        /// </summary>
        Success = 0x00,

        /// <summary>
        /// Invalid code
        /// </summary>
        InvalidCode,

        /// <summary>
        /// Invalid argument
        /// </summary>
        InvalidArgument,

        /// <summary>
        /// Invalid operation
        /// </summary>
        InvalidOperation,

        /// <summary>
        /// Generic hardware failure
        /// </summary>
        Failure = 0xFF
    }

    /// <summary>
    /// Command base class.
    /// </summary>
    internal class CommandBase : ICommandBase
    {
        #region Fields

        private readonly CommandId cmdId;

        #endregion Fields



        #region Constructors

        /// <summary>
        /// Class c-tor.
        /// </summary>
        /// <param name="id"><see cref="CommandId"/> code</param>
        public CommandBase(CommandId id)
        {
            this.cmdId = id;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Get the command Id.
        /// </summary>
        public CommandId CmdId => this.cmdId;

        #endregion Properties
    }

    /// <summary>
    /// Get the drawer weight command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class GetDrawerWeightCmd : CommandBase
    {
        #region Fields

        private static readonly GetDrawerWeightCmd instance7 = new GetDrawerWeightCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="GetDrawerWeight"/> class.
        /// </summary>
        private GetDrawerWeightCmd() : base(CommandId.GetDrawerWeight) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static GetDrawerWeightCmd Instance => instance7;

        public float Current => 0.0f;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
                short ic = 0x00;

                // ic
                var convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(ic);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.GetDrawerWeight));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Get I/O emergency sensors status command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class GetIOEmergencyStateCmd : CommandBase
    {
        #region Fields

        private static readonly GetIOEmergencyStateCmd instance_B = new GetIOEmergencyStateCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="GetMainStateCmd"/> class.
        /// </summary>
        private GetIOEmergencyStateCmd() : base(CommandId.GetIOEmergencyState) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static GetIOEmergencyStateCmd Instance => instance_B;

        public int[] IOEmergencyState => null;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// // /// <param name="state">4 bytes mask bits</param>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.GetIOEmergencyState));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Get I/O sensors status command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class GetIOStateCmd : CommandBase
    {
        #region Fields

        private static readonly GetIOStateCmd instance_A = new GetIOStateCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="GetMainStateCmd"/> class.
        /// </summary>
        private GetIOStateCmd() : base(CommandId.GetIOState) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static GetIOStateCmd Instance => instance_A;

        public int[] IOState => null;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.GetIOState));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Get main status command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class GetMainStateCmd : CommandBase
    {
        #region Fields

        private static readonly GetMainStateCmd instance9 = new GetMainStateCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="GetMainStateCmd"/> class.
        /// </summary>
        private GetMainStateCmd() : base(CommandId.GetMainState) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static GetMainStateCmd Instance => instance9;

        public byte State => 0x00;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(ref byte[] ans, out int nBytes)
        {
            nBytes = 0;

            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.GetMainState));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Move along horizontal axis with profile command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class MoveAlongHorizontalAxisWithProfileCmd : CommandBase
    {
        #region Fields

        private static readonly MoveAlongHorizontalAxisWithProfileCmd instance3 = new MoveAlongHorizontalAxisWithProfileCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SetVerticalAxisOriginCmd"/> class.
        /// </summary>
        private MoveAlongHorizontalAxisWithProfileCmd() : base(CommandId.MoveAlongHorizontalAxisWithProfile) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static MoveAlongHorizontalAxisWithProfileCmd Instance => instance3;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// NOTE: the profile is stored in the internal memory of device Inverter. Specify an input argument parameter to use this parameter directly from device inverter (?)
        /// </summary>
        /// <param name="v1">Parameters to describe the profile.</param>
        /// <param name="a"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="v2"></param>
        /// <param name="a1"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="v3"></param>
        /// <param name="a2"></param>
        /// <param name="s5"></param>
        /// <param name="s6"></param>
        /// <param name="a3"></param>
        /// <param name="s7"></param>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(float v1, float a, short s1, short s2, float v2, float a1, short s3, short s4, float v3, float a2, short s5, short s6, float a3, short s7, ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
                var convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(v1);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(s1);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);

                convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(s2);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a1);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a2);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(s3);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);

                convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(s4);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(v3);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a2);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(s5);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);

                convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(s6);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a3);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(s7);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var currIndex = 0;
                        var direction = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var v1 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var s1 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        var s2 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        var v2 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a1 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var s3 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        var s4 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        var v3 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a2 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var s5 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        var s6 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        var a3 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var s7 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);

                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.MoveAlongHorizontalAxisWithProfile));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Move along vertical axis to point command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class MoveAlongVerticalAxisToPointCmd : CommandBase
    {
        #region Fields

        private static readonly MoveAlongVerticalAxisToPointCmd instance1 = new MoveAlongVerticalAxisToPointCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="MoveAlongVerticalAxisToPointCmd"/> class.
        /// </summary>
        private MoveAlongVerticalAxisToPointCmd() : base(CommandId.MoveAlongVerticalAxisToPoint) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static MoveAlongVerticalAxisToPointCmd Instance => instance1;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="vMax"></param>
        /// <param name="a"></param>
        /// <param name="a1"></param>
        /// <param name="w"></param>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(short x, float vMax, float a, float a1, float w, ref byte[] ans, out int nBytes)
        {
            nBytes = 0;

            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
                var convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(x);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(vMax);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a1);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(w);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }
            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var currIndex = 0;
                        short x = ans[currIndex];
                        currIndex += sizeof(short);
                        var vMax = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a1 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var w = BitConverter.ToSingle(ans, currIndex);

                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.MoveAlongVerticalAxisToPoint));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Run routine to measure the drawer weight command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class RunDrawerWeightRoutineCmd : CommandBase
    {
        #region Fields

        private static readonly RunDrawerWeightRoutineCmd instance6 = new RunDrawerWeightRoutineCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="RunDrawerWeightRoutineCmd"/> class.
        /// </summary>
        private RunDrawerWeightRoutineCmd() : base(CommandId.RunDrawerWeightRoutine) { this.Executed = false; }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static RunDrawerWeightRoutineCmd Instance => instance6;

        /// <summary>
        /// <c>True</c> if command has been executed.
        /// </summary>
        public bool Executed
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="w"></param>
        /// <param name="a"></param>
        /// <param name="e"></param>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(short d, float w, float a, byte e, ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
                var convertionBuffer = new byte[sizeof(short)];
                convertionBuffer = BitConverter.GetBytes(d);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(short);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(w);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(e);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var currIndex = 0;
                        var d = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        var w = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var e = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);

                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.RunDrawerWeightRoutine));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Run bay shutter command class.
    /// Two movements are available: open shutter and close shutter.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class RunShutterCmd : CommandBase
    {
        #region Fields

        private static readonly RunShutterCmd instance5 = new RunShutterCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="RunShutterCmd"/> class.
        /// </summary>
        private RunShutterCmd() : base(CommandId.RunShutter) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static RunShutterCmd Instance => instance5;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(byte m, ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
                var convertionBuffer = new byte[1];
                convertionBuffer = BitConverter.GetBytes(m);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(byte);
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a given adapter interface.
        /// TO REMOVE
        /// </summary>
        /// <param name="type"></param>
        /// <param name="m"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <returns></returns>
        public ExitStatus Execute(AdapterType type, byte m, out byte[] ans, out int nBytes)
        {
            ans = null;
            nBytes = 0;

            var exitCode = ExitStatus.Success;

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var inputBuffer = new byte[40];
                        nBytes = 40;

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var currIndex = 0;
                        var m = ans[currIndex];

                        var posx = 0;
                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.RunShutter));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Select movement command class.
    /// The movements available are: horizontal movement, vertical movement.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class SelectMovementCmd : CommandBase
    {
        #region Fields

        private static readonly SelectMovementCmd instance2 = new SelectMovementCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SelectMovementCmd"/> class.
        /// </summary>
        private SelectMovementCmd() : base(CommandId.SetTypeOfMotorMovement) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static SelectMovementCmd Instance => instance2;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(byte m, ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;

            var posx = 0;

            try
            {
                // m
                var convertionBuffer = new byte[sizeof(byte)];
                convertionBuffer = BitConverter.GetBytes(m);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(byte);
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var currIndex = 0;
                        var m = ans[currIndex];
                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.SetTypeOfMotorMovement));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Set command class.
    /// It is ON/OFF command type
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class SetCmd : CommandBase
    {
        #region Fields

        private static readonly SetCmd instance_C = new SetCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SetCmd"/> class.
        /// </summary>
        private SetCmd() : base(CommandId.Set) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static SetCmd Instance => instance_C;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="value"></param>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(int i, byte value, ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
                // i
                var convertionBuffer = new byte[sizeof(int)];
                convertionBuffer = BitConverter.GetBytes(i);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(int);

                // value
                convertionBuffer = new byte[1];
                convertionBuffer = BitConverter.GetBytes(value);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(byte);
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }
            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var currIndex = 0;
                        var i = BitConverter.ToInt32(ans, currIndex);
                        currIndex += sizeof(int);
                        var value = ans[currIndex];

                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.Set));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Set Vertical Axis Origin command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class SetVerticalAxisOriginCmd : CommandBase
    {
        #region Fields

        private static readonly SetVerticalAxisOriginCmd instance0 = new SetVerticalAxisOriginCmd();

        private bool executed = false;

        #endregion Fields



        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SetVerticalAxisOriginCmd"/> class.
        /// </summary>
        private SetVerticalAxisOriginCmd() : base(CommandId.SetVerticalAxisOrigin) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static SetVerticalAxisOriginCmd Instance => instance0;

        /// <summary>
        /// <c>True</c> if command instance has been executed.
        /// </summary>
        public bool Executed => this.executed;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <param name="vSearch"></param>
        /// <param name="vCam0"></param>
        /// <param name="a"></param>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(byte direction, float vSearch, float vCam0, float a, float a1, float a2, ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;

            var posx = 0;

            try
            {
                var convertionBuffer = new byte[sizeof(byte)];
                convertionBuffer = BitConverter.GetBytes(direction);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(byte);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(vSearch);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(vCam0);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a1);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);

                convertionBuffer = new byte[sizeof(float)];
                convertionBuffer = BitConverter.GetBytes(a2);
                convertionBuffer.CopyTo(ans, posx);
                posx += sizeof(float);
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }

            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var currIndex = 0;
                        var direction = ans[currIndex];
                        currIndex += sizeof(byte);
                        var vSearch = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var vCam0 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a1 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        var a2 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);

                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.SetVerticalAxisOrigin));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        this.executed = true;

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }

    /// <summary>
    /// Stop command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class StopCmd : CommandBase
    {
        #region Fields

        private static readonly StopCmd instance8 = new StopCmd();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="StopCmd"/> class.
        /// </summary>
        private StopCmd() : base(CommandId.Stop) { }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static StopCmd Instance => instance8;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cache data for the command in a bytes array.
        /// </summary>
        /// <param name="ans">Data buffer</param>
        /// <param name="nBytes">Number of written bytes</param>
        /// <returns></returns>
        public ExitStatus CacheData(ref byte[] ans, out int nBytes)
        {
            nBytes = 0;
            if (null == ans)
            {
                return ExitStatus.InvalidArgument;
            }

            var exitCode = ExitStatus.Success;
            var posx = 0;

            try
            {
            }
            catch (Exception)
            {
                exitCode = ExitStatus.Failure;
            }
            nBytes = posx;

            return exitCode;
        }

        /// <summary>
        /// Translate the command in a well-formatted array of bytes according to the given adapter interface.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ans"></param>
        /// <param name="nBytes"></param>
        /// <param name="outData"></param>
        /// <returns></returns>
        public ExitStatus Translate(AdapterType type, byte[] ans, int nBytes, ref byte[] outData)
        {
            if (null == ans && nBytes > 0)
            {
                return ExitStatus.InvalidArgument;
            }

            switch (type)
            {
                case AdapterType.Vectron:
                    {
                        var posx = 0;

                        var convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.Stop));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        byte header = 0x61;
                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(header);
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(nBytes));
                        convertionBuffer.CopyTo(outData, posx);
                        posx += sizeof(byte);

                        break;
                    }

                case AdapterType.CustomAdapter:
                    {
                        break;
                    }
                default:
                    break;
            }

            return ExitStatus.Success;
        }

        #endregion Methods
    }
}
