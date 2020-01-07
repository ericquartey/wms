using System;

namespace Ferretto.VW.App.Scaffolding.DataAnnotations
{
    public interface ILocalizableString
    {
        Type ResourceType { get; }
        string ResourceName { get; }
        string DefaultValue { get; }
    }

}
