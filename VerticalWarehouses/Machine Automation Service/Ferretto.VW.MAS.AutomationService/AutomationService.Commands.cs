﻿using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService
{
    partial class AutomationService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return false;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
