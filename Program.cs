using System;
using System.Collections.Generic;
using System.IO;


Package? lastPackage = null;

List<Package>? packageFamily;

string file = "packages.csv";

bool firstRow = true;

foreach(string line in File.ReadAllLines(file))
{
    if (firstRow)
    {
        firstRow = false;
        continue;
    }

    string[] array = line.Split(',');
    Package package = new(
        array[0].Substring(1,array[0].Length-2),
        array[1].Substring(1,array[1].Length-2),
        array[2].Substring(1,array[2].Length-2),
        array[3].Substring(1,array[3].Length-2),
        array[4].Substring(1,array[4].Length-2),
        array[4].Substring(1,array[4].Length-2)
    );

    if (package.Id == lastPackage?.Id)
    {
        
    }

}

record Package(string Id, string Version, string ParsedVersion, string TargetFramework, string PrevTFM, string ChangedTFM);
record PackageInfo(string Name, PackageTargetFrameworkIgnoreMapping[] Mapping);
record PackageTargetFrameworkIgnoreMapping(string TargetFramework, string[] Ignore);