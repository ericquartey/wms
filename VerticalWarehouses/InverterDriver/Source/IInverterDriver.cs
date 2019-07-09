using System.Collections;

namespace Ferretto.VW.Drivers.Inverter
{
    public interface IInverterDriver
    {
        #region Events

        event System.EventHandler<EnquiryTelegramDoneEventArgs> EnquiryTelegramDone_CalibrateVerticalAxis;

        event System.EventHandler<EnquiryTelegramDoneEventArgs> EnquiryTelegramDone_PositioningDrawer;

        event System.EventHandler<SelectTelegramDoneEventArgs> SelectTelegramDone_CalibrateVerticalAxis;

        event System.EventHandler<SelectTelegramDoneEventArgs> SelectTelegramDone_PositioningDrawer;

        #endregion

        #region Properties

        bool Brake_Resistance_Overtemperature { get; }

        int Current_Position_Horizontal_Shaft { get; }

        int Current_Position_Vertical_Shaft { get; }

        ActionType CurrentActionType { get; set; }

        bool Emergency_Stop { get; }

        bool Enable_Update_Current_Position_Horizontal_Shaft_Mode { get; set; }

        bool Enable_Update_Current_Position_Vertical_Shaft_Mode { get; set; }

        bool Get_Status_Word_Enable { get; set; }

        bool Pawl_Sensor_Zero { get; }

        BitArray Status_Word { get; }

        bool Udc_Presence_Cradle_Machine { get; }

        bool Udc_Presence_Cradle_Operator { get; }

        #endregion

        #region Methods

        bool Initialize();

        InverterDriverExitStatus SendRequest(ParameterId parameterId, byte systemIndex, byte dataSetIndex);

        InverterDriverExitStatus SettingRequest(ParameterId parameterId, byte systemIndex, byte dataSetIndex, object value);

        void Terminate();

        #endregion
    }
}
