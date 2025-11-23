namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Enum used to represent report lifecycle states (backend names).
    /// </summary>
    public enum ObstacleReportStatus
    {
        New,
        Open,
        InProgress,
        Resolved,
        Closed,
        Deleted,
    }

    /// <summary>
    /// UI-oriented enum (localized) used in views for status labels.
    /// </summary>
    public enum EnumStatus
    {
        Ny = 0,
        UnderBehandling = 1,
        Godkjent = 2,
        Avvist = 3,
        Venter = 4
    }
}
