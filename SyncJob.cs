using BitPortLibrary;
using Quartz;

namespace bitport_google_drive_syncer;

[DisallowConcurrentExecution]
public class SyncJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var oldFolders = FolderCache.folders;
            var newFolders = await ClientBuilder.getFolders();
            newFolders.ExceptWith(oldFolders);
            foreach (var folder in newFolders)
            {
                var message = await ClientBuilder.client.GetFolderZipURL(folder.code);
                var result = await Drive.UploadZipFile(message, folder.name, FolderCache.id);
                Console.WriteLine($"Zip file uploaded to Google Drive with ID: ${result}");
            }
            FolderCache.folders.UnionWith(newFolders);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in job: {ex.Message}");
        }
    }
}