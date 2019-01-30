namespace Ferretto.Common.Controls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using DevExpress.Xpf.Core.Native;
    using DevExpress.Xpf.Editors;
    using DevExpress.Xpf.Layout.Core;
    using Ferretto.Common.BLL.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.Win32;

    public class WmsImageEdit : ImageEdit
    {
        #region Fields

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
             nameof(Path), typeof(string), typeof(WmsImageEdit), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register(
             nameof(Filename), typeof(string), typeof(WmsImageEdit), new PropertyMetadata(default(string), new PropertyChangedCallback(OnPathChanged)));

        private readonly IImageProvider imageService;

        #endregion Fields

        private bool isUpdatingImage;

        #region Constructors

        public WmsImageEdit()
        {
            this.imageService = ServiceLocator.Current.GetInstance<IImageProvider>();
        }

        #endregion Constructors

        #region Properties

        public string Path
        {
            get => (string)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }
         public string Filename
        {
            get => (string)this.GetValue(FilenameProperty);
            set => this.SetValue(FilenameProperty, value);
        }

        #endregion Properties

        #region Methods

        protected override void LoadCore()
        {
            ImageSource image = this.LoadImage();

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


        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsImageEdit wmsImage)
            {
                if (wmsImage.isUpdatingImage)
                {
                    wmsImage.isUpdatingImage = false;
                    return;
                }
                if(e.NewValue != null)
                {
                    wmsImage.Source = ImageUtils.RetrieveImage(wmsImage.imageService, (string)e.NewValue);
                }
            }
        }

        private ImageSource LoadImage()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = EditorLocalizer.GetString(EditorStringId.ImageEdit_OpenFileFilter);
            if (dlg.ShowDialog() == true)
            {
                using (Stream stream = dlg.OpenFile())
                {
                    if (stream is FileStream)
                    {
                        this.isUpdatingImage = true;
                        this.Filename = System.IO.Path.GetFileName(((FileStream)stream).Name);
                        this.Path = System.IO.Path.GetFullPath(((FileStream)stream).Name);
                    }

                    MemoryStream ms = new MemoryStream(stream.GetDataFromStream());
                    return ImageHelper.CreateImageFromStream(ms);
                }
            }
            return null;
        }

        #endregion Methods
    }
}
