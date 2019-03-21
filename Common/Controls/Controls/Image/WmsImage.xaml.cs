using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Xpf.Layout.Core;
using Ferretto.Common.BLL.Interfaces.Providers;
using CommonServiceLocator;

namespace Ferretto.Common.Controls
{
    public partial class WmsImage : UserControl
    {
        #region Fields

        public static readonly DependencyProperty HasImageProperty = DependencyProperty.Register(
         nameof(HasImage), typeof(bool), typeof(WmsImage), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
                 nameof(Path), typeof(string), typeof(WmsImage), new PropertyMetadata(default(string), new PropertyChangedCallback(OnPathChanged)));

        private readonly IImageFileProvider imageService;

        #endregion

        #region Constructors

        public WmsImage()
        {
            this.InitializeComponent();

            this.imageService = ServiceLocator.Current.GetInstance<IImageFileProvider>();
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

        #region Methods

        private static async void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsImage wmsImage)
            {
                wmsImage.InnerImage.Source = await ImageUtils.RetrieveImage(wmsImage.imageService, (string)e.NewValue);
                wmsImage.HasImage = e.NewValue != null;
            }
        }

        #endregion
    }
}
