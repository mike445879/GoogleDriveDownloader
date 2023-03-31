using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Download;
using System.Net.Http.Headers;

namespace GoogleDriveDownloadLargeFile
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Replace with the path to your credentials file.
            string credentialsPath = @"C:\googledrivelargfile.json";

            // Prompt the user to enter the file ID and output file name.
            Console.Write("Enter the file ID: ");
            string fileId = Console.ReadLine();

            Console.Write("Enter the output file name: ");
            string outputFileName = Console.ReadLine();

            // Set the output file path to the Downloads folder.
            outputFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", outputFileName);

            // Download the file and await its completion.
            await DownloadFileAsync(credentialsPath, fileId, outputFileName);

            // Prompt the user to exit.
            Console.WriteLine("Download completed. Press enter to exit.");
            Console.ReadLine();
        }

        // This method downloads a file from Google Drive using the Drive API.
        private static async Task DownloadFileAsync(string credentialsPath, string fileId, string outputFileName)
        {
            try
            {
                // Load the user's credentials from the specified file path.
                UserCredential credential;
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { DriveService.Scope.Drive },
                        "user",
                        CancellationToken.None);
                }

                // Create a new DriveService instance using the user's credentials.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential
                });

                // Create a new file download request using the specified file ID.
                var request = service.Files.Get(fileId);

                // Execute the file download request and get the file metadata.
                var fileMetadata = request.Execute();

                // Create a new MediaDownloader instance for downloading the file.
                var mediaDownloader = new MediaDownloader(service);

                // Set the starting byte position for resuming an interrupted download.
                long startByte = 0;
                if (File.Exists(outputFileName))
                {
                    var fileInfo = new FileInfo(outputFileName);
                    startByte = fileInfo.Length;
                }

                // Open the output file stream for writing and set the download chunk size.
                using (var fileStream = new FileStream(outputFileName, FileMode.OpenOrCreate))
                {
                    // Configure the chunk size of the media downloader.
                    //mediaDownloader.ChunkSize = 256 * 1024; // 256KB
                    mediaDownloader.ChunkSize = 1024 * 1024; // 1 MB

                    // Set up the progress changed event handler.
                    mediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                                Console.WriteLine($"Downloaded {progress.BytesDownloaded} bytes.");
                                break;
                            case DownloadStatus.Completed:
                                Console.WriteLine("Download completed.");
                                break;
                            case DownloadStatus.Failed:
                                Console.WriteLine($"Download failed: {progress.Exception}");
                                break;
                        }
                    };

                    // If the file exists, set the media downloader's range header and seek the file stream.
                    if (startByte > 0)
                    {
                        mediaDownloader.Range = new RangeHeaderValue(startByte, null);
                        fileStream.Seek(startByte, SeekOrigin.Begin);
                    }

                    // Download the file asynchronously using the media downloader and output file stream.
                    var downloadUri = request.CreateRequest().RequestUri.ToString();
                    await mediaDownloader.DownloadAsync(downloadUri, fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

    }
}
