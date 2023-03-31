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

            Console.Write("Enter the file ID: ");
            string fileId = Console.ReadLine();

            Console.Write("Enter the output file name: ");
            string outputFileName = Console.ReadLine();

            outputFileName= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", outputFileName);

            await DownloadFileAsync(credentialsPath, fileId, outputFileName);

            Console.WriteLine("Download completed. Press enter to exit.");
            Console.ReadLine();
        }

        private static async Task DownloadFileAsync(string credentialsPath, string fileId, string outputFileName)
        {
            try
            {
                UserCredential credential;
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { DriveService.Scope.Drive },
                        "user",
                        CancellationToken.None);
                }

                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential
                });

                var request = service.Files.Get(fileId);
                var fileMetadata = request.Execute();
                var mediaDownloader = new MediaDownloader(service);

                long startByte = 0;
                if (File.Exists(outputFileName))
                {
                    var fileInfo = new FileInfo(outputFileName);
                    startByte = fileInfo.Length;
                }

                using (var fileStream = new FileStream(outputFileName, FileMode.OpenOrCreate))
                {
                    //mediaDownloader.ChunkSize = 256 * 1024; // 256KB
                    mediaDownloader.ChunkSize = 1024 * 1024; // 1 MB

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
            }
        }


        //private static async Task DownloadFileAsync(string credentialsPath, string fileId, string outputFileName)
        //{
        //    try
        //    {
        //        UserCredential credential;
        //        using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
        //        {
        //            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //                GoogleClientSecrets.Load(stream).Secrets,
        //                new[] { DriveService.Scope.DriveReadonly },
        //                "user",
        //                CancellationToken.None);
        //        }

        //        var service = new DriveService(new BaseClientService.Initializer
        //        {
        //            HttpClientInitializer = credential
        //        });

        //        var request = service.Files.Get(fileId);
        //        var fileMetadata = request.Execute();
        //        var downloadUrl = fileMetadata.WebContentLink;

        //        var mediaDownloader = new MediaDownloader(service);

        //        using (var fileStream = new FileStream(outputFileName, FileMode.Create))
        //        {
        //            mediaDownloader.ChunkSize = 256 * 1024; // 256KB
        //            mediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
        //            {
        //                switch (progress.Status)
        //                {
        //                    case DownloadStatus.Downloading:
        //                        Console.WriteLine($"Downloaded {progress.BytesDownloaded} bytes.");
        //                        break;
        //                    case DownloadStatus.Completed:
        //                        Console.WriteLine("Download completed.");
        //                        break;
        //                    case DownloadStatus.Failed:
        //                        Console.WriteLine($"Download failed: {progress.Exception}");
        //                        break;
        //                }
        //            };
        //            await mediaDownloader.DownloadAsync(downloadUrl, fileStream);
        //        }
        //    }catch(Exception err)
        //    {

        //        var aa = err.Message;

        //    }
        //}
    }
}
