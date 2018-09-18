using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    internal class ImageViewModel : BindableBase
    {
        #region Fields

        private readonly IImageService imageService;
        private ImageSource source;

        #endregion Fields

        #region Constructors

        public ImageViewModel()
        {
            this.imageService = ServiceLocator.Current.GetInstance<IImageService>();
        }

        #endregion Constructors

        #region Properties

        public ImageSource Source
        {
            get => this.source;
            set => this.SetProperty(ref this.source, value);
        }

        #endregion Properties

        #region Methods

        public void RetrieveImage(string imagePath)
        {
            this.Source = string.IsNullOrWhiteSpace(imagePath) ? null : this.imageService.GetImage(imagePath);
        }

        #endregion Methods
    }
}
