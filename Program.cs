using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

string file = "packages.csv";
Dictionary<string, List<Package>> packages = new();
IEnumerable<string> linesInFile = File.ReadLines(file);
linesInFile.GetEnumerator().MoveNext();

/*
Example target framework families:

.NETFramework4.5.1
.NETStandard2.0
.NETCoreApp3.0
net5.0
*/


// Get packages
foreach(string line in linesInFile)
{
    string[] array = line.Split(',');
    string targetFramework = array[3].Substring(1,array[3].Length-2);
    string tfm = targetFramework;
    if (targetFramework[0] == '.')
    {
        string[] tfItems = targetFramework.Split('.');
        string baseTfm = tfItems[1].Substring(0,tfItems[1].Length -1);
        string majorVersion = tfItems[1].Substring(tfItems[1].Length -1, 1);
        tfm = baseTfm switch
        {
            "NETCoreApp" => $"netcoreapp{majorVersion}.{tfItems[2]}",
            "NETStandard" => $"netstandard{majorVersion}.{tfItems[2]}",
            "NETFramework" => $"net{targetFramework.Substring(13).Replace(".",null)}",
            _ => throw new Exception("Unknown target framework"),
        };
    }

    Package package = new(
        array[0].Substring(1,array[0].Length-2),
        array[1].Substring(1,array[1].Length-2),
        tfm
    );

    if(!packages.TryGetValue(package.Id, out List<Package>? packageList))
    {
        packageList = new();
        packages.Add(package.Id, packageList);
    }

    packageList.Add(package);
}

List<PackageIgnoreInfo> packageIgnoreInfo = new();

// Process packages
foreach(string name in packages.Keys)
{
    List<Package> packageFamily = packages[name];
    if (packageFamily.Count == 1)
    {
        continue;
    }

    int lastPackage = packageFamily.Count -1;
    PackageTargetFrameworkIgnoreMapping[] mappings = new PackageTargetFrameworkIgnoreMapping[lastPackage];
    packageIgnoreInfo.Add(new(name,mappings));
    Package package = packageFamily[lastPackage];
    List<string> ignoreVersions = new() {GetWildcardVersion(package.Version)};
    for(int i = lastPackage - 1; i >= 0; i--)
    {
        package = packageFamily[i];
        PackageTargetFrameworkIgnoreMapping mapping = new(package.TargetFramework,ignoreVersions.ToArray());
        mappings[i] = mapping; 
        ignoreVersions.Add(GetWildcardVersion(package.Version));
    }
}

// Print data file
PackageSet packageSet = new(packageIgnoreInfo, packageIgnoreInfo.Count);
string json = JsonSerializer.Serialize<PackageSet>(packageSet, new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    });
Console.WriteLine(json);

string GetWildcardVersion(string version)
{
    string[] versionComponents = version.Split('.');

    if (versionComponents.Length == 3)
    {
        return $"{versionComponents[0]}.{versionComponents[1]}.*";
    }

    return version;
}

record Package(string Id, string Version, string TargetFramework);
record PackageIgnoreInfo(string Name, PackageTargetFrameworkIgnoreMapping[] Mapping);
record PackageTargetFrameworkIgnoreMapping(string TargetFramework, string[] Ignore);
record PackageSet(List<PackageIgnoreInfo> Packages, int Count);