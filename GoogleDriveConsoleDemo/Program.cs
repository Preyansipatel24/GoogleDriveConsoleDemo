/// Import necessary libraries
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

// Define the path to the credentials file, the ID of the folder where files will be uploaded, and the files to be uploaded
string credentialPath = "credentials.json";

string folderId = "1zfI3Rh4uwvwquMui08DgynEoZMYUSi0v";

string[] filesToUpload = { "1.png" };

// Call the function to upload the files
UploadFilesToFolder(credentialPath, folderId, filesToUpload);

void UploadFilesToFolder(string credentialPath, string folderId, string[] filesToUpload)
{
    // Declare a variable to hold the Google credentials
    GoogleCredential credentials;

    // Read the credentials from the file
    using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
    {
        credentials = GoogleCredential.FromStream(stream).CreateScoped(new[]
        {
            DriveService.ScopeConstants.DriveFile
        });
    }

    // Initialize the Google Drive service
    var service = new DriveService(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credentials,
        ApplicationName = "GoogleDriveUploadFile"
    });

    // Loop through each file to be uploaded
    foreach (var item in filesToUpload)
    {
        // Create the file metadata
        var fileMetaData = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(item),
            Parents = new List<string>() { folderId }
        };

        // Declare a variable to hold the upload request
        FilesResource.CreateMediaUpload request;

        // Read the file and create the upload request
        using (var stream = new FileStream(item, FileMode.Open, FileAccess.Read))
        {
            request = service.Files.Create(fileMetaData, stream, "");
            request.Fields = "id";
            request.Upload();
        }

        // Get the ID of the uploaded file
        var uploadFile = request.ResponseBody.Id;

        // Set the file's permissions to public
        var permission = new Google.Apis.Drive.v3.Data.Permission
        {
            Type = "anyone",
            Role = "reader"
        };

        // Create the permissions request and execute it
        var permRequest = service.Permissions.Create(permission, uploadFile);
        permRequest.Execute();

        // Get the file's metadata using the file ID
        FilesResource.GetRequest getRequest = service.Files.Get(uploadFile);
        getRequest.Fields = "webViewLink";
        var file = getRequest.Execute();

        // Print the URL of the uploaded file
        Console.WriteLine("File URL: " + file.WebViewLink);
    }
}