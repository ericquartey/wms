using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemSearchDetailViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IWmsImagesProvider wmsImagesProvider;

        private Image image;

        private Item item;

        #endregion

        #region Constructors

        public ItemSearchDetailViewModel(IWmsImagesProvider wmsImagesProvider)
            : base(PresentationMode.Operator)
        {
            this.wmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.None;

        public Image Image { get => this.image; set => this.SetProperty(ref this.image, value); }

        public Item Item
        {
            get => this.item;
            set
            {
                this.item = value;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.Image?.Dispose();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            // Sistema di cache, potenzialmente errato!
            var searchViewModel = ServiceLocator.Current.GetInstance<IItemSearchViewModel>();
            if (searchViewModel != null &&
                searchViewModel.SelectedItem != null)
            {
                this.Item = searchViewModel.SelectedItem;
                // await this.LoadImage(this.item.Code);
                this.RaisePropertyChanged(nameof(this.Item));
            }
            else
            {
                // effettuare la chiamata?
            }
        }

        private async Task LoadImage(string code)
        {
            this.Image?.Dispose();
            this.Image = null;
            var stream = await this.wmsImagesProvider.GetImageAsync(code);
            this.Image = Image.FromStream(stream);
        }

        #endregion
    }
}
