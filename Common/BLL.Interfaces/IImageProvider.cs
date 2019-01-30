namespace Ferretto.Common.BLL.Interfaces
{
    public interface IImageProvider
    {
        #region Methods

        System.IO.Stream GetImage(string pathName);

        void SaveImage(string pathImage);

        #endregion Methods
    }
}
