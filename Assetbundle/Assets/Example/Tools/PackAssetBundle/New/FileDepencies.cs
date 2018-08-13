
/**************************************************************************************************
	Copyright (C) 2017 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：FileDepencies.cs;
	作	者：W_X;
	时	间：2017 - 11 - 28;
	注	释：;
**************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class AssetBundleMD5Info
{
    public string bundleName;
    public string md5;
    public List<string> Assets = new List<string>();
    public string value;

    public void AddAssets(string path, string md5)
    {
        Assets.Add(string.Format("{0} : {1}", path, md5));

        if (string.IsNullOrEmpty(value))
        {
            value = md5;
        }
        else
        {
            value = string.Format("{0}\n{1}", value, md5);
        }
    }

    public void Compute()
    {
        if (Assets.Count <= 1)
        {
            md5 = value;
            return;
        }

        md5 = MD5Utils.GetMD5FromValue(value);
    }
}

public class FileDepencies
{
    private static Dictionary<string, string[]> s_CacheDep = new Dictionary<string, string[]>();
    private static List<AssetBundleMD5Info> md5List = new List<AssetBundleMD5Info>();

    public static void Clear()
    {
        s_CacheDep.Clear();
        md5List.Clear();
    }

    public static void AddDepencies(string path, string[] deps)
    {
        if (string.IsNullOrEmpty(path) || deps == null || deps.Length < 1)
        {
            return;
        }

        if (s_CacheDep.ContainsKey(path))
        {
            return;
        }

        s_CacheDep.Add(path, deps);
    }

    public static string[] GetDepencies(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string[] deps = null;
        if (s_CacheDep.TryGetValue(path, out deps))
        {
            return deps;
        }

        return new string[] { path };
    }

    public static string GetBundleMD5(string bundlePath)
    {
        if (string.IsNullOrEmpty(bundlePath) || !File.Exists(bundlePath))
        {
            return "";
        }

        List<string> assets = GetBundleAssets(bundlePath);
        List<string> depth_assets = GetBundleAssetsDepths(assets);
        if (depth_assets == null || depth_assets.Count < 1)
        {
            return "";
        }

        AssetBundleMD5Info info = GetMD5FromAssets(depth_assets, bundlePath);
        info.Compute();

        md5List.Add(info);

        return info.md5;
    }

    public static void WriteMd5Info()
    {
        string strFileList = string.Format("{0}.txt", ResourceConst.MD5Name);
        BuildCommon.WriteJsonToFile(PackAssetBundle.bundleBuildFolder, strFileList, md5List);
    }

    static AssetBundleMD5Info GetMD5FromAssets(List<string> assets, string bundlePath)
    {
        AssetBundleMD5Info info = new AssetBundleMD5Info();
        info.bundleName = bundlePath;

        if (assets == null || assets.Count < 1)
        {
            return info;
        }

        for (int i = 0; i < assets.Count; i++)
        {
            string md5 = MD5Utils.GetMD5FromFile(assets[i]);
            info.AddAssets(assets[i], md5);
        }

        return info;
    }

    public static List<string> GetBundleAssets(string bundlePath)
    {
        if (string.IsNullOrEmpty(bundlePath) || !File.Exists(bundlePath))
        {
            return null;
        }

        string bundleManifest = string.Format("{0}.manifest", bundlePath);
        if (!File.Exists(bundleManifest))
        {
            return null;
        }

        BundleManifestBean mainfest = null;

        FileStream fs = File.Open(bundleManifest, FileMode.Open);
        StreamReader reader = new StreamReader(fs);

        try
        {
            //mainfest = deserializer.Deserialize<BundleManifestBean>(reader);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        fs.Close();

        if (mainfest == null)
        {
            return null;
        }

        return mainfest.Assets;
    }

    public static List<string> GetBundleAssetsDepths(List<string> assets)
    {
        if (assets == null || assets.Count < 1)
        {
            return assets;
        }

        List<string> dep_assets = new List<string>();
        for (int i = 0; i < assets.Count; i++)
        {
            string path = assets[i];
            if (!dep_assets.Contains(path))
            {
                dep_assets.Add(path);
            }

            string[] deps = GetDepencies(path);
            foreach (string dep in deps)
            {
                if (dep_assets.Contains(dep))
                {
                    continue;
                }

                if (IsFilter(dep))
                {
                    continue;
                }

                dep_assets.Add(dep);
            }
        }

        dep_assets.Sort();
        return dep_assets;
    }

    static bool IsFilter(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return true;
        }

        if (path.EndsWith(".cs"))
        {
            return true;
        }

        string extension = Path.GetExtension(path);
        extension = extension.ToLower();
        string folder = "";
        if (string.Equals(extension, ".unity"))
        {
            folder = "scenes";
        }
        string bundleName = PackAssetBundleUtlis.GetBundleName(path, folder);
        string bundlePath = string.Format("{0}/{1}", PackAssetBundle.bundleBuildFolder, bundleName);

        if (File.Exists(bundlePath))
        {
            return true;
        }

        return false;
    }

}

//public class ShaderCheck
//{
//    public class MaterialShaderInfo
//    {
//        public string materail_path;
//        public string shader_name = "";
//    }

//    static string shader_folder = "builtin_shaders-5.6.1f1";
//    static string material_shaderFile = "AllMat_Shader.txt";
//    static List<MaterialShaderInfo> shaderInfoList = new List<MaterialShaderInfo>();

//    [MenuItem("Tools/[11]查找build-in的shader")]
//    static void Check()
//    {
//        List<string> all_mats = EditorCommon.GetAllFiles("Assets/", "*.mat");

//        Debug.LogError(all_mats.Count);
//        for ( int i=0; i< all_mats.Count; i++ )
//        {
//            MaterialShaderInfo shaderInfo = GetMaterial(all_mats[i]);
//            if ( shaderInfo == null )
//            {
//                continue;
//            }

//            if (!shaderInfoList.Contains(shaderInfo))
//            {
//                shaderInfoList.Add(shaderInfo);
//            }
//        }
//        Write2File();
//        Debug.LogError(shaderInfoList.Count);
//    }

//    [MenuItem("Tools/[11]修复build-in的shader")]
//    static void Repair()
//    {
//        ReadFile();
//        foreach (MaterialShaderInfo shaderinfo in shaderInfoList )
//        {
//            Material mat = AssetDatabase.LoadAssetAtPath<Material>(shaderinfo.materail_path);
//            Shader shader = Shader.Find(shaderinfo.shader_name);

//            if ( mat == null || shader == null )
//            {
//                continue;
//            }

//            mat.shader = shader;
//        }
//    }

//    static void Write2File()
//    {
//        BuildCommon.WriteJsonToFile("Assets", material_shaderFile, shaderInfoList);
//    }

//    static void ReadFile()
//    {
//        shaderInfoList = BuildCommon.ReadJsonFromFile<List<MaterialShaderInfo>>("Assets/" + material_shaderFile);
//    }



//    static MaterialShaderInfo GetMaterial(string materialPath)
//    {
//        if ( string.IsNullOrEmpty(materialPath) )
//        {
//            return null;
//        }

//        Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
//        if ( mat == null || mat.shader == null)
//        {
//            return null;
//        }

//        string shader_path = AssetDatabase.GetAssetPath(mat.shader);
//        if ( string.IsNullOrEmpty(shader_path) || !shader_path.Contains(shader_folder) )
//        {
//            return null;
//        }

//        MaterialShaderInfo shaderInfo = new MaterialShaderInfo();
//        shaderInfo.materail_path = materialPath;
//        shaderInfo.shader_name = mat.shader.name;

//        return shaderInfo;
//    }
//}