using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Xpf.Layout.Core;
using Ferretto.Common.BLL.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public partial class WmsImage : UserControl
    {
        #region Fields

        public static readonly DependencyProperty HasImageProperty = DependencyProperty.Register(
         nameof(HasImage), typeof(bool), typeof(WmsImage), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
                 nameof(Path), typeof(string), typeof(WmsImage), new PropertyMetadata(default(string), new PropertyChangedCallback(OnPathChanged)));

        #endregion Fields

        private readonly IImageProvider imageService;

        #region Constructors

        public WmsImage()
        {
            this.InitializeComponent();

            this.InnerImage.DataContext = new WmsImageViewModel();

            this.imageService = ServiceLocator.Current.GetInstance<IImageProvider>();

        }

        #endregion Constructors

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

        #endregion Properties

        #region Methods

        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsImage wmsImage && wmsImage.InnerImage.DataContext is WmsImageViewModel viewModel)
            {
                wmsImage.InnerImage.Source = ImageUtils.RetrieveImage(wmsImage.imageService, (string)e.NewValue);
                if (e.NewValue == null)
                {
                    wmsImage.HasImage = false;
                }
                else
                {
                    wmsImage.HasImage = true;
                }
            }
        }

        #endregion Methods
    }
}
