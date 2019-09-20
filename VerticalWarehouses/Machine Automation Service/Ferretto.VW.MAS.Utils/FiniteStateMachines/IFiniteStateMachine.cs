using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.Utils
{
    public interface IFiniteStateMachine
    {


        #region Events

        event EventHandler Completed;

        #endregion



        #region Methods

        void Start(IMessageData data, CancellationToken cancellationToken);

        #endregion
    }
}
