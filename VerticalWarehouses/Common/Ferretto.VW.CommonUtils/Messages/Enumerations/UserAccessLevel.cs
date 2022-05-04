namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum UserAccessLevel
    {
        NoAccess = 0,

        Operator = 1,   // No manuali

        Movement = 2, // Si manuali

        Installer = 3,  // Con sicurezze

        Support = 4,

        Admin = 99,     // Tutto
    }
}
