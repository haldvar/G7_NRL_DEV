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

  
    public enum EnumStatus 
    { 
        Ny = 0, 
        UnderBehandling = 1, 
        Godkjent = 2, 
        Avvist = 3, 
        Venter = 4 
    }

    
    public enum OrgNames
    {
        NLA,
        Politiet,
        Avinor,
        Lufttransport,
        Helitrans,
        Forsvaret       
    }
    
}
