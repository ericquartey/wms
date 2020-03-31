namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum UserAccessLevel
    {
        NoAccess = 0,

        Operator = 1,   // No manuali

        Installer = 2,  // Con sicurezze

        Support = 3,

        Admin = 99,     // Tutto
    }
}
