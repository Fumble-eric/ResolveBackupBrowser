// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

String resolveBackupFolder = @"/Users/eric/Movies/Resolve Project Backups";
String projectFilter = null;
bool onlyLastVersion = false;

if (args.Length > 0)
{
    foreach (var arg in args)
    {
        if (Const.debug)
            Console.WriteLine($"Argument={arg}");
        if (arg.Contains("-P"))
            resolveBackupFolder = arg.Replace("-P=","");
        if (arg.Contains("-L"))
            onlyLastVersion = true;
        if (arg.Contains("-F"))
            projectFilter = arg.Replace("-F=", "");
    }
}
else
{
    if (Const.debug)
        Console.WriteLine("No arguments");
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
        Console.WriteLine("Version: " + version.date + " - " + version.databaseFile);
    }
    else
        foreach (var version2 in project.versions)
        {
            ProjectVersion version = (ProjectVersion)version2;
            Console.WriteLine("Version: " + version.date + " - " + version.databaseFile);
        }
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
    public String date;
    public String databaseFile;

    public ProjectVersion(String databaseFile)
    {
        this.databaseFile = databaseFile;
        Match match = Regex.Match(databaseFile, @"db\.(\d{0,})");
        if (match.Success)
        {
            date = match.Groups[1].Value;
        }
        if (Const.debug)
            Console.WriteLine("creating version " + date);
    }

    public int CompareTo(object? obj)
    {
//        return (date < ((ProjectVersion)obj).date ? -1 :1);
        return -1*date.CompareTo(((ProjectVersion)obj).date);
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