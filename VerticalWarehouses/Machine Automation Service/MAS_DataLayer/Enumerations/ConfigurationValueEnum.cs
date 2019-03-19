namespace Ferretto.VW.MAS_DataLayer
{
    public enum ConfigurationValueEnum : long
    {
        CellSpacing,

        HomingModeAcceleration,

        HomingCreepSpeed,

        HomingFastSpeed,

        InverterOperationTimeout,

        Resolution,

        Offset,

        InverterAddress,

        InverterPort,

        IoAddress,

        IoPort,

        PositioningOverrideTargetPosition,

        PositioningOverrideTargetSpeed,

        PositioningOverrideTargetAcceleration,

        PositioningOverrideTargetDeceleration,

        // INFO General Info parameters
        UpperBound,

        LowerBound,

        Address,

        Alfa_Num_1,

        Alfa_Num_2,

        Alfa_Num_3,

        Bays_Quantity,

        Carrying_Capacity,

        City,

        Client_Code,

        Client_Name,

        Country,

        Drawers,

        Height,

        Height_Bay_1_Position_1,

        Height_Bay_1_Position_2,

        Bay_1_Position_1,

        Bay_1_Position_2,

        Height_Bay_2_Position_1,

        Height_Bay_2_Position_2,

        Bay_2_Position_1,

        Bay_2_Position_2,

        Height_Bay_3_Position_1,

        Height_Bay_3_Position_2,

        Bay_3_Position_1,

        Bay_3_Position_2,

        Installation_Date,

        Laser_1,

        Laser_2,

        Laser_3,

        Latitude,

        Longitude,

        Machine_Number_In_Area,

        Model,

        Order, // INFO Commessa

        Province,

        Serial,

        Type_Bay_1,

        Type_Bay_2,

        Type_Bay_3,

        Type_Shutter_1,

        Type_Shutter_2,

        Type_Shutter_3,

        WMS_ON,

        // INFO Installation Info parameters
        Belt_Burnishing,

        Machine_Ok,

        Shutter_1_Ok,

        Shutter_2_Ok,

        Shutter_3_Ok,

        Laser_1_Ok,

        Laser_2_Ok,

        Laser_3_Ok,

        Shape_1_Ok,

        Shape_2_Ok,

        Shape_3_Ok,

        Weight_Check,

        Origin_Y_Axis,

        Origin_Z_Axis,

        Check_Y_Offset,

        Cells_Check,

        Check_Shelf_Panel,

        Check_Bay_1, // INFO Controllo baia 1

        Check_Bay_2, // INFO Controllo baia 2

        Check_Bay_3, // INFO Controllo baia 3

        Load_First_Drawer, // INFO The first drawer has passed all the tests

        Load_Empty_Drawers, // INFO All the drawers are loaded

        Set_Y_Resolution,
    }
}
