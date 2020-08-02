using System;
using System.Collections.Generic;
using Prism.Ioc;
using Unity;

namespace Ferretto.VW.Devices.TokenReader
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry ConfigureTokenReaderDriver(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<ITokenReaderDriver, TokenReaderDriver>();

            return containerRegistry;
        }

        #endregion
    }
}
