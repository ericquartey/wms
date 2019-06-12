namespace Ferretto.VW.MAS_DataLayer.Enumerations
{
    public enum SetupStatus : long
    {
        Undefined = 0L,

        VerticalHomingDone = 1L, // INFO Origine_Asse_Y

        HorizontalHomingDone = 2L, // INFO Origine_Asse_Z

        BeltBurnishingDone = 3L, // INFO Rodaggio cinghia

        VerticalResolutionDone = 4L, // INFO Set risoluzione Y

        VerticalOffsetDone = 5L, // INFO Verifica Offset Y

        CellsControlDone = 6L, // INFO Controllo celle

        PanelsControlDone = 7L, // INFO Controllo pannelli mensola

        Shape1Done = 8L, // INFO Sagoma_1_OK

        Shape2Done = 9L, // INFO Sagoma_2_OK

        Shape3Done = 10L, // INFO Sagoma_3_OK

        WeightMeasurementDone = 11L, // INFO Controllo peso

        Shutter1Done = 12L, // INFO OK_Serranda_1

        Shutter2Done = 13L, // INFO OK_Serranda_2

        Shutter3Done = 14L, // INFO OK_Serranda_3

        Bay1ControlDone = 15L, // INFO Controllo baia 1

        Bay2ControlDone = 16L, // INFO Controllo baia 2

        Bay3ControlDone = 17L, // INFO Controllo baia 3

        FirstDrawerLoadDone = 18L, // INFO Primo cassetto caricato

        DrawersLoadedDone = 19L, // INFO Caricati cassetti vuoti

        Laser1Done = 20L, // INFO Laser_1_OK

        Laser2Done = 21L, // INFO Laser_2_OK

        Laser3Done = 22L, // INFO Laser_3_OK

        MachineDone = 23L, // Macchina_OK
    }
}
