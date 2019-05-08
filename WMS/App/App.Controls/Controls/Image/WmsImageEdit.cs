using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        public static readonly DependencyProperty CommandActionProperty = DependencyProperty.Register(
                             nameof(CommandAction), typeof(WmsCommand), typeof(WmsImageEdit), new PropertyMetadata(OnCommandActionChanged));

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
                        wmsImage.IsLoading = true;
                        if (wmsImage.isUpdatingImage)
                        {
                            wmsImage.isUpdatingImage = false;
                            wmsImage.IsLoading = false;
                            return;
                        }

                        if (e.NewValue != null)
                        {
                            wmsImage.Source = await ImageUtils
                                .GetImageAsync(wmsImage.fileProvider, (string)e.NewValue)
                                .ConfigureAwait(true);
                        }
                        else
                        {
                            wmsImage.Source = null;
                        }

                        wmsImage.IsLoading = false;
                    }
                }));

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
                     nameof(IsLoading), typeof(bool), typeof(WmsImageEdit), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
                             nameof(Path), typeof(string), typeof(WmsImageEdit), new PropertyMetadata(default(string)));

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

        public Func<Task<bool>> UploadAction => async () => await this.UploadImageAsync();

        #endregion

        #region Methods

        public override void Clear()
        {
            base.Clear();
            this.Filename = null;
            this.Path = null;
            this.IsLoading = false;
        }

        public async Task<bool> UploadImageAsync()
        {
            if (this.Path == null)
            {
                return true;
            }

            var result = await this.fileProvider.UploadAsync(this.Path);

            this.Filename = result.Success ? result.Entity : null;

            return result.Success;
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
                wmsCommand.BeforeExecute(wmsImageEdit.UploadAction);
            }
        }

        private ImageSource LoadImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = EditorLocalizer.GetString(EditorStringId.ImageEdit_OpenFileFilter)
            };

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
