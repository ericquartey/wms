namespace Ferretto.VW.App.Services
{
    public interface INavigableView
    {
        #region Properties

        string ParentModuleName { get; set; }

        string ParentViewName { get; set; }

        #endregion
    }
}
