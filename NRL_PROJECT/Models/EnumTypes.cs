namespace NRL_PROJECT.Models
{
    public enum ObstacleReportStatus
    {
        New,
        Open,
        InProgress,
        Resolved,
        Closed,
        Deleted,
    }


    public enum UserRoleType
    {
        Pilot = 0,
        Registrar = 1,
        Admin = 2,
        ExternalOrg = 3
    }

    public enum EnumStatus { Ny = 0, UnderBehandling = 1, Godkjent = 2, Avvist = 3, Venter = 4 }

}
