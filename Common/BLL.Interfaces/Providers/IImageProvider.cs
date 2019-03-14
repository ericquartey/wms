namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IImageProvider
    {
        #region Methods

        System.IO.Stream GetImage(string pathName);

        void SaveImage(string originalPathImage);

        #endregion
    }
}
