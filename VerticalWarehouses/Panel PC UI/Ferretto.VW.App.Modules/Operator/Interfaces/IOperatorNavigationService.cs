namespace Ferretto.VW.App.Modules.Operator
{
    public interface IOperatorNavigationService
    {
        #region Methods

        void NavigateToDrawerView();

        void NavigateToDrawerViewConfirmPresent();

        void NavigateToOperationOrGoBack();

        void NavigateToOperatorMenuAsync();

        #endregion
    }
}
