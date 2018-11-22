using System.Windows.Media;
using Ferretto.Common.Modules.BLL;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    internal class WmsImageViewModel : Prism.Mvvm.BindableBase
    {
        #region Fields

        private readonly IImageProvider imageService;
        private ImageSource source;

        #endregion Fields

        #region Constructors

        public WmsImageViewModel()
        {
            this.imageService = ServiceLocator.Current.GetInstance<IImageProvider>();
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
