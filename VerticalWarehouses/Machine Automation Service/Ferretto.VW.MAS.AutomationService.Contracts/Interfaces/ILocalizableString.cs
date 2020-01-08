using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    public interface ILocalizableString
    {
        Type ResourceType { get; }
        string ResourceName { get; }
        string DefaultValue { get; }
    }

}
