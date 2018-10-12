using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ferretto.VW.Utils;

namespace Ferretto.VW.InverterDriver
{

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
        /// Select movement (horizontal/vertical).
        /// </summary>
        SelectMovement = 0x02,
        /// <summary>
        /// Move with given profile along horizontal axis.
        /// </summary>
        MoveAlongHorizontalAxisWithProfile = 0x03,
        /// <summary>
        /// Horizontal axis move point to point.
        /// </summary>
        //x MoveAlongHorizontalAxisToPoint = 0x04,
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
        /// Get the height of drawer.
        /// See LightCurtainsDriver project
        /// </summary>
        //GetHeight = 0x0F,

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
    /// Command base class.
    /// </summary>
    internal class CommandBase : ICommandBase
    {
        private readonly CommandId cmdId;   

        /// <summary>
        /// Class c-tor.
        /// </summary>
        /// <param name="id"><see cref="CommandId"/> code</param>
        public CommandBase(CommandId id)
        {
            this.cmdId = id;
        }

        /// <summary>
        /// Get the command Id.
        /// </summary>
        public CommandId CmdId
        {
            get
            {
                return this.cmdId;
            }
        }

    } 

    /// <summary>
    /// Set Vertical Axis Origin command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class SetVerticalAxisOriginCmd : CommandBase
    {
        private static readonly SetVerticalAxisOriginCmd instance0 = new SetVerticalAxisOriginCmd();

        private bool executed = false;        

        /// <summary>
        /// Initialize a new instance of the <see cref="SetVerticalAxisOriginCmd"/> class.
        /// </summary>
        private SetVerticalAxisOriginCmd() : base(CommandId.SetVerticalAxisOrigin) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static SetVerticalAxisOriginCmd Instance
        {
            get { return instance0; }
        }

        /// <summary>
        /// <c>True</c> if command instance has been executed.
        /// </summary>
        public bool Executed
        {
            get { return this.executed; }
        }


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

            ExitStatus exitCode = ExitStatus.Success;

            int posx = 0;

            try
            {
               
                byte[] convertionBuffer = new byte[sizeof(byte)];
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
                        int currIndex = 0;
                        byte direction = ans[currIndex];
                        currIndex += sizeof(byte);
                        float vSearch = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float vCam0 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a1 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a2 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        
                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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

    } 

    /// <summary>
    /// Move along vertical axis to point command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class MoveAlongVerticalAxisToPointCmd : CommandBase
    {
        private static readonly MoveAlongVerticalAxisToPointCmd instance1 = new MoveAlongVerticalAxisToPointCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="MoveAlongVerticalAxisToPointCmd"/> class.
        /// </summary>
        private MoveAlongVerticalAxisToPointCmd() : base(CommandId.MoveAlongVerticalAxisToPoint) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static MoveAlongVerticalAxisToPointCmd Instance
        {
            get { return instance1; }
        }

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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

            try
            {
                byte[] convertionBuffer = new byte[sizeof(short)];
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
                        int currIndex = 0;
                        short x = ans[currIndex];
                        currIndex += sizeof(short);
                        float vMax = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a1 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float w = BitConverter.ToSingle(ans, currIndex);

                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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


    }

    /// <summary>
    /// Select movement command class.
    /// The movements available are: horizontal movement, vertical movement.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class SelectMovementCmd : CommandBase
    {
        private static readonly SelectMovementCmd instance2 = new SelectMovementCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="SelectMovementCmd"/> class.
        /// </summary>
        private SelectMovementCmd() : base(CommandId.SelectMovement) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static SelectMovementCmd Instance
        {
            get { return instance2; }
        }

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

            ExitStatus exitCode = ExitStatus.Success;

            int posx = 0;

            try
            {
                // m
                byte[] convertionBuffer = new byte[sizeof(byte)];
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
                        int currIndex = 0;
                        byte m = ans[currIndex];
                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
                        convertionBuffer = BitConverter.GetBytes(Convert.ToByte(CommandId.SelectMovement));
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

    } 

    /// <summary>
    /// Move along horizontal axis with profile command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class MoveAlongHorizontalAxisWithProfileCmd : CommandBase
    {
        private static readonly MoveAlongHorizontalAxisWithProfileCmd instance3 = new MoveAlongHorizontalAxisWithProfileCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="SetVerticalAxisOriginCmd"/> class.
        /// </summary>
        private MoveAlongHorizontalAxisWithProfileCmd() : base(CommandId.MoveAlongHorizontalAxisWithProfile) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static MoveAlongHorizontalAxisWithProfileCmd Instance
        {
            get { return instance3; }
        }

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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

            try
            {
                byte[] convertionBuffer = new byte[sizeof(float)];
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
                        int currIndex = 0;
                        float direction = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float v1 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        short s1 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        short s2 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        float v2 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a1 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        short s3 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        short s4 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        float v3 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a2 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        short s5 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        short s6 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        float a3 = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        short s7 = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);

                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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

    } 

    /// <summary>
    /// Run bay shutter command class.
    /// Two movements are available: open shutter and close shutter.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class RunShutterCmd : CommandBase
    {
        private static readonly RunShutterCmd instance5 = new RunShutterCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="RunShutterCmd"/> class.
        /// </summary>
        private RunShutterCmd() : base(CommandId.RunShutter) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static RunShutterCmd Instance
        {
            get { return instance5; }
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

            ExitStatus exitCode = ExitStatus.Success;

            switch (type)
            {
                case AdapterType.Vectron:
                    {

                        byte[] inputBuffer = new byte[40];
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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

            try
            {
                byte[] convertionBuffer = new byte[1];
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
                        int currIndex = 0;
                        byte m = ans[currIndex];

                        int posx = 0;
                        byte[] convertionBuffer = new byte[sizeof(byte)];
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

    } 
    /// <summary>
    /// Run routine to measure the drawer weight command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class RunDrawerWeightRoutineCmd : CommandBase
    {
        private static readonly RunDrawerWeightRoutineCmd instance6 = new RunDrawerWeightRoutineCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="RunDrawerWeightRoutineCmd"/> class.
        /// </summary>
        private RunDrawerWeightRoutineCmd() : base(CommandId.RunDrawerWeightRoutine) { this.Executed = false; }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static RunDrawerWeightRoutineCmd Instance
        {
            get { return instance6; }
        }

        /// <summary>
        /// <c>True</c> if command has been executed.
        /// </summary>
        public bool Executed
        {
            get;
            set;
        }

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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

            try
            {
                byte[] convertionBuffer = new byte[sizeof(short)];
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
                        int currIndex = 0;
                        short d = BitConverter.ToInt16(ans, currIndex);
                        currIndex += sizeof(short);
                        float w = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float a = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);
                        float e = BitConverter.ToSingle(ans, currIndex);
                        currIndex += sizeof(float);

                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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
    } 
    /// <summary>
    /// Get the drawer weight command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class GetDrawerWeightCmd : CommandBase
    {
        private static readonly GetDrawerWeightCmd instance7 = new GetDrawerWeightCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="GetDrawerWeight"/> class.
        /// </summary>
        private GetDrawerWeightCmd() : base(CommandId.GetDrawerWeight) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static GetDrawerWeightCmd Instance
        {
            get { return instance7; }
        }

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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

            try
            {
                short ic = 0x00;

                // ic
                byte[] convertionBuffer = new byte[sizeof(short)];
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

                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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


        public float Current
        {
            get { return 0.0f; }
        }
    } 

    /// <summary>
    /// Stop command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class StopCmd : CommandBase
    {
        private static readonly StopCmd instance8 = new StopCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="StopCmd"/> class.
        /// </summary>
        private StopCmd() : base(CommandId.Stop) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static StopCmd Instance
        {
            get { return instance8; }
        }

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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

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

                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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

    } 
    /// <summary>
    /// Get main status command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class GetMainStateCmd : CommandBase
    {
        private static readonly GetMainStateCmd instance9 = new GetMainStateCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="GetMainStateCmd"/> class.
        /// </summary>
        private GetMainStateCmd() : base(CommandId.GetMainState) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static GetMainStateCmd Instance
        {
            get { return instance9; }
        }

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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

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

                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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


        public byte State
        {

            get { return 0x00; }
        }

    } 
    /// <summary>
    /// Get I/O sensors status command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class GetIOStateCmd : CommandBase
    {
        private static readonly GetIOStateCmd instance_A = new GetIOStateCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="GetMainStateCmd"/> class.
        /// </summary>
        private GetIOStateCmd() : base(CommandId.GetIOState) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static GetIOStateCmd Instance
        {
            get { return instance_A; }
        }

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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

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
                        
                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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


        public int[] IOState
        {
            get { return null; }
        }

    }
    /// <summary>
    /// Get I/O emergency sensors status command class.
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class GetIOEmergencyStateCmd : CommandBase
    {
        private static readonly GetIOEmergencyStateCmd instance_B = new GetIOEmergencyStateCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="GetMainStateCmd"/> class.
        /// </summary>
        private GetIOEmergencyStateCmd() : base(CommandId.GetIOEmergencyState) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static GetIOEmergencyStateCmd Instance
        {
            get { return instance_B; }
        }

        
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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

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

                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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


        public int[] IOEmergencyState
        {
            get { return null; }
        }

    } 
    /// <summary>
    /// Set command class.
    /// It is ON/OFF command type
    /// It inherits from <see cref="CommandBase"/> class.
    /// </summary>
    internal class SetCmd : CommandBase
    {
        private static readonly SetCmd instance_C = new SetCmd();

        /// <summary>
        /// Initialize a new instance of the <see cref="SetCmd"/> class.
        /// </summary>
        private SetCmd() : base(CommandId.Set) { }

        /// <summary>
        /// Gets the instance
        /// </summary>
        public static SetCmd Instance
        {
            get { return instance_C; }
        }

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

            ExitStatus exitCode = ExitStatus.Success;
            int posx = 0;

            try
            {
                // i
                byte[] convertionBuffer = new byte[sizeof(int)];
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
                        int currIndex = 0;
                        int i = BitConverter.ToInt32(ans, currIndex);
                        currIndex += sizeof(int);
                        byte value = ans[currIndex];

                        int posx = 0;

                        byte[] convertionBuffer = new byte[sizeof(byte)];
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

    } 

}
