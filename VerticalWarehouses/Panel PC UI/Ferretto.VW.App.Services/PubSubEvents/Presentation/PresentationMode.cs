using System;

namespace Ferretto.VW.App.Services
{
    [Flags]
    public enum PresentationMode
    {
        None,

        Help,

        Login,

        Menu,

        Installer,

        Operator,
    }
}
