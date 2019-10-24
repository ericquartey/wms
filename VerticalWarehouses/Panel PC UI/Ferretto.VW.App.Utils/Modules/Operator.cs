﻿namespace Ferretto.VW.Utils.Modules
{
    public static class Operator
    {
        #region Fields

        public const string EMPTY = "EmptyViewModel";

        public const string OPERATORMENU = "OperatorMenuViewModel";

        #endregion

        #region Classes

        public static class ItemSearch
        {
            #region Fields

            public const string DETAIL = "ItemSearchDetailViewModel";

            public const string MAIN = "ItemSearchMainViewModel";

            #endregion
        }

        public static class Others
        {
            #region Fields

            public const string IMMEDIATEDRAWERCALL = "ImmediateDrawerCallViewModel";

            public const string NAVIGATION = "OthersNavigationViewModel";

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

                public const string NAVIGATION = "StatisticsNavigationViewModel";

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
