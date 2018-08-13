
/**************************************************************************************************
	Copyright (C) 2017 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：PackAssetBundleUtlis.cs;
	作	者：W_X;
	时	间：2017 - 07 - 05;
	注	释：;
**************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class PackAssetBundleUtlis
{
    [MenuItem("Tools/清除本地数据库数据", priority = 1000)]
    static void ClearLocalPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    
    public static void CheckAllBundles(bool tip_dialog = true)
    {
        DateTime dt1 = System.DateTime.UtcNow;
        // 获取所有的Bundle包;
        List<string> bundles = EditorCommon.GetAllFiles(ResourceConst.BundleFolder, "*.*");
        string folder = string.Format("{0}/../", Application.dataPath);
        folder = Path.GetFullPath(folder);
        folder = folder.Replace("\\", "/");

        List<string> failed = new List<string>();
        for( int i=0; i<bundles.Count; i++ )
        {
            string filePath = bundles[i];
            if ( filePath.EndsWith(".dat") || filePath.EndsWith(".txt"))
            {
                continue;
            }
            bool result = CheckBundle(folder, filePath);
            if ( !result )
            {
                failed.Add(filePath);
            }

            ShowProgress(i, bundles.Count, "检测Bundle", "检测Bundle");
        }

        EditorUtility.ClearProgressBar();

        DateTime dt2 = System.DateTime.UtcNow;
        string strInfo = failed.Count < 1 ? "检测成功，没有错误的Bundle包" : string.Format("检测失败，{0}个Bundle包错误，详情请看Log", failed.Count);
        strInfo = string.Format("检测耗时：{0}秒\n{1}", (dt2-dt1).TotalSeconds.ToString("f1"), strInfo);

        // 为了使打资源包过程中不停顿，在资源检查错误或者要求有提示信息的时候，才弹出提示框
        if (failed.Count >= 1 || tip_dialog)
        {
            EditorUtility.DisplayDialog("检测完成", strInfo, "好的");
        }
    }

    public static void ShowProgress(int index, int count, string title, string content)
    {
        float progress = (float)index / count;
        EditorUtility.DisplayProgressBar(title, content, progress);
    }

    public static bool IsFilterSceneRes(string dep, string file)
    {
        if ( string.IsNullOrEmpty(dep) || string.Equals(dep, file))
        {
            return true;
        }

        // 只记录贴图、动作、音效资源;
        string extension = BuildCommon.GetExtension(dep);
        if (BuildCommon.IsTextureAsset(dep,extension)
            || BuildCommon.IsAudioAsset(dep,extension)
            || BuildCommon.IsShaderAsset(dep,extension)
            //|| BuildCommon.IsBigModelAsset(dep,extension)
            )
        {
            return false;
        }

        return true;
    }

    public static bool IsFilterAsset(string file)
    {
        if ( string.IsNullOrEmpty(file) )
        {
            return true;
        }

        string sub = Path.GetExtension(file);
        sub = sub.ToLower();
        if (sub == ".meta")
            return true;
        if (sub == ".cs")
            return true;

        return false;
    }

    public static bool IsFilterAssetDep(string dep, string file)
    {
        if (string.IsNullOrEmpty(dep) || string.Equals(dep, file))
        {
            return true;
        }

        // 只记录贴图、动作、音效资源;
        string extension = BuildCommon.GetExtension(dep);
        if (BuildCommon.IsTextureAsset(dep,extension)
            || BuildCommon.IsAnimAsset(dep,extension)
            || BuildCommon.IsAudioAsset(dep,extension)
            || BuildCommon.IstTTFFontAsset(dep,extension)
            || BuildCommon.IsShaderAsset(dep,extension)
            || BuildCommon.IsAtlasAsset(dep,extension)
            || BuildCommon.IsBigModelAsset(dep,extension)
            )
        {
            return false;
        }

        return true;
    }

    public static string GetBundleName(string path, string folder = "")
    {
        if ( string.IsNullOrEmpty(path) )
        {
            return "";
        }

        // Shader统一打包;
        string extension = BuildCommon.GetExtension(path);
        if ( BuildCommon.IsShaderAsset(path,extension))
        {
            return ResourceConst.bundleShader;
        }

        string bundleName = path.ToLower();
        string fileName = Path.GetFileNameWithoutExtension(bundleName);
        if ( !string.IsNullOrEmpty(folder) )
        {
            bundleName = string.Format("{0}/{1}{2}", folder, fileName, ResourceConst.BundleExtensions);

            return bundleName;
        }

        if (bundleName.StartsWith(ResourceConst.AssetResourceName.ToLower()))
        {
            bundleName = bundleName.Substring(ResourceConst.AssetResourceName.Length);
        }

        string bundleExtension = Path.GetExtension(bundleName);
        bundleName = bundleName.Trim();
        bundleName = bundleName.Replace('\\', '/');
        bundleName = bundleName.Replace(" ", "_");
        bundleName = bundleName.Replace("#", "_");
        bundleName = bundleName.Replace(bundleExtension, ResourceConst.BundleExtensions);

        if (BuildCommon.IsTextureAsset(path, extension))
        {
            bundleName = bundleName.Replace("_rgb", "_texture");
            bundleName = bundleName.Replace("_alpha", "_texture");
        }

        //if ( path.Contains("bg_2.jpg") )
        //{
        //    Debug.LogErrorFormat("{0} -------------> {1}", path, bundleName);
        //}

        return bundleName;
    }

    public static void DisposeFiles(string folder)
    {
        List<string> old_abs = EditorCommon.GetAllFiles(folder, "*.ab");

        List<string> abs = EditorCommon.GetAllFiles(folder, "*" + ResourceConst.BundleExtensions);
        if (abs == null || abs.Count < 1)
        {
            return;
        }

        foreach ( string old_ab in old_abs )
        {
            if ( !abs.Contains(old_ab) )
            {
                abs.Add(old_ab);
            }
        }

        string fielName = Path.GetFileNameWithoutExtension(folder);

        string path = string.Format("{0}/{1}", folder, fielName);
        AssetBundleManifest abm = GetAssetBundleManifest(path);
        if (abm == null)
        {
            return;
        }
        string[] files = abm.GetAllAssetBundles();

        foreach (string file in files)
        {
            string bundlePath = string.Format("{0}/{1}", folder, file);
            if (abs.Contains(bundlePath))
            {
                abs.Remove(bundlePath);
            }
        }

        for ( int i=0; i<abs.Count; i++ )
        {
            string ab = abs[i];
            System.IO.File.Delete(ab);
            System.IO.File.Delete(string.Format("{0}.manifest", ab));
            ShowProgress(i, abs.Count, "删除无用文件：", ab);
        }
        EditorUtility.ClearProgressBar();
    }

    public static AssetBundleManifest GetAssetBundleManifest(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string wwwPath = string.Format("{0}/../{1}", Application.dataPath, path);
        AssetBundle bundle = AssetBundle.LoadFromFile(wwwPath);
        if (bundle == null)
        {
            return null;
        }

        AssetBundleManifest abm = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        bundle.Unload(false);

        return abm;
    }

    public static void CopyFolder( string src, string dest )
    {
        if ( string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dest) )
        {
            return;
        }

        if ( !Directory.Exists(src) )
        {
            return;
        }

        string folderName = Path.GetFileName(src);        
        dest = string.Format("{0}/{1}", dest, folderName);

        FileUtils.ClearDirectory(dest);

        FileUtils.CopyDirectory(src, dest);
    }

    public static void CopyAssetBundle(string src, string dest, string conf = "")
    {
        if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dest))
        {
            return;
        }

        if (!Directory.Exists(src))
        {
            return;
        }

        string folderName = Path.GetFileName(src);
        dest = string.Format("{0}/{1}", dest, folderName);

        // 清除文件夹;
        FileUtils.ClearDirectory(dest);

        // 拷贝获取AssetBundleManifest文件;
        string topManifest = string.Format("{0}/{1}", src, folderName);
        CopyFile(topManifest, dest + "/" + folderName);

        // 拷贝FileList文件;
        string fileList = string.Format("{0}/{1}", src, ResourceConst.FileListName);
        string newFileList = string.Format("{0}/{1}", dest, ResourceConst.FileListName);
        CopyFile(fileList, newFileList);

        // 拷贝Conf文件夹;
        if( !string.IsNullOrEmpty(conf) )
        {
            string confFolder = string.Format("{0}/{1}", src, conf);
            CopyFolder(confFolder, dest);
        }

        // 拷贝gitlog和md5文件;
        CopyFile( string.Format("{0}/gitlog.txt", src), string.Format("{0}/gitlog.txt", dest));
        //CopyFile(string.Format("{0}/md5_info.txt", src), string.Format("{0}/md5_info.txt", dest));

        // 获取AssetBundleManifest;
        AssetBundleManifest manifest = PackBundleTools.GetAssetBundleManifest(topManifest);
        if (manifest == null)
        {
            return;
        }

        string[] allBundleKeyList = manifest.GetAllAssetBundles();
        for ( int i=0; i<allBundleKeyList.Length; i++ )
        {
            string file = allBundleKeyList[i];
            if ( string.IsNullOrEmpty(file) )
            {
                continue;
            }

            string srcFile = string.Format("{0}/{1}", src, file);
            string destFile = string.Format("{0}/{1}", dest, file);
            ShowProgress(i, allBundleKeyList.Length, "拷贝", file);
            CopyFile(srcFile, destFile);
        }
        EditorUtility.ClearProgressBar();
    }

    public static void CopyFile( string src, string dest )
    {
        if ( string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dest))
        {
            return;
        }

        if ( !File.Exists(src) )
        {
            return;
        }

        if ( File.Exists(dest) )
        {
            File.Delete(dest);
        }

        string dir = Path.GetDirectoryName(dest);
        if ( !Directory.Exists(dir) )
        {
            Directory.CreateDirectory(dir);
        }

        File.Copy(src, dest);
    }

    public static bool CheckBundle(string folder, string filePath)
    {
        if ( string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(filePath) )
        {
            return false;
        }

        string wwwPath = string.Format("file://{0}/{1}", folder, filePath);

        WWW www = new WWW(wwwPath);
        if ( www == null )
        {
            return false;
        }

        if ( !string.IsNullOrEmpty(www.error) )
        {
            Debug.LogErrorFormat("{0}\n{1}", filePath, www.error);
            www.Dispose();
            return false;
        }

        if (www.assetBundle == null)
        {
            Debug.LogErrorFormat("{0}\nNo AssetBundle", filePath);
            www.Dispose();
            return false;
        }

        www.assetBundle.Unload(true);
        www.Dispose();

        return true;
    }

    public static string CompareFileList(BundleUpdateMode mode = BundleUpdateMode.Update_CRC)
    {
        string old_FileListPath = string.Format("{0}/{1}", ResourceConst.BundleFolder, ResourceConst.FileListName);
        string new_fileListPath = string.Format("{0}/{1}", PackAssetBundle.bundleBuildFolder, ResourceConst.FileListName);

        if ( !File.Exists(new_fileListPath) || !File.Exists(old_FileListPath) )
        {
            return "1";
        }

        FileList new_fileList = PackBundleTools.GetAssetBundleFileList(new_fileListPath);
        FileList old_fileList = PackBundleTools.GetAssetBundleFileList(old_FileListPath);

        if ( new_fileList == null || old_fileList == null )
        {
            return "2";
        }

        FileListCompareData compareData = FileList.Compare(new_fileList, old_fileList, true, mode);
        if ( compareData == null )
        {
            return "3";
        }

        string update_size = Size2String(compareData.add_size + compareData.modifiy_size);
        string add_size = Size2String(compareData.add_size );
        string mod_size = Size2String(compareData.modifiy_size);
        string del_size = Size2String(compareData.delete_size);

        string strInfo = string.Format("本地打包较上次需要更新{0}（{1}）", update_size, mode);
        strInfo = string.Format("{0}\n其中：\n增加：{1}", strInfo, add_size);
        strInfo = string.Format("{0}\n改变：{1}", strInfo, mod_size);
        strInfo = string.Format("{0}\n删除：{1}", strInfo, del_size);

        return strInfo;
    }

    private static string Size2String(uint size)
    {
        float scale = 1f / 1024f;

        float mb_size = size * scale * scale;

        return string.Format("{0}MB", mb_size.ToString("0.00"));
    }

}