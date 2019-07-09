using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;

namespace Ferretto.VW.App
{
    public class BaseModel : BindableBase, IDataErrorInfo
    {
        #region Fields

        private bool isValidationEnabled;

        #endregion

        #region Constructors

        protected BaseModel()
        {
        }

        #endregion

        #region Properties

        public string Error => string.Join(
            System.Environment.NewLine,
            this.GetType().GetProperties()
                .Select(p => this[p.Name])
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s)));

        public bool IsValidationEnabled
        {
            get => this.isValidationEnabled;
            set
            {
                if (this.SetProperty(ref this.isValidationEnabled, value))
                {
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }

        #endregion

        #region Indexers

        public string this[string columnName] => this.ValidateIfEnabled(columnName);

        #endregion

        #region Methods

        protected virtual string ValidateProperty(string propertyName)
        {
            // do nothing
            return null;
        }

        private string ValidateIfEnabled(string propertyName)
        {
            if (!this.IsValidationEnabled)
            {
                return null;
            }

            return this.ValidateProperty(propertyName);
        }

        #endregion
    }
}
