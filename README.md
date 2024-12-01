# ResolveBackupBrowser
Small console app to browse your DaVinci Resolve backups and quickly determine the name of all projects to choose and use the correct backup.
Thid project came from issues I had with my Resolve project database where I loose several Project.db files and I then needed to browse all the backups to find the correct project. As Resolve is storing backup with unreadable folder name and that you need to download a SQLite DB browser to see which backup match which project, I decided to create this small tool that saved me hours.

This is a basic .NET console app which can be used either on Mac or on Windows (Release is only provided on Mac for the moment as it's my working daily OS).

Usage (parameters):
- -P="_Path_of_your_Resolve_Backup_folder_": this is a mandatory parameter thast is used to specify your DaVinci Resolve backup folder
- -L: Only display the last backup file
- -F="_Project_name_filter_": Display only the project containing the filter

