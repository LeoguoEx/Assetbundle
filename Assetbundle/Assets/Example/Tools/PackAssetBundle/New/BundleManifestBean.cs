using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BundleManifestBean
{
    public string ManifestFileVersion { get; set; }
    public string CRC { get; set; }
    public Hashes Hashes { get; set; }
    public List<ClassType> ClassTypes { get; set; }
    public List<string> Assets { get; set; }
    public List<string> Dependencies { get; set; }
    public int HashAppended { get; set; }
}

public class Hashes
{
    public AssetFileHash AssetFileHash { get; set; }
    public TypeTreeHash TypeTreeHash { get; set; }
}

public class AssetFileHash
{
    public string serializedVersion { get; set; }
    public string Hash { get; set; }
}

public class TypeTreeHash
{
    public string serializedVersion { get; set; }
    public string Hash { get; set; }
}

public class ClassType
{
    public string Class { get; set; }
    public Script Script { get; set; }
}

public class Script
{
    public string instanceID { get; set; }
    public string fileID { get; set; }
    public string guid { get; set; }
    public string type { get; set; }
}