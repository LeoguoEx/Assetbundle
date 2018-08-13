
/**************************************************************************************************
	Copyright (C) 2017 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：MD5Utils.cs;
	作	者：W_X;
	时	间：2017 - 11 - 27;
	注	释：;
**************************************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ArtResourceInfo
{
    public string mFilePath; 
    public string mFileMD5;
    public string mTimeDate;
}

public class ArtResourceFileList
{
    public string m_BuildTime = "";
    public Dictionary<string, string> m_ResourceInfoList = new Dictionary<string, string>();

    public string GetMD5(string path)
    {
        if ( string.IsNullOrEmpty(path) )
        {
            return "";
        }

        string md5;
        m_ResourceInfoList.TryGetValue(path, out md5);

        return md5;
    }
}

public class MD5Utils
{
    private static List<string> s_AllArtsFiles = new List<string>();
    private static ArtResourceFileList s_FileList = null;

    public static void BuildAllArtsFileList()
    {
        s_AllArtsFiles.Clear();

        DateTime dt1 = System.DateTime.UtcNow;
        DisposeSceneRes();

        DateTime dt2 = System.DateTime.UtcNow;
        DisposeResourceFolder();

        DateTime dt3 = System.DateTime.UtcNow;
        ArtResourceFileList filelist = BuildMD5();

        DateTime dt4 = System.DateTime.UtcNow;
        BuildCommon.WriteJsonToFile("Assets", "ArtsFileList.txt", filelist, true);

        DateTime dt5 = System.DateTime.UtcNow;

        string info = string.Format("MD5计算完成\n总共{0}个文件\n耗时:{1}分钟\n其中：\n"
            , s_AllArtsFiles.Count, (dt5 - dt1).TotalMinutes.ToString("f1"));

        info = string.Format("{0}分析场景资源耗时：{1}秒\n", info, (dt2 - dt1).TotalSeconds.ToString("f1"));
        info = string.Format("{0}分析Resource资源耗时：{1}秒\n", info, (dt3 - dt2).TotalSeconds.ToString("f1"));
        info = string.Format("{0}生成MD5耗时：{1}秒\n", info, (dt4 - dt3).TotalSeconds.ToString("f1"));
        info = string.Format("{0}保存MD5文件耗时：{1}秒\n", info, (dt5 - dt4).TotalSeconds.ToString("f1"));

        EditorUtility.DisplayDialog("计算完成", info, "好的");
    }

    public static string GetMD5FromFile(string path)
    {
        if ( string.IsNullOrEmpty(path) )
        {
            return "";
        }

        if (s_FileList == null)
        {
            s_FileList = BuildCommon.ReadJsonFromFile<ArtResourceFileList>("Assets/" + "ArtsFileList.txt");
        }

        if ( s_FileList == null )
        {
            return "";
        }

        return s_FileList.GetMD5(path);
    }

    public static string GetMD5(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return "";
        }

        if (!File.Exists(filePath))
        {
            return "";
        }

        string hashStr = string.Empty;
        FileStream fs = null;

        try
        {
            fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(fs);
            hashStr = ByteArrayToHexString(hash);
            fs.Close();
            fs.Dispose();
        }
        catch (System.Exception ex)
        {
            if (fs != null)
                fs.Close();
        }

        return hashStr;
    }

    public static string GetMD5(byte[] buffer)
    {
        string hashStr = string.Empty;

        try
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(buffer);
            hashStr = ByteArrayToHexString(hash);
        }
        catch
        {

        }
        return hashStr;
    }

    public static string GetMD5FromValue(string value)
    {
        if ( string.IsNullOrEmpty(value) )
        {
            return "";
        }

        return GetMD5(System.Text.Encoding.Default.GetBytes(value));
    }

    static string GetUnityFileMD5(string filePath)
    {
        string metaFile = filePath + ".meta";
        string fileMD5 = GetMD5(filePath);

        if (File.Exists(metaFile))
        {
            return fileMD5;
        }

        string fileMetaMD5 = GetMD5(metaFile);

        string md5 = fileMD5 + fileMetaMD5;

        string hashStr = string.Empty;
        hashStr = GetMD5(System.Text.Encoding.Default.GetBytes(md5));

        return hashStr;
    }

    static void DisposeResourceFolder()
    {
        string title = "分析Game_Prefab资源";
        int count = ResourceConst.PackResourceInfoGroups.Length;
        for (int i = 0; i < count; i++)
        {
            ResourceConst.PackResourceInfo info = ResourceConst.PackResourceInfoGroups[i];
            if (info == null)
            {
                continue;
            }

            DisposeFolder(ResourceConst.GetResourcePath(info.path), i, count, title, info.suffix);
        }
        EditorUtility.ClearProgressBar();
    }

    static void DisposeFolder(string path, int index, int count, string title, string suffix = "")
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        bool default_search = string.IsNullOrEmpty(suffix);

        string searchPattern = default_search ? "*" : suffix;

        float basic_progress = (float)index / count;
        float step_scale = (1f / count);

        string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        float step_progress = (1f / files.Length);

        for (int i = 0; i < files.Length; i++)
        {
            float progress = basic_progress + step_scale * i * step_progress;
            string message = string.Format("{0}({1}/{2})", path, i, files.Length);
            DisposeAsset(files[i]);
            EditorUtility.DisplayProgressBar(title, message, progress);
        }
    }

    static void DisposeSceneRes()
    {
        string[] scenePaths = Directory.GetFiles(ResourceConst.ScenePath, "*.unity");
        if (scenePaths == null || scenePaths.Length < 1)
        {
            return;
        }

        string[] sceneConfPaths = Directory.GetFiles(ResourceConst.ScenePath, "*.asset");
        foreach (string sceneConf in sceneConfPaths)
        {
            DisposeAsset(sceneConf);
        }

        for (int i = 0; i < scenePaths.Length; i++)
        {
            string scenePath = scenePaths[i];
            PackAssetBundleUtlis.ShowProgress(i, scenePaths.Length, "分析场景资源：", scenePath);

            DisposeAsset(scenePath);
        }
        EditorUtility.ClearProgressBar();
    }

    static void DisposeAsset(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            return;
        }

        string[] deps = AssetDatabase.GetDependencies(scenePath);
        for (int i = 0; i < deps.Length; i++)
        {
            string dep = deps[i];
            if (!s_AllArtsFiles.Contains(dep))
            {
                s_AllArtsFiles.Add(dep);
            }
        }
    }

    static ArtResourceFileList BuildMD5()
    {
        ArtResourceFileList filelist = new ArtResourceFileList();
        filelist.m_BuildTime = System.DateTime.Now.ToString();

        for ( int i=0; i<s_AllArtsFiles.Count; i++ )
        {
            string path = s_AllArtsFiles[i];
            string md5 = GetUnityFileMD5(path);
            //ArtResourceInfo info = new ArtResourceInfo();
            //info.mFilePath = path;
            //info.mFileMD5 = md5;

            filelist.m_ResourceInfoList.Add(path, md5);

            PackAssetBundleUtlis.ShowProgress(i, s_AllArtsFiles.Count, "计算MD5：", path);
        }
        EditorUtility.ClearProgressBar();

        return filelist;
    }    

    static string ByteArrayToHexString(byte[] values)
    {
        StringBuilder sb = new StringBuilder();

        foreach (byte value in values)
        {
            sb.AppendFormat("{0:X2}", value);
        }

        return sb.ToString();
    }
}