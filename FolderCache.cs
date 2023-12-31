namespace bitport_google_drive_syncer;

public static class FolderCache
{
    public static string id { get; set; }
    public static HashSet<(string name, string code)> folders { get; set; }
}