using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CommonServiceLocator;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using Ferretto.Common.BLL.Interfaces.Providers;
using Microsoft.Win32;

namespace Ferretto.WMS.App.Controls
{
    public class WmsImageEdit : ImageEdit
    {
        #region Fields

        public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register(
            nameof(Filename),
            typeof(string),
            typeof(WmsImageEdit),
            new PropertyMetadata(
                default(string),
                async (d, e) =>
                {
                    if (d is WmsImageEdit wmsImage)
                    {
                        await wmsImage.RefreshImageAsync();
                    }
                }));

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy), typeof(bool), typeof(WmsImageEdit), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            nameof(Path), typeof(string), typeof(WmsImageEdit), new PropertyMetadata(
                default(string),
                async (d, e) =>
                {
                    if (d is WmsImageEdit wmsImage)
                    {
                        await wmsImage.RefreshImageAsync();
                    }
                }));

        private readonly IFileProvider fileProvider;

        private bool isUpdatingImage;

        #endregion

        #region Constructors

        public WmsImageEdit()
        {
            this.fileProvider = ServiceLocator.Current.GetInstance<IFileProvider>();
        }

        #endregion

        #region Properties

        public string Filename
        {
            get => (string)this.GetValue(FilenameProperty);
            set => this.SetValue(FilenameProperty, value);
        }

        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }

        public string Path
        {
            get => (string)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        #endregion

        #region Methods

        public override void Clear()
        {
            base.Clear();
            this.Filename = null;
            this.Path = null;
            this.IsBusy = false;
        }

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

        private ImageSource LoadImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = EditorLocalizer.GetString(EditorStringId.ImageEdit_OpenFileFilter),
            };

            if (dlg.ShowDialog() == true)
            {
                using (var stream = dlg.OpenFile())
                {
                    this.IsBusy = true;

                    if (stream is FileStream fileStream)
                    {
                        this.isUpdatingImage = true;
                        this.Path = System.IO.Path.GetFullPath(fileStream.Name);
                    }

                    var ms = new MemoryStream(stream.GetDataFromStream());

                    this.IsBusy = false;
                    return ImageHelper.CreateImageFromStream(ms);
                }
            }

            return null;
        }

        private async Task RefreshImageAsync()
        {
            this.IsBusy = true;
            if (this.isUpdatingImage)
            {
                this.isUpdatingImage = false;
                this.IsBusy = false;
                return;
            }

            if (this.Filename != null)
            {
                this.Source = await ImageUtils
                    .GetImageAsync(this.fileProvider, this.Filename)
                    .ConfigureAwait(true);
            }
            else
            {
                this.Source = null;
            }

            this.IsBusy = false;
        }

        #endregion
    }
}
