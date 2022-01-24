using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public abstract class BaseOperatorViewModel : BaseMainViewModel, IRegionMemberLifetime
    {
        #region Fields

        private bool noteEnabled;

        #endregion

        #region Constructors

        protected BaseOperatorViewModel(PresentationMode mode)
            : base(mode)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public override bool KeepAlive => true;

        public bool NoteEnabled
        {
            get => this.noteEnabled;
            set => this.SetProperty(ref this.noteEnabled, value);
        }

        #endregion
    }
}
