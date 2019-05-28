using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls
{
    public class TestArticle : BindableBase
    {
        #region Fields

        private string article;

        private string description;

        private string machine;

        #endregion

        #region Constructors

        public TestArticle(string article, string description, string machine)
        {
            this.article = article;
            this.description = description;
            this.machine = machine;
        }

        public TestArticle()
        {
        }

        #endregion

        #region Properties

        public string Article { get => this.article; set => this.SetProperty(ref this.article, value); }

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public string Machine { get => this.machine; set => this.SetProperty(ref this.machine, value); }

        #endregion
    }
}
