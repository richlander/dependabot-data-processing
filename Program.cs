using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

string file = "packages.csv";
Dictionary<string, List<Package>> packages = new();
IEnumerable<string> linesInFile = File.ReadLines(file);
linesInFile.GetEnumerator().MoveNext();

// Get packages
foreach(string line in linesInFile)
{
    string[] array = line.Split(',');
    Package package = new(
        array[0].Substring(1,array[0].Length-2),
        array[1].Substring(1,array[1].Length-2),
        array[3].Substring(1,array[3].Length-2),
        array[4].Substring(1,array[4].Length-2)
    );

    if(!packages.TryGetValue(package.Id, out List<Package>? packageList))
    {
        packageList = new();
        packages.Add(package.Id, packageList);
    }

    packageList.Add(package);
}

List<PackageIgnoreInfo> packageIgnoreInfo = new();
PackageSet packageSet = new(packageIgnoreInfo);

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
    List<string> ignoreVersions = new() {package.Version};
    for(int i = lastPackage - 1; i >= 0; i--)
    {
        package = packageFamily[i];
        PackageTargetFrameworkIgnoreMapping mapping = new(package.TargetFramework,ignoreVersions.ToArray());
        mappings[i] = mapping; 
        ignoreVersions.Add(package.Version);
    }
}

// Print data file
string json = JsonSerializer.Serialize<PackageSet>(packageSet, new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    });
Console.WriteLine(json);

record Package(string Id, string Version, string TargetFramework, string PrevTFM);
record PackageIgnoreInfo(string Name, PackageTargetFrameworkIgnoreMapping[] Mapping);
record PackageTargetFrameworkIgnoreMapping(string TargetFramework, string[] Ignore);
record PackageSet(List<PackageIgnoreInfo> Packages);