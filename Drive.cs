using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace bitport_google_drive_syncer;

public static class Drive
{
    public static DriveService Service { get; set; }
    public static void GetService(GoogleDriveConfig google)
    {
        var tokenResponse = new TokenResponse
        {
            AccessToken = google.AccessToken,
            RefreshToken = google.RefreshToken,
        };

        var applicationName = google.applicationName; // Use the name of the project in Google Cloud
        var username = google.username; // Use your email

        var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = google.google_ClientId,
                ClientSecret = google.google_ClientSecret
            },
            Scopes = new[] { DriveService.Scope.Drive },
            DataStore = new FileDataStore(applicationName)
        });

        var credential = new UserCredential(apiCodeFlow, username, tokenResponse);

        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName
        });
        Service = service;
    }
    
    
    public static string CreateFolder(string folderName)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder"
        };
        var request = Service.Files.Create(fileMetadata);
        request.Fields = "id";
        var file = request.Execute();
        Console.WriteLine("Folder ID: " + file.Id);
        return file.Id;
    }
    
    public static async Task<string> UploadZipFile(Stream stream, string code, string folderId)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File();
        fileMetadata.Name = $"{code}.zip";
        fileMetadata.Parents = new List<string>() { folderId };
        fileMetadata.MimeType = "application/zip";
        FilesResource.CreateMediaUpload request;
        request = Service.Files.Create(fileMetadata, stream,"application/zip");
        request.Fields = "id";
        await request.UploadAsync();
        var file = request.ResponseBody;
        return file.Id;
    }
}