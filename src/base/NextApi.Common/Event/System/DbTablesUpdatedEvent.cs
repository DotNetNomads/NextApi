namespace NextApi.Common.Event.System
{
    /// <summary>
    /// Database table change event.
    /// Fires when NextApi detects changes in database.
    /// </summary>
    public class DbTablesUpdatedEvent : BaseNextApiEvent<string[]>
    {
    }
}
