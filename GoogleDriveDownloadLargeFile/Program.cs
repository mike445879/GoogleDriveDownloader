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
            string credentialsPath = @"C:\googledrivelargfile.json";

            Console.Write("Enter the file ID: ");
            string fileId = Console.ReadLine();

            Console.Write("Enter the output file name: ");
            string outputFileName = Console.ReadLine();

            outputFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", outputFileName);

            await DownloadFileAsync(credentialsPath, fileId, outputFileName);

            Console.WriteLine("Download completed. Press enter to exit.");
            Console.ReadLine();
        }

        private static async Task DownloadFileAsync(string credentialsPath, string fileId, string outputFileName)
        {
            UserCredential credential = await GetUserCredentialAsync(credentialsPath);

            using (var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            }))
            {
                var fileMetadata = GetFileMetadata(service, fileId);

                if (fileMetadata != null)
                {
                    await DownloadFileContentAsync(service, fileId, outputFileName, fileMetadata);
                }
                else
                {
                    Console.WriteLine("File not found on Google Drive.");
                }
            }
        }

        private static async Task<UserCredential> GetUserCredentialAsync(string credentialsPath)
        {
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None);
            }
        }

        private static Google.Apis.Drive.v3.Data.File GetFileMetadata(DriveService service, string fileId)
        {
            try
            {
                return service.Files.Get(fileId).Execute();
            }
            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine($"Google API Error: {ex.Message}");
                return null;
            }
        }

        private static async Task DownloadFileContentAsync(DriveService service, string fileId, string outputFileName, Google.Apis.Drive.v3.Data.File fileMetadata)
        {
            var request = service.Files.Get(fileId);
            var mediaDownloader = new MediaDownloader(service)
            {
                ChunkSize = 1024 * 1024 // 1 MB
            };

            long startByte = 0;
            if (File.Exists(outputFileName))
            {
                var fileInfo = new FileInfo(outputFileName);
                if (fileInfo.Length < fileMetadata.Size.GetValueOrDefault())
                {
                    startByte = fileInfo.Length;
                }
                else
                {
                    Console.WriteLine("Download already seems to be complete.");
                    return;
                }
            }

            mediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        string formattedSize = FormatSize(progress.BytesDownloaded);
                        Console.Write($"\rDownloaded {formattedSize}.");
                        break;
                    case DownloadStatus.Completed:
                        Console.WriteLine("\rDownload completed.                  ");
                        break;
                    case DownloadStatus.Failed:
                        Console.WriteLine($"\rDownload failed: {progress.Exception}");
                        break;
                }
            };

            try
            {
                using (var fileStream = new FileStream(outputFileName, FileMode.OpenOrCreate))
                {
                    if (startByte > 0)
                    {
                        mediaDownloader.Range = new RangeHeaderValue(startByte, null);
                        fileStream.Seek(startByte, SeekOrigin.Begin);
                    }

                    var downloadUri = request.CreateRequest().RequestUri.ToString();
                    await mediaDownloader.DownloadAsync(downloadUri, fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                File.Delete(outputFileName); // Cleanup partially downloaded file
            }
        }
        private static string FormatSize(long bytes)
        {
            const double KB = 1024;
            const double MB = KB * 1024;
            const double GB = MB * 1024;

            if (bytes < MB)
            {
                return $"{bytes / KB:F2} KB";
            }
            else if (bytes < GB)
            {
                return $"{bytes / MB:F2} MB";
            }
            else
            {
                return $"{bytes / GB:F2} GB";
            }
        }
    }
}
