
/**************************************************************************************************
	Copyright (C) 2016 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：PackBundleCommon.cs;
	作	者：W_X;
	时	间：2017 - 02 - 09;
	注	释：Bundle打包工具类;
**************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class AssetBundleBuildEx
{
    public string assetBundleName;

    public string assetBundleVariant;

    public List<string> assetNames;

    public HashSet<string> assetNamesSet;

    public bool force_build;

    public int build_count;
}

public class PackBundleTools
{
    //[MenuItem("Bundles/DisposeAllFbxs2")]
    //public static void DisposeAllFbxs2()
    //{
    //    BuildCommon.ChangeMaterial(Selection.activeGameObject);
    //}

    //[MenuItem("Bundles/DisposeAllFbxs")]
    //public static void DisposeAllFbxs()
    //{
    //    List<string> fbxs = EditorCommon.GetAllFiles("Assets/Arts/", "*.fbx");
    //    Debug.LogFormat("{0}", fbxs.Count);

    //    for( int i=0; i< fbxs.Count; i++ )
    //    {
    //        string fbxPath = fbxs[i];
    //        GameObject gObj = AssetDatabase.LoadAssetAtPath(fbxPath, typeof(GameObject)) as GameObject;
    //        BuildCommon.ChangeMaterial(gObj);

    //        PackAssetBundleUtlis.ShowProgress(i, fbxs.Count, "处理Fbx", "处理Fbx");

    //        if (i % 50 == 0)
    //        {
    //            AssetDatabase.SaveAssets();
    //            AssetDatabase.Refresh();
    //        }
    //    }
    //    EditorUtility.ClearProgressBar();
    //}

    public static int GetAssetBundleBuildPriority(AssetBundleBuild build)
    {
        if (build.assetNames == null || build.assetNames.Length < 1)
        {
            return -1;
        }

        string path = build.assetNames[0];
        if ( string.IsNullOrEmpty(path) )
        {
            return -1;
        }

        string extension = System.IO.Path.GetExtension(path);
        extension = extension.ToLower();

        if ( string.Equals(extension, ".unity") )
        {
            return 100;
        }

        if ( BuildCommon.IsShaderAsset(path, extension) )
        {
            return 0;
        }

        if (BuildCommon.IsTextureAsset(path, extension, false))
        {
            return 1;
        }

        if (BuildCommon.IsAnimAsset(path, extension))
        {
            return 2;
        }

        if (BuildCommon.IsAudioAsset(path, extension))
        {
            return 3;
        }

        if (BuildCommon.IsMaterialAsset(path, extension))
        {
            return 4;
        }

        if (BuildCommon.IsModelAsset(path, extension))
        {
            return 5;
        }

        if (BuildCommon.IstTTFFontAsset(path, extension))
        {
            return 6;
        }

        if (BuildCommon.IsUIFontAsset(path, extension))
        {
            return 21;
        }

        if (BuildCommon.IsAtlasAsset(path, extension))
        {
            return 22;
        }

        if (BuildCommon.IsPrefabAsset(path, extension))
        {
            return 99;
        }
        //Debug.Log(path);
        return 20;
    }

#if UNITY_ANDROID
    public static BuildTargetGroup group = BuildTargetGroup.Android;
    public static BuildTarget platform = BuildTarget.Android;
#elif UNITY_IPHONE
    public static BuildTargetGroup group = BuildTargetGroup.iOS;
	    public static BuildTarget platform = BuildTarget.iOS;
#else
    public static BuildTargetGroup group = BuildTargetGroup.Standalone;
        public static BuildTarget platform = BuildTarget.StandaloneWindows;
#endif

    private const int BATCH_COUNT= 2000;
    private Dictionary<string, AssetBundleBuildEx> mBundleDictionary = new Dictionary<string, AssetBundleBuildEx>();

    private List<AssetBundleBuild> mBundleBuildList = new List<AssetBundleBuild>();
	private HashSet<string> m_AssetList = new HashSet<string>();
   
    public static string GetBundleName(string path, bool isRes, bool addExtension = false)
    {
        string filePath = path;
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(path);
        string folderPath= isRes ? "res/" : "";
        folderPath = folderPath.ToLower();

        if (isRes || addExtension)
        {
            //如果是资源类，自动加上扩展名
            fileName = fileName + extension;
            fileName = fileName.Replace('.', '_');
        }
        fileName = fileName.Replace('#', '_');
        fileName = fileName.Replace(" ", "");

        fileName = fileName.ToLower();

        filePath = folderPath + fileName + ".ab";
        return filePath;
    }

    public static AssetBundleManifest GetAssetBundleManifest(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string wwwPath = string.Format("file://{0}/../{1}", Application.dataPath, path);
        WWW www = new WWW(wwwPath);

        if (www == null || www.assetBundle == null)
        {
            return null;
        }

        AssetBundleManifest abm = www.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        www.assetBundle.Unload(false);
        www.Dispose();

        return abm;
    }
    public static FileList GetAssetBundleFileList(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string wwwPath = string.Format("file://{0}/../{1}", Application.dataPath, path);
        WWW www = new WWW(wwwPath);

        if (www == null || www.assetBundle == null)
        {
            return null;
        }

        UnityEngine.Object obj = www.assetBundle.LoadAsset("FileList");
        if ( obj == null )
        {
            www.assetBundle.Unload(true);
            www.Dispose();
            return null;
        }

        FileList filelist = null;

        try
        {
            //filelist = LitJson.JsonMapper.ToObject<FileList>(obj.ToString());
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }

        www.assetBundle.Unload(false);
        www.Dispose();

        return filelist;
    }

    public static FileList GetAssetBundleFileListFromAbsPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string wwwPath = string.Format("file://{0}", path);        
        AssetBundle assetbundle = AssetBundle.LoadFromFile(path);
        UnityEngine.Object obj = assetbundle.LoadAsset("FileList");
        if (obj == null)
        {
            assetbundle.Unload(true);
            return null;
        }

        FileList filelist = null;

        try
        {
            //filelist = LitJson.JsonMapper.ToObject<FileList>(obj.ToString());
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }

        assetbundle.Unload(true);
        //www.Dispose();

        return filelist;
    }

    public static void DisposeFiles(string folder, string bundle)
    {
        List<string> abs = EditorCommon.GetAllFiles(folder, "*.ab");
        if (abs == null || abs.Count < 1)
        {
            return;
        }

        string path = string.Format("{0}/{1}", folder, bundle);
        AssetBundleManifest abm = PackBundleTools.GetAssetBundleManifest(path);
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
            //Debug.Log(file);
        }

        foreach (string ab in abs)
        {
            Debug.Log(ab);
            System.IO.File.Delete(ab);
            System.IO.File.Delete(string.Format("{0}.manifest", ab));
        }
    }

    public void Clear()
    {
        mBundleBuildList.Clear();
        mBundleDictionary.Clear();
        m_AssetList.Clear();
    }

    public int Count
    {
        get { return mBundleBuildList.Count; }
    }

    int AssetBundleBuildSort(AssetBundleBuild left, AssetBundleBuild right)
    {
        int left_priority = GetAssetBundleBuildPriority(left);
        int right_priority = GetAssetBundleBuildPriority(right);

        if ( left_priority < right_priority )
        {
            return -1;
        }
        else if ( left_priority > right_priority )
        {
            return 1;
        }

        return 0;
    }

    public static List<string> fbxList = new List<string>();

    public void DisposeFbx()
    {
        fbxList.Clear();
        AssetDatabase.Refresh();
        foreach ( AssetBundleBuildEx ab in mBundleDictionary.Values)
        {
            foreach ( string dep in ab.assetNames )
            {
                string extension = BuildCommon.GetExtension(dep);
                if ( !BuildCommon.IsModelAsset(dep, extension) )
                {
                    continue;
                }

                if ( fbxList.Contains(dep) )
                {
                    continue;
                }

                fbxList.Add(dep);
            }
        }
        DisposeAllFbx();
    }

    public static void DisposeAllFbx()
    {
        foreach (string fbx in fbxList)
        {
            GameObject gObj = AssetDatabase.LoadAssetAtPath(fbx, typeof(GameObject)) as GameObject;
            BuildCommon.ChangeMaterial(gObj);            
        }
    }
       
    public void BuildAssetBundle(string outputPath, BuildAssetBundleOptions opt = BuildAssetBundleOptions.None)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            return;
        }
        
        foreach ( AssetBundleBuildEx buildEx in mBundleDictionary.Values)
        {
            //if ( !buildEx.force_build && buildEx.build_count <= 1)
            //{
            //    continue;
            //}
            AssetBundleBuild assetbundle_build = new AssetBundleBuild();
            assetbundle_build.assetNames = buildEx.assetNames.ToArray();
            assetbundle_build.assetBundleName = buildEx.assetBundleName;
            assetbundle_build.assetBundleVariant = buildEx.assetBundleVariant;
            mBundleBuildList.Add(assetbundle_build);
        }
        mBundleBuildList.Sort(AssetBundleBuildSort);
        
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        int batchCount = 0;
        int bundleCount = mBundleBuildList.Count;
        AssetBundleBuild[] batchBundleList;
        bool complete = false;

        int batch_count = BATCH_COUNT;
        if ( SystemInfo.processorCount >= 8 && SystemInfo.systemMemorySize >= 16000 )
        {
            batch_count *= 100;
        }

        while (!complete)
        {
            complete = false;
            //每次打包数量增加1000
            batchCount += batch_count;
            if (batchCount >= bundleCount)
            {
                complete = true;
                batchCount = bundleCount;
            }
            batchBundleList = new AssetBundleBuild[batchCount];
            
            for (int i = 0; i < batchCount; i++)
            {
                batchBundleList[i] = mBundleBuildList[i];
            }
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, batchBundleList, opt, platform);
        }
    }

    public void PushAssetBuild(string assetPath, string bundleName, bool force_build = true)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return;
        }

        assetPath = assetPath.Replace('\\', '/');

        if ( m_AssetList.Contains(assetPath) )
        {
            AssetBundleBuildEx buildEx;
            if (mBundleDictionary.TryGetValue(bundleName, out buildEx))
            {
                buildEx.build_count += 1;
                if (!buildEx.force_build || force_build)
                {
                    buildEx.force_build = force_build;
                }
            }
            return;
		}
		m_AssetList.Add(assetPath);
        AddBundle2List(assetPath, bundleName, force_build);
    }

    public void AddBundle2List(string assetPath, string bundleName, bool force_build = true, string variant = "")
    {
        if (assetPath == null || string.IsNullOrEmpty(bundleName))
        {
            return;
        }

        AssetBundleBuildEx buildEx;
        if ( !mBundleDictionary.TryGetValue(bundleName, out buildEx) )
        {
            buildEx = new AssetBundleBuildEx();
            buildEx.assetBundleName = bundleName;
            buildEx.assetBundleVariant = variant;
            buildEx.assetNames = new List<string>();
            buildEx.assetNamesSet = new HashSet<string>();
            mBundleDictionary.Add(bundleName, buildEx);
        }

        buildEx.build_count += 1;
        if ( !buildEx.force_build || force_build)
        {
            buildEx.force_build = force_build;
        }

        if (!string.IsNullOrEmpty(assetPath) && !buildEx.assetNamesSet.Contains(assetPath))
        {
            buildEx.assetNames.Add(assetPath);
            buildEx.assetNamesSet.Add(assetPath);
        }

        //if (mBundleNameSet.Contains(bundleName))
        //{
        //    return;
        //}

        //AssetBundleBuild assetbundle_build = new AssetBundleBuild();
        //assetbundle_build.assetNames = assetPaths;
        //assetbundle_build.assetBundleName = bundleName;
        //assetbundle_build.assetBundleVariant = variant;

        //mBundleBuildList.Add(assetbundle_build);
        //mBundleNameSet.Add(bundleName);
    }

    public string[] GetAssetsByBundle(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return null;

        AssetBundleBuildEx buildEx;
        if (!mBundleDictionary.TryGetValue(bundleName, out buildEx)) return null;
        if (buildEx == null || buildEx.assetNames == null) return null;

        return buildEx.assetNames.ToArray();
    }
}