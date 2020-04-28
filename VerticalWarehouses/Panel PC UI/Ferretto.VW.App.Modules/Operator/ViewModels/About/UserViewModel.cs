using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class UserViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private string adminLanguage;

        private string installerLanguage;

        private ObservableCollection<string> languageList;

        private string operatorLanguage;

        private string serviceLanguage;

        #endregion

        #region Constructors

        public UserViewModel()
            : base()
        {
        }

        #endregion

        #region Properties

        public string AdminLanguage
        {
            get => this.adminLanguage;
            set => this.SetProperty(ref this.adminLanguage, value, this.RaiseCanExecuteChanged);
        }

        public string InstallerLanguage
        {
            get => this.installerLanguage;
            set => this.SetProperty(ref this.installerLanguage, value, this.RaiseCanExecuteChanged);
        }

        public ObservableCollection<string> LanguageList
        {
            get => this.languageList;
            set => this.SetProperty(ref this.languageList, value, this.RaiseCanExecuteChanged);
        }

        public string OperatorLanguage
        {
            get => this.operatorLanguage;
            set => this.SetProperty(ref this.operatorLanguage, value, this.RaiseCanExecuteChanged);
        }

        public string ServiceLanguage
        {
            get => this.serviceLanguage;
            set => this.SetProperty(ref this.serviceLanguage, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.LanguageList = new ObservableCollection<string> { "ITA", "EN" };

            await base.OnAppearedAsync();
        }

        internal bool CanExecuteCommand()
        {
            return true;
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
