using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommonServiceLocator;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Microsoft.Win32;

namespace Ferretto.Common.Controls
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Bug",
        "S3168:\"async\" methods should not return \"void\"",
        Justification = "Ok",
        Scope = "member",
        Target = "~M:Ferretto.Common.Controls.WmsImageEdit.OnCommandActionChanged(System.Windows.DependencyObject,System.Windows.DependencyPropertyChangedEventArgs)")]
    public class WmsImageEdit : ImageEdit
    {
        #region Fields

        public static readonly DependencyProperty CommandActionProperty = DependencyProperty.Register(
                             nameof(CommandAction), typeof(WmsCommand), typeof(WmsImageEdit), new PropertyMetadata(OnCommandActionChanged));

        public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register(
                             nameof(Filename), typeof(string), typeof(WmsImageEdit), new PropertyMetadata(default(string), new PropertyChangedCallback(OnPathChanged)));

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
                     nameof(IsLoading), typeof(bool), typeof(WmsImageEdit), new PropertyMetadata(default(bool)));

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

        public WmsCommand CommandAction
        {
            get => (WmsCommand)this.GetValue(CommandActionProperty);
            set => this.SetValue(CommandActionProperty, value);
        }

        public string Filename
        {
            get => (string)this.GetValue(FilenameProperty);
            set => this.SetValue(FilenameProperty, value);
        }

        public bool IsLoading
        {
            get => (bool)this.GetValue(IsLoadingProperty);
            set => this.SetValue(IsLoadingProperty, value);
        }

        public string Path
        {
            get => (string)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        public Func<Task> UploadAction
        {
            get => new Func<Task>(async () => await this.UploadImageAsync());
        }

        #endregion

        #region Methods

        public override void Clear()
        {
            base.Clear();
            this.Filename = null;
            this.Path = null;
            this.IsLoading = false;
        }

        public async Task UploadImageAsync()
        {
            var image = await this.imageService.UploadAsync(this.Path, null);
            this.Filename = image;
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

        private static void OnCommandActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsImageEdit wmsImageEdit && e.NewValue is ICommand command)
            {
                var wmsCommand = (WmsCommand)command;
                /*call image service*/
                wmsCommand.BeforeExecute(wmsImageEdit.UploadAction);
            }
        }

        private static async void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsImageEdit wmsImage)
            {
                wmsImage.IsLoading = true;
                if (wmsImage.isUpdatingImage)
                {
                    wmsImage.isUpdatingImage = false;
                    wmsImage.IsLoading = false;
                    return;
                }

                if (e.NewValue != null)
                {
                    wmsImage.Source = await ImageUtils.RetrieveImageAsync(wmsImage.imageService, (string)e.NewValue);
                    wmsImage.IsLoading = false;
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
            this.IsLoading = true;
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

            this.IsLoading = false;
            return null;
        }

        #endregion
    }
}
