// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

String resolveBackupFolder = null;
String projectFilter = null;
bool onlyLastVersion = false;

if (args.Length > 0)
{
    foreach (var arg in args)
    {
        if (Const.debug)
            Console.WriteLine($"Argument={arg}");
        if (arg.ToLower().Contains("-p"))
            resolveBackupFolder = arg.Replace("-P=","").Replace("-p=","");
        if (arg.ToLower().Contains("-l"))
            onlyLastVersion = true;
        if (arg.ToLower().Contains("-f"))
            projectFilter = arg.Replace("-F=", "").Replace("-f=", "");
        if (arg.ToLower().Contains("-h") || arg.ToLower().Contains("-help"))
        {
            DisplayHelp();
            return;
        }
    }
}
else
{
    if (Const.debug)
        Console.WriteLine("No arguments");
}

if (resolveBackupFolder == null)
{
    Console.WriteLine("No backup folder specified, please use -P to specify backup folder");
    DisplayHelp();
    return;
}

string[] allfiles = Directory.GetFiles(resolveBackupFolder, "Project.db.*", SearchOption.AllDirectories);
Hashtable projects = new Hashtable();

foreach (var file in allfiles)
{
    
    FileInfo info = new FileInfo(file);
    // Do something with the Folder or just add them to a list via nameoflist.add();
    String projectName = GetProjectName(file);
    
    if (projectFilter != null && !projectName.ToLower().Contains(projectFilter.ToLower()))
        continue;
    
    if (Const.debug)
        Console.WriteLine($"Scanning: {file} => {projectName}");

    ResolveProject project = null;

    if (projects.ContainsKey(projectName))
    {
        if (Const.debug)
            Console.WriteLine("Project already exists");
        project = projects[projectName] as ResolveProject;
    }
    else
    {
        if (Const.debug)
            Console.WriteLine("Creating new project");
        project = new ResolveProject(projectName);
    }
    
    project.versions.Add(new ProjectVersion(file));

    try
    {
        projects.Add(projectName, project);
    }
    catch
    {
    }

}

foreach (DictionaryEntry entry in projects)
{
    var project = (ResolveProject)entry.Value;
    Console.WriteLine($"-----------------------------------------------");
    Console.WriteLine($"Project: " + project.name);
    project.versions.Sort();
    if (onlyLastVersion)
    {
        ProjectVersion version = (ProjectVersion)project.versions[0];
        Console.WriteLine("Version: " + version.ToString());
    }
    else
        foreach (var version2 in project.versions)
        {
            ProjectVersion version = (ProjectVersion)version2;
            Console.WriteLine("Version: " + version.ToString());
        }
}



static void DisplayHelp()
{
    Console.WriteLine("Let you browse your DaVinci Resolve Backup folder more easily");
    Console.WriteLine("Usage (parameters):");
    Console.WriteLine("- -P=\"Path_of_your_Resolve_Backup_folder\": this is a mandatory parameter thast is used to specify your DaVinci Resolve backup folder");
    Console.WriteLine("- -L: Only display the last backup file");
    Console.WriteLine("- -F=\"Project_name_filter\": Display only the project containing the filter");

}

static String GetProjectName(String dbFilePath)
{
    String result = "";
    using (var connection = new SqliteConnection("Data Source=" + dbFilePath))
    {
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"SELECT ProjectName FROM SM_Project";

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                result = reader.GetString(0);

                //Console.WriteLine($"Hello, {name}!");
            }
        }
    }
    return result;
}

class Const
{
    public const bool debug = false;
   
}

class ProjectVersion: IComparable
{
    public String dateText;
    public DateTime date;
    public String databaseFile;

    public ProjectVersion(String databaseFile)
    {
        this.databaseFile = databaseFile;
        Match match = Regex.Match(databaseFile, @"db\.(\d{0,})");
        if (match.Success)
        {
            dateText = match.Groups[1].Value;
            date = DateTime.ParseExact(dateText, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }
        if (Const.debug)
            Console.WriteLine("creating version " + dateText);
    }

    public int CompareTo(object? obj)
    {
//        return (date < ((ProjectVersion)obj).date ? -1 :1);
        return -1*dateText.CompareTo(((ProjectVersion)obj).dateText);
    }

    public String ToString()
    {
        return "Date: " + date.ToString("G") + "       ---> " + databaseFile;
    }
}

class ResolveProject
{
    public String name;
    public ArrayList versions = new ArrayList();

    public ResolveProject(String name)
    {
        this.name = name;
        if (Const.debug)
            Console.WriteLine("creating project " + name);
    }
}