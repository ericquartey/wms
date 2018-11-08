namespace Ferretto.VW.Navigation
{
    public static class NavigationService
    {
        #region Delegates

        public delegate void BackToVWAppEvent();

        public delegate void CellBlocksChangedEvent();

        public delegate void CellsChangedEvent();

        public delegate void DrawersChangedEvent();

        public delegate void ExitViewEvent();

        public delegate void GoToViewEvent();

        public delegate void InstallationInfoChangedEvent();

        #endregion Delegates

        #region Events

        public static event BackToVWAppEvent BackToVWAppEventHandler;

        public static event CellBlocksChangedEvent CellBlocksChangedEventHandler;

        public static event CellsChangedEvent CellsChangedEventHandler;

        public static event DrawersChangedEvent DrawersChangedEventHandler;

        public static event ExitViewEvent ExitViewEventHandler;

        public static event GoToViewEvent GoToViewEventHandler;

        public static event InstallationInfoChangedEvent InstallationInfoChangedEventHandler;

        #endregion Events

        #region Methods

        public static void InitializeEvents()
        {
            CellBlocksChangedEventHandler += InitializerMethod;
            CellsChangedEventHandler += InitializerMethod;
            DrawersChangedEventHandler += InitializerMethod;
            InstallationInfoChangedEventHandler += InitializerMethod;
            GoToViewEventHandler += InitializerMethod;
            ExitViewEventHandler += InitializerMethod;
        }

        public static void RaiseBackToVWAppEvent() => BackToVWAppEventHandler();

        public static void RaiseCellBlockChangedEvent() => CellBlocksChangedEventHandler();

        public static void RaiseCellsChangedEvent() => CellsChangedEventHandler();

        public static void RaiseDrawersChangedEvent() => DrawersChangedEventHandler();

        public static void RaiseExitViewEvent() => ExitViewEventHandler();

        public static void RaiseGoToViewEvent() => GoToViewEventHandler();

        public static void RaiseInstallationInfoChangedEvent() => InstallationInfoChangedEventHandler();

        private static void InitializerMethod()
        {
        }

        #endregion Methods
    }
}
