# GoogleDriveDownloader
GoogleDriveDownloader is a C# console application for downloading large files from Google Drive. It uses the Google Drive API for authentication and authorization and can resume interrupted downloads and control chunk size.

Downloading large files from Google Drive using a browser can sometimes be problematic due to timeouts or network issues. Instead, you can try using our code by using Google Drive API. Here are the steps:

1.Download the project.

2.Enable Google Drive API:

   - Go to the Google Developers Console and create a new project.
   - Navigate to "APIs & Services" > "Dashboard" > "+ ENABLE APIS AND SERVICES" and search for "Google Drive API."
   - Enable it for your project.

3.Create credentials:

   - Under "APIs & Services" > "Credentials," click "Create credentials" and select "OAuth client ID."
   - Choose "Desktop app" as the application type and give it a name.
   - Download the JSON file containing your client ID and client secret.
   - Save the JSON file to your local machine.
   - Optional (or you can modife the code)
       - Save the JSON file to C drive root.
       - Named the file "googledrivelargfile.json".
   
4.Modify the code:

   - Open the project in Visual Studio.
   - Replace the path to the JSON file in the Main method of the Program.cs file with the path to your JSON file.
   - Modify the chunk size if necessary.

Note: The default download location is the "Downloads" folder in the user's profile directory. You can modify the output file name and location in the Main method of the Program.cs file.

=========================================================================

The *FILE_ID* refers to the unique identifier assigned to a file in Google Drive. It is not the file name. Each file and folder in Google Drive has a unique ID that is used to reference it within the Google Drive API.

To find the FILE_ID of a file in Google Drive:

   1. Open Google Drive in your web browser.
   2. Navigate to the file you want to download.
   3. Right-click the file and select "Get link" or, if the file is already open, click the "Share" button in the top-right corner of the page.
   4. In the sharing dialog, you'll see a link that looks like this: https://drive.google.com/file/d/FILE_ID/view?usp=sharing
   5. The FILE_ID is the long string of letters and numbers in the middle of the link, between /d/ and /view.

For example, if your file's sharing link is:
https://drive.google.com/file/d/1aBcDeFgHiJkLmNoPQrStUvWxZy/view?usp=sharing

Then the FILE_ID is:
1aBcDeFgHiJkLmNoPQrStUvWxZy
