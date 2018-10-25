namespace Ferretto.VW.Navigation
{
    public static class NavigationService
    {
        #region Delegates

        public delegate void BackToVWAppEvent();

        public delegate void CellBlocksChangedEvent();

        public delegate void CellsChangedEvent();

        public delegate void DrawersChangedEvent();

        public delegate void InstallationInfoChangedEvent();

        #endregion Delegates

        #region Events

        public static event BackToVWAppEvent BackToVWAppEventHandler;

        public static event CellBlocksChangedEvent CellBlocksChangedEventHandler;

        public static event CellsChangedEvent CellsChangedEventHandler;

        public static event DrawersChangedEvent DrawersChangedEventHandler;

        public static event InstallationInfoChangedEvent InstallationInfoChangedEventHandler;

        #endregion Events

        #region Methods

        public static void RaiseBackToVWAppEvent() => BackToVWAppEventHandler();

        public static void RaiseCellBlockChangedEvent() => CellBlocksChangedEventHandler();

        public static void RaiseCellsChangedEvent() => CellsChangedEventHandler();

        public static void RaiseDrawersChangedEvent() => DrawersChangedEventHandler();

        public static void RaiseInstallationInfoChangedEvent() => InstallationInfoChangedEventHandler();

        #endregion Methods
    }
}
