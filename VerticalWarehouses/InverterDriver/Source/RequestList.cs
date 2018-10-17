using System;
using System.Collections.Generic;

namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    /// This class contains a list of requests.
    /// According to the provided operation a well-defined list of requests is created.
    /// </summary>
    internal class RequestList
    {
        #region Fields

        private List<Request> list;

        #endregion Fields

        #region Constructors

        public RequestList()
        {
            this.list = new List<Request>();
        }

        #endregion Constructors

        #region Properties

        public int Count => this.list.Count;

        #endregion Properties

        #region Indexers

        public Request this[int index]
        {
            get
            {
                Request rq = null;
                try { rq = this.list[index]; }
                catch { return null; }

                return rq;
            }

            set => this.list[index] = value;
        }

        #endregion Indexers

        #region Methods

        public void Add(Request rq)
        {
            if (null == rq)
            {
                throw new ArgumentNullException();
            }

            this.list.Add(rq);
        }

        public void build_For_MoveAlongVerticalAxisToPoint_Operation(short x, float vMax, float acc, float dec, float w)
        {
            if (null != this.list)
            {
                this.list.Clear();
            }

            // Create the entire request list to send to inverter in order to perform the <MoveAlongVerticalAxisToPoint> operation
            this.list = new List<Request>();

            // Write the target position parameter
            var rq = new Request(TypeOfRequest.Write, Request.POSITION_TARGET_POSITION_PARAM);
            rq.DataType = ValueDataType.Int16;
            rq.ParameterValueInt16 = x;
            this.list.Add(rq);

            // Write the target speed parameter
            rq = new Request(TypeOfRequest.Write, Request.POSITION_TARGET_SPEED_PARAM);
            rq.DataType = ValueDataType.Float;
            rq.ParameterValueFloat = vMax;
            this.list.Add(rq);

            // Write the acceleration parameter
            rq = new Request(TypeOfRequest.Write, Request.POSITION_ACCELERATION_PARAM);
            rq.DataType = ValueDataType.Float;
            rq.ParameterValueFloat = acc;
            this.list.Add(rq);

            // Write the deceleration parameter
            rq = new Request(TypeOfRequest.Write, Request.POSITION_DECELERATION_PARAM);
            rq.DataType = ValueDataType.Float;
            rq.ParameterValueFloat = dec;
            this.list.Add(rq);

            // Write the operating mode parameter
            // ....
            // Read the actual operating mode parameter
            // ....
            // Write the control word parameter
            // ....
        }

        public void build_For_SetVerticalAxisOrigin_Operation(byte mode, float vSearch, float vCam0, float offset)
        {
            if (null != this.list)
            {
                this.list.Clear();
            }

            // Create the entire request list to send to inverter in order to perform the <SetVerticalAxisOrigin> operation
            this.list = new List<Request>();

            // Write the homing mode parameter
            var rq = new Request(TypeOfRequest.Write, Request.HOMING_MODE_PARAM);
            rq.DataType = ValueDataType.Byte;
            rq.ParameterValueByte = mode;
            this.list.Add(rq);

            // Write the offset parameter
            rq = new Request(TypeOfRequest.Write, Request.HOMING_OFFSET_PARAM);
            rq.DataType = ValueDataType.Float;
            rq.ParameterValueFloat = offset;
            this.list.Add(rq);

            // Write the fast speed parameter
            rq = new Request(TypeOfRequest.Write, Request.HOMING_FAST_SPEED_PARAM);
            rq.DataType = ValueDataType.Float;
            rq.ParameterValueFloat = vSearch;
            this.list.Add(rq);

            // Write the creep speed parameter
            rq = new Request(TypeOfRequest.Write, Request.HOMING_CREEP_SPEED_PARAM);
            rq.DataType = ValueDataType.Float;
            rq.ParameterValueFloat = vCam0;
            this.list.Add(rq);

            // Write the operating mode parameter
            // ....
            // Read the actual operating mode parameter
            // ....
            // Write the control word parameter
            // ....
        }

        public void Clear()
        {
            foreach (var rq in this.list) { }

            this.list.Clear();
        }

        public void RemoveAt(int index)
        {
            Request rq = null;
            try { rq = this.list[index]; }
            catch { }

            this.list.RemoveAt(index);
        }

        #endregion Methods
    }
}
