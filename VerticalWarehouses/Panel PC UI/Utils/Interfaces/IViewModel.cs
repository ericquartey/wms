namespace Ferretto.VW.Utils.Interfaces
{
    public interface IViewModel
    {
        #region Methods

        void ExitFromViewMethod();

        void OnEnterView();

        void UnSubscribeMethodFromEvent();

        #endregion
    }
}
