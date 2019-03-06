using System;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IFiniteStateMachines
    {
        //TODO to remove

        #region Methods

        void Destroy();

        /// <summary>
        ///     Execute complete homing.
        /// </summary>
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException" /> is thrown, if object is null.</exception>
        void DoHoming();

        /// <summary>
        ///     Execute vertical homing.
        /// </summary>
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException" /> is thrown, if object is null.</exception>
        void DoVerticalHoming();

        #endregion
    }
}
