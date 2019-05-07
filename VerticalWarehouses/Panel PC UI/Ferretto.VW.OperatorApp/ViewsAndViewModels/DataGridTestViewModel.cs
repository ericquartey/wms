using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    public class DataGridTestViewModel : BindableBase, IDataGridTestViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase gridPlaceholder;

        #endregion

        #region Properties

        public BindableBase GridPlaceholder { get => this.gridPlaceholder; set => this.SetProperty(ref this.gridPlaceholder, value); }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.gridPlaceholder = this.container.Resolve<ICustomControlArticleDataGridViewModel>() as CustomControlArticleDataGridViewModel;
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
