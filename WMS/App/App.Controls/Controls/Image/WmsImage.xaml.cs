using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Providers;

namespace Ferretto.WMS.App.Controls
{
    public partial class WmsImage : UserControl
    {
        #region Fields

        public static readonly DependencyProperty HasImageProperty = DependencyProperty.Register(
            nameof(HasImage), typeof(bool), typeof(WmsImage), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            nameof(Path),
            typeof(string),
            typeof(WmsImage),
            new PropertyMetadata(
                default(string),
                async (d, e) =>
                {
                    if (d is WmsImage wmsImage)
                    {
                        wmsImage.InnerImage.Source = await ImageUtils
                            .GetImageAsync(wmsImage.imageService, (string)e.NewValue)
                            .ConfigureAwait(true);
                        wmsImage.HasImage = e.NewValue != null;
                    }
                }));

        private readonly IFileProvider imageService;

        #endregion

        #region Constructors

        public WmsImage()
        {
            this.InitializeComponent();

            this.imageService = ServiceLocator.Current.GetInstance<IFileProvider>();
        }

        #endregion

        #region Properties

        public bool HasImage
        {
            get => (bool)this.GetValue(HasImageProperty);
            set => this.SetValue(HasImageProperty, value);
        }

        public string Path
        {
            get => (string)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        #endregion
    }
}
