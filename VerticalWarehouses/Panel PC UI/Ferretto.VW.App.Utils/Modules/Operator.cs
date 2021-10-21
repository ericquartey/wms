namespace Ferretto.VW.Utils.Modules
{
    public static class Operator
    {
        #region Fields

        public const string EMPTY = "EmptyViewModel";

        public const string OPERATOR_MENU = "OperatorMenuViewModel";

        #endregion

        #region Classes

        public static class About
        {
            #region Fields

            public const string ALARM = "AlarmViewModel";

            public const string ALARMSEXPORT = "AlarmsExportViewModel";

            public const string COUNTERS = "CountersViewModel";

            public const string DIAGNOSTICS = "DiagnosticsViewModel";

            public const string GENERAL = "GeneralViewModel";

            public const string LOGSEXPORT = "LogsExportViewModel";

            public const string NAVIGATION = "AboutMenuNavigationViewModel";

            public const string NETWORKADAPTERS = "NetworkAdaptersViewModel";

            public const string STATISTICS = "StatisticsViewModel";

            public const string USER = "UserViewModel";

            #endregion
        }

        public static class ItemOperations
        {
            #region Fields

            public const string ADD_DRAPERYITEM_INTO_LOADINGUNIT = "AddingItemDraperyToLoadingUnitViewModel";

            public const string ADDITEMINTOLOADINGUNIT = "AddingItemToLoadingUnitViewModel";

            public const string INVENTORY = "ItemInventoryViewModel";

            public const string INVENTORY_DETAILS = "ItemInventoryDetailsViewModel";

            public const string LOADING_UNIT = "LoadingUnitViewModel";

            public const string LOADING_UNIT_INFO = "LoadingUnitInfoViewModel";

            public const string PICK = "ItemPickViewModel";

            public const string PICK_DETAILS = "ItemPickDetailsViewModel";

            public const string PUT = "ItemPutViewModel";

            public const string PUT_DETAILS = "ItemPutDetailsViewModel";

            public const string SOCKETLINKOPERATION = "SocketLinkOperationViewModel";

            public const string WAIT = "ItemOperationWaitViewModel";

            public static string DRAPERYCONFIRM = "ItemDraperyConfirmViewModel";

            public static string SIGNALLINGDEFECT = "ItemSignallingDefectViewModel";

            public static string WEIGHT = "ItemWeightViewModel";

            public static string WEIGHT_UPDATE = "ItemWeightUpdateViewModel";

            #endregion
        }

        public static class ItemSearch
        {
            #region Fields

            public const string ITEM_DETAILS = "ItemSearchDetailViewModel";

            public const string MAIN = "ItemSearchMainViewModel";

            public const string UNITS = "ItemSearchUnitsViewModel";

            #endregion
        }

        public static class Others
        {
            #region Fields

            public const string CHANGELASEROFFSET = "ChangeLaserOffsetViewModel";

            public const string IMMEDIATELOADINGUNITCALL = "ImmediateLoadingUnitCallViewModel";

            public const string LOADINGUNITSMISSIONS = "LoadingUnitsMissionsViewModel";

            public const string NAVIGATION = "OthersNavigationViewModel";

            public const string OPERATIONONBAY = "OperationOnBayViewModel";

            #endregion

            #region Classes

            public static class DrawerCompacting
            {
                #region Fields

                public const string DETAIL = "DrawerCompactingDetailViewModel";

                public const string MAIN = "DrawerCompactingViewModel";

                #endregion
            }

            public static class Maintenance
            {
                #region Fields

                public const string DETAIL = "MaintenanceDetailViewModel";

                public const string MAIN = "MaintenanceViewModel";

                #endregion
            }

            public static class Statistics
            {
                #region Fields

                public const string CELLS = "StatisticsCellsViewModel";

                public const string ERRORS = "StatisticsErrorsViewModel";

                public const string MACHINE = "StatisticsMachineViewModel";

                public const string NAVIGATION = "StatisticsNavigationViewModel";

                #endregion

                #region Classes

                public static class Drawers
                {
                    #region Fields

                    public const string MAIN = "StatisticsDrawersViewModel";

                    public const string SPACESATURATION = "StatisticsSpaceSaturationViewModel";

                    public const string WEIGHTSATURATION = "StatisticsWeightSaturationViewModel";

                    #endregion
                }

                #endregion
            }

            #endregion
        }

        public static class WaitingLists
        {
            #region Fields

            public const string DETAIL = "WaitingListDetailViewModel";

            public const string MAIN = "WaitingListsViewModel";

            #endregion
        }

        #endregion
    }
}
