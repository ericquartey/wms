using System.IO;
using System.Windows;
using System.Windows.Media;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using CommonServiceLocator;
using Microsoft.Win32;
using Ferretto.Common.BLL.Interfaces.Providers;

namespace Ferretto.Common.Controls
{
    public class WmsImageEdit : ImageEdit
    {
        #region Fields

        public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register(
             nameof(Filename), typeof(string), typeof(WmsImageEdit), new PropertyMetadata(default(string), new PropertyChangedCallback(OnPathChanged)));

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
                     nameof(Path), typeof(string), typeof(WmsImageEdit), new PropertyMetadata(default(string)));

        private readonly IImageProvider imageService;

        private bool isUpdatingImage;

        #endregion

        #region Constructors

        public WmsImageEdit()
        {
            this.imageService = ServiceLocator.Current.GetInstance<IImageProvider>();
        }

        #endregion

        #region Properties

        public string Filename
        {
            get => (string)this.GetValue(FilenameProperty);
            set => this.SetValue(FilenameProperty, value);
        }

        public string Path
        {
            get => (string)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        #endregion

        #region Methods

        protected override void LoadCore()
        {
            var image = this.LoadImage();

            if (image != null)
            {
                this.EditStrategy.SetImage(image);
            }
        }

        protected override void OnSourceChanged(ImageSource oldValue, ImageSource newValue)
        {
            base.OnSourceChanged(oldValue, newValue);
            if (newValue == null)
            {
                this.Path = null;
            }
        }

        private static async void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsImageEdit wmsImage)
            {
                if (wmsImage.isUpdatingImage)
                {
                    wmsImage.isUpdatingImage = false;
                    return;
                }

                if (e.NewValue != null)
                {
                    wmsImage.Source = await ImageUtils.RetrieveImageAsync(wmsImage.imageService, (string)e.NewValue);
                }
                else
                {
                    wmsImage.Source = null;
                }
            }
        }

        private ImageSource LoadImage()
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = EditorLocalizer.GetString(EditorStringId.ImageEdit_OpenFileFilter);

            if (dlg.ShowDialog() == true)
            {
                using (var stream = dlg.OpenFile())
                {
                    if (stream is FileStream fileStream)
                    {
                        this.isUpdatingImage = true;
                        this.Filename = System.IO.Path.GetFileName(fileStream.Name);
                        this.Path = System.IO.Path.GetFullPath(fileStream.Name);
                    }

                    var ms = new MemoryStream(stream.GetDataFromStream());
                    return ImageHelper.CreateImageFromStream(ms);
                }
            }

            return null;
        }

        #endregion
    }
}
