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
