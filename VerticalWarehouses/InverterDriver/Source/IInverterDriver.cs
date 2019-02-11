using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InverterDriver
{
    public interface IInverterDriver
    {
        #region Properties
        
        bool Initialize();
       
        bool Brake_Resistance_Overtemperature { get; }
        bool Emergency_Stop { get; }
        bool Pawl_Sensor_Zero { get; }
        bool Udc_Presence_Cradle_Machine { get; }
        bool Udc_Presence_Cradle_Operator { get; }
        bool Enable_Update_Current_Position_Horizontal_Shaft_Mode { get; set; }
        bool Get_Status_Word_Enable { get; set; }
        int Current_Position_Horizontal_Shaft { get; }
        int Current_Position_Vertical_Shaft { get; }
        BitArray Status_Word { get; }
        ActionType CurrentActionType { get; set; }
        bool Enable_Update_Current_Position_Vertical_Shaft_Mode { get; set; }

        #endregion Properties

        #region Events

        event SelectTelegramDoneEventHandler SelectTelegramDone_CalibrateVerticalAxis;
        event EnquiryTelegramDoneEventHandler EnquiryTelegramDone_CalibrateVerticalAxis;
        event EnquiryTelegramDoneEventHandler EnquiryTelegramDone_PositioningDrawer;
        event SelectTelegramDoneEventHandler SelectTelegramDone_PositioningDrawer;

        #endregion Events

        #region Method

        void Terminate();
        InverterDriverExitStatus SettingRequest(ParameterID paramID, byte systemIndex, byte dataSetIndex, object value);
        InverterDriverExitStatus SendRequest(ParameterID paramID, byte systemIndex, byte dataSetIndex);

        #endregion Method


    }
}
