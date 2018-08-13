
/**************************************************************************************************
	Copyright (C) 2017 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：PackAssetBundle.cs;
	作	者：W_X;
	时	间：2017 - 07 - 05;
	注	释：;
**************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class PackAssetBundle
{
    private static PackBundleTools _packTools = new PackBundleTools();

    public static string bundleBuildFolder
    {
        get
        {
            return string.Format("{0}/{0}{1}/{0}", ResourceConst.PkgBundleFolder, ResourceConst.PkgBundleBuildFolder);
        }
    }

    static void Clear()
    {
        _packTools.Clear();
        ClearAllOtherFile();

        FileDepencies.Clear();
    }
    
    public static void TestDep()
    {
        if ( Selection.activeObject == null )
        {
            return;
        }
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if ( string.IsNullOrEmpty(path) )
        {
            return;
        }

        List<string> dep_PrefabList = new List<string>();
        List<string> dep_MatList = new List<string>();
        List<string> dep_OtherList = new List<string>();
        List<string> dep_TextureList = new List<string>();

        string[] deps = AssetDatabase.GetDependencies(path);
        foreach ( string dep in deps )
        {
            Debug.LogError(dep);
            string extension = System.IO.Path.GetExtension(dep);
            if ( BuildCommon.IsPrefabAsset(dep, extension) )
            {
                dep_PrefabList.Add(dep);
            }
            else if (BuildCommon.IsMaterialAsset(dep, extension))
            {
                dep_MatList.Add(dep);
            }
            else if (BuildCommon.IsTextureAsset(dep, extension))
            {
                dep_TextureList.Add(dep);
            }
            else
            {
                dep_OtherList.Add(dep);
            }
        }

        Print(dep_PrefabList);
        Print(dep_MatList);
        Print(dep_OtherList);
        Print(dep_TextureList);
    }

    static void Print(List<string> deps)
    {
        deps.Sort();
        string strInfo = "";
        foreach ( string dep in deps )
        {
            strInfo = string.Format("{0}\n{1}", strInfo, dep);            
        }

        Debug.Log(strInfo);
    }
    
    static IEnumerator PackAllAssetBundleAsync(bool check)
    {
        string srcConfFolder = GetConf();
        if (string.IsNullOrEmpty(srcConfFolder))
        {
            yield break;
        }

        if (!Directory.Exists(ResourceConst.PkgBundleFolder))
        {
            Directory.CreateDirectory(ResourceConst.PkgBundleFolder);
        }

        Clear();

        DateTime dt1 = System.DateTime.UtcNow;
        // 分析场景;
        yield return EditorCoroutineRunner.StartEditorCoroutine( DisposeSceneAsync());

        DateTime dt2 = System.DateTime.UtcNow;
        yield return EditorCoroutineRunner.StartEditorCoroutine(DisposeResourcesFolderAsync());

        DateTime dt3 = System.DateTime.UtcNow;
        Build();

        DateTime dt4 = System.DateTime.UtcNow;

        // 拷贝Conf;
        PackAssetBundleUtlis.CopyFolder(srcConfFolder, bundleBuildFolder);
        FileListUtility.BuildFileList(false);
        DateTime dt5 = System.DateTime.UtcNow;

        string confFolder = Path.GetFileName(srcConfFolder);
        PackAssetBundleUtlis.CopyAssetBundle(bundleBuildFolder, ResourceConst.PkgBundleFolder, confFolder);

        DateTime dt6 = System.DateTime.UtcNow;

        string info = string.Format("bundle打包完成\n总共{0}个文件\n耗时:{1}分钟\n其中：\n"
            , _packTools.Count, (dt6 - dt1).TotalMinutes.ToString("f1"));

        info = string.Format("{0}分析场景资源耗时：{1}秒\n", info, (dt2 - dt1).TotalSeconds.ToString("f1"));
        info = string.Format("{0}分析Resource资源耗时：{1}秒\n", info, (dt3 - dt2).TotalSeconds.ToString("f1"));
        info = string.Format("{0}打包AssetBundle耗时：{1}秒\n", info, (dt4 - dt3).TotalSeconds.ToString("f1"));
        info = string.Format("{0}生成FileList耗时：{1}秒\n", info, (dt5 - dt4).TotalSeconds.ToString("f1"));
        info = string.Format("{0}拷贝AssetBundle耗时：{1}秒\n", info, (dt6 - dt5).TotalSeconds.ToString("f1"));

        EditorUtility.DisplayDialog("打包完成", info, "好的");

        if (check)
        {
            PackAssetBundleUtlis.CheckAllBundles();
        }
    }

    static IEnumerator DisposeSceneAsync()
    {
        string[] scenePaths = Directory.GetFiles(ResourceConst.ScenePath, "*.unity");
        if (scenePaths == null || scenePaths.Length < 1)
        {
            yield break;
        }

        for (int i = 0; i < scenePaths.Length; i++)
        {
            string scenePath = scenePaths[i];
            PackAssetBundleUtlis.ShowProgress(i, scenePaths.Length, "分析场景：", scenePath);
            DisposeSingleScene(scenePath);
            yield return 1;
        }

        string[] sceneConfPaths = Directory.GetFiles(ResourceConst.ScenePath, "*.asset");
        if (sceneConfPaths == null || sceneConfPaths.Length < 1)
        {
            yield break;
        }

        foreach (string sceneConf in sceneConfPaths)
        {
            _packTools.PushAssetBuild(sceneConf, ResourceConst.bundleSceneConf);
        }

        EditorUtility.ClearProgressBar();
    }

    static IEnumerator DisposeResourcesFolderAsync()
    {
        string title = "分析resources";
        int count = ResourceConst.PackResourceInfoGroups.Length;
        for (int i = 0; i < count; i++)
        {
            ResourceConst.PackResourceInfo info = ResourceConst.PackResourceInfoGroups[i];
            if (info == null)
            {
                continue;
            }

            //PackAssetBundleUtlis.ShowProgress(i, resourceGroups.Length, "", resourceGroups[i]);
            DisposeFolder(ResourceConst.GetResourcePath(info.path), i, count, title, info.suffix, info.check_dep);
            yield return 1;
        }
        EditorUtility.ClearProgressBar();
    }

    public static void PackAllAssetBundle(bool check, bool tip_dialog = true)
    {
        string srcConfFolder = GetConf(tip_dialog);
        if (string.IsNullOrEmpty(srcConfFolder))
        {
            return;
        }

        if ( !Directory.Exists(ResourceConst.PkgBundleFolder) )
        {
            Directory.CreateDirectory(ResourceConst.PkgBundleFolder);
        }

        Clear();

        DateTime dt1 = System.DateTime.UtcNow;
        // 分析场景;
        DisposeScene();

        DateTime dt2 = System.DateTime.UtcNow;
        DisposeResourcesInfoFolder();

        // 检查用到的Shader;
        if ( !CheckShader() )
        {
            return;
        }

        DateTime dt3 = System.DateTime.UtcNow;
        Build();

        DateTime dt4 = System.DateTime.UtcNow;

        // 拷贝Conf;
        PackAssetBundleUtlis.CopyFolder(srcConfFolder, bundleBuildFolder);
        FileListUtility.BuildFileList(false);

        GitUtility.PrintGitToData();
        //GitUtility.PrintGitModifyDetailToData_2();

        // 记录比较;
        string compareInfo = PackAssetBundleUtlis.CompareFileList( BundleUpdateMode.Update_CRC );
        string compareInfo2 = PackAssetBundleUtlis.CompareFileList(BundleUpdateMode.Update_MD5);
        string compareInfo3 = PackAssetBundleUtlis.CompareFileList(BundleUpdateMode.Update_CRCANDMD5);
        string compareInfo4 = PackAssetBundleUtlis.CompareFileList(BundleUpdateMode.Update_CRCORMD5);

        DateTime dt5 = System.DateTime.UtcNow;

        string confFolder = Path.GetFileName(srcConfFolder);
        PackAssetBundleUtlis.CopyAssetBundle(bundleBuildFolder, ResourceConst.PkgBundleFolder, confFolder);

        DateTime dt6 = System.DateTime.UtcNow;
//		BuildMiniFileList.Build();
		DateTime dt7 = System.DateTime.UtcNow;
//		ExportMiniClient.Export();
		DateTime dt8 = System.DateTime.UtcNow;

		string info = string.Format("bundle打包完成\n总共{0}个文件\n耗时:{1}分钟\n其中：\n"
            , _packTools.Count, (dt6 - dt1).TotalMinutes.ToString("f1"));

        info = string.Format("{0}分析场景资源耗时：{1}秒\n", info, (dt2 - dt1).TotalSeconds.ToString("f1"));
        info = string.Format("{0}分析Resource资源耗时：{1}秒\n", info, (dt3 - dt2).TotalSeconds.ToString("f1"));
        info = string.Format("{0}打包AssetBundle耗时：{1}秒\n", info, (dt4 - dt3).TotalSeconds.ToString("f1"));
        info = string.Format("{0}生成FileList耗时：{1}秒\n", info, (dt5 - dt4).TotalSeconds.ToString("f1"));
        info = string.Format("{0}拷贝AssetBundle耗时：{1}秒\n", info, (dt6 - dt5).TotalSeconds.ToString("f1"));
		info = string.Format("{0}生成FileList2耗时：{1}秒\n", info, (dt7 - dt6).TotalSeconds.ToString("f1"));
		info = string.Format("{0}导出微端资源耗时：{1}秒\n", info, (dt8 - dt7).TotalSeconds.ToString("f1"));

		info = string.Format("{0}\n{1}\n", info, compareInfo );
        info = string.Format("{0}\n{1}\n", info, compareInfo2);
        info = string.Format("{0}\n{1}\n", info, compareInfo3);
        info = string.Format("{0}\n{1}\n", info, compareInfo4);

        if(tip_dialog)
        {
            EditorUtility.DisplayDialog("打包完成", info, "好的");
        }

        if ( check )
        {
            PackAssetBundleUtlis.CheckAllBundles(tip_dialog);
        }
    }
    
    static void ClearAllOtherFile()
    {
        string root = ResourceConst.PkgBundleFolder;
        List<string> filterList = new List<string>();
        filterList.Add(".git");
        filterList.Add(ResourceConst.PkgBundleFolder);
        filterList.Add(string.Format("{0}{1}", ResourceConst.PkgBundleFolder, ResourceConst.PkgBundleBuildFolder));

        string[] files = Directory.GetFiles(root);
        foreach ( string file in files )
        {
            File.Delete(file);
        }

        string[] directories = Directory.GetDirectories(root);
        foreach (string directory in directories)
        {
            string name = Path.GetFileName(directory);
            if ( filterList.Contains(name) )
            {
                continue;
            }

            FileUtil.DeleteFileOrDirectory(directory);
        }
    }
    
    public static void CopyAssetBundle2StreamingAssets()
    {
        //string srcConfFolder = GetConf();
        //if ( string.IsNullOrEmpty(srcConfFolder) )
        //{
        //    return;
        //}

        //// 拷贝conf文件夹;
        //PackAssetBundleUtlis.CopyFolder(srcConfFolder, "Assets/StreamingAssets");

        PackAssetBundleUtlis.CopyFolder(string.Format("{0}/{0}", ResourceConst.PkgBundleFolder), "Assets/StreamingAssets");

        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("拷贝完成", "拷贝完成", "好的");
    }
    
    public static void OnlyCopyConf2StreamingAssets()
    {
        string srcConfFolder = GetConf();
        if (string.IsNullOrEmpty(srcConfFolder))
        {
            return;
        }

        DateTime dt1 = System.DateTime.UtcNow;

        // 拷贝conf文件夹;
        PackAssetBundleUtlis.CopyFolder(srcConfFolder, string.Format("{0}", bundleBuildFolder));
        PackAssetBundleUtlis.CopyFolder(srcConfFolder,  string.Format("{0}", ResourceConst.BundleFolder) );
        DateTime dt2 = System.DateTime.UtcNow;

        bool result = FileListUtility.BuildFileListByConf();
        DateTime dt3 = System.DateTime.UtcNow;
        //FileListUtility.BuildFileList(true);

        // 拷贝FileList文件;
        string fileList = string.Format("{0}/{1}", bundleBuildFolder, ResourceConst.FileListName);
        string newFileList = string.Format("{0}/{1}", ResourceConst.BundleFolder, ResourceConst.FileListName);
        PackAssetBundleUtlis.CopyFile(fileList, newFileList);
        GitUtility.PrintGitToData();
        DateTime dt4 = System.DateTime.UtcNow;

        if (result)
        {
            string info = string.Format("Conf打包完成，总时长{0}秒", (dt4 - dt1).TotalSeconds.ToString("f1"));
            EditorUtility.DisplayDialog("打包完成", info, "好的");
        }
        else
        {
            EditorUtility.DisplayDialog("打包失败", "Conf打包失败，详情请看Log！", "马上去看");

        }
    }

    static string GetConf(bool tip_dialog = true)
    {
        if(tip_dialog)
        {
            if (!EditorUtility.DisplayDialog("注意", "确认conf中的data_dat是最新的！", "我确认是最新的", "重新生成conf data_dat"))
            {
                return string.Empty;
            }
        }

#if UNITY_IPHONE       
        if (!EditorUtility.DisplayDialog("注意", "确认conf和develop平级目录！", "我确认", "不确定，我去看看"))
        {
            return string.Empty;
        } 
        string srcConfFolder = string.Format("{0}/../../conf/data_dat", Application.dataPath);
#else
        string srcConfFolder = @"T://data_dat";
#endif

        // 检查Conf文件夹;
        if (!Directory.Exists(srcConfFolder))
        {
            EditorUtility.DisplayDialog("骗子", "根本找不到 conf/data_dat 文件夹", "对不起");
            return string.Empty;
        }

        return srcConfFolder;
    }

    // 处理场景资源;
    static void DisposeScene()
    {
        string[] scenePaths = null;
        try
        {
            scenePaths = Directory.GetFiles(ResourceConst.ScenePath, "*.unity");
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        
        if (scenePaths == null || scenePaths.Length < 1)
        {
            return;
        }

        for (int i = 0; i < scenePaths.Length; i++)
        {
            string scenePath = scenePaths[i];
            PackAssetBundleUtlis.ShowProgress(i, scenePaths.Length, "分析场景：", scenePath);
            DisposeSingleScene(scenePath);
        }

        string[] sceneConfPaths = Directory.GetFiles(ResourceConst.ScenePath, "*.asset");
        if (sceneConfPaths == null || sceneConfPaths.Length < 1)
        {
            return;
        }
        
        foreach (string sceneConf in sceneConfPaths)
        {
            _packTools.PushAssetBuild(sceneConf, ResourceConst.bundleSceneConf);
        }

        EditorUtility.ClearProgressBar();
    }

    static void DisposeSingleScene(string scenPath)
    {
        if (string.IsNullOrEmpty(scenPath))
        {
            return;
        }

        string bundleName = string.Empty;

        // 打包依赖忽略文件;
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenPath);

        string[] deps = AssetDatabase.GetDependencies(scenPath);
        FileDepencies.AddDepencies(scenPath, deps);
        if (deps != null)
        {
            for (int i=0;i<deps.Length;i++)
            {
                string dep = deps[i];
                if (PackAssetBundleUtlis.IsFilterSceneRes(dep, scenPath))
                {
                    continue;
                }

                bundleName = PackAssetBundleUtlis.GetBundleName(dep);
                _packTools.PushAssetBuild(dep, bundleName, false);
            }
        }

        //打场景本身;
        bundleName = PackAssetBundleUtlis.GetBundleName(scenPath, "scenes");
        _packTools.PushAssetBuild(scenPath, bundleName);
    }

    //static void DisposeResourcesFolder()
    //{
    //    string[] resourceGroups = ResourceConst.PackResourceGroups;

    //    string title = "分析resources";
    //    for (int i = 0; i < resourceGroups.Length; i++)
    //    {
    //        //PackAssetBundleUtlis.ShowProgress(i, resourceGroups.Length, "", resourceGroups[i]);
    //        DisposeFolder(ResourceConst.GetResourcePath(resourceGroups[i]), i, resourceGroups.Length, title);
    //    }
    //    EditorUtility.ClearProgressBar();
    //}
    static void DisposeResourcesInfoFolder()
    {
        string title = "分析resources";
        int count = ResourceConst.PackResourceInfoGroups.Length;
        for (int i = 0; i < count; i++)
        {
            ResourceConst.PackResourceInfo info = ResourceConst.PackResourceInfoGroups[i];
            if (info == null)
            {
                continue;
            }

            //PackAssetBundleUtlis.ShowProgress(i, resourceGroups.Length, "", resourceGroups[i]);
            DisposeFolder(ResourceConst.GetResourcePath(info.path), i, count, title, info.suffix, info.check_dep);
        }
        EditorUtility.ClearProgressBar();
    }


    static void DisposeFolder(string path, int index, int count, string title, string suffix="", bool check_dep=true)
    {
        if ( string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        bool default_search = string.IsNullOrEmpty(suffix);

        string searchPattern = default_search ? "*" : suffix;

        float basic_progress = (float)index / count;
        float step_scale = (1f / count);

        string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        if (default_search)
        {
            files = GetFilterFiles(files);
        }
        float step_progress = (1f / files.Length);

        for (int i = 0; i < files.Length; i++)
        {
            float progress = basic_progress + step_scale * i * step_progress;
            string message = string.Format("{0}({1}/{2})", path, i, files.Length);
            DisposeAsset(files[i], check_dep);
            EditorUtility.DisplayProgressBar(title, message, progress);
        }
    }

    static string[] GetFilterFiles(string[] files)
    {
        if ( files == null )
        {
            return null;
        }

        List<string> fileList = new List<string>();
        foreach ( string file in files )
        {
            if ( file.EndsWith(".meta") )
            {
                continue;
            }
            fileList.Add(file);
        }

        return fileList.ToArray();
    }

    static void DisposeAsset(string path, bool check_dep = true)
    {
        if ( string.IsNullOrEmpty(path) )
        {
            return;
        }

        if ( PackAssetBundleUtlis.IsFilterAsset(path) )
        {
            return;
        }
        string bundleName = string.Empty;

        if (check_dep)
        {
            // 分析依赖;
            string[] deps = AssetDatabase.GetDependencies(path);
            FileDepencies.AddDepencies(path, deps);
            if (deps != null)
            {
                for (int i = 0; i < deps.Length; i++)
                {
                    string dep = deps[i];
                    if (PackAssetBundleUtlis.IsFilterAssetDep(dep, path))
                    {
                        continue;
                    }

                    bundleName = PackAssetBundleUtlis.GetBundleName(dep);
                    _packTools.PushAssetBuild(dep, bundleName, false);
                }
            }
        }

        // 添加Asset自身;
        bundleName = PackAssetBundleUtlis.GetBundleName(path);        
        _packTools.PushAssetBuild(path, bundleName);

    }

    private static void Build()
    {
        //_packTools.DisposeFbx();
        _packTools.BuildAssetBundle(bundleBuildFolder, BuildAssetBundleOptions.DeterministicAssetBundle);
        EditorUtility.UnloadUnusedAssetsImmediate();

        PackAssetBundleUtlis.DisposeFiles(bundleBuildFolder);
    }

    // 编译原包
    public static void BuildSrcApp()
    {
        // 拷贝Data包
        PackAssetBundleUtlis.CopyFolder(string.Format("{0}/{0}", ResourceConst.PkgBundleFolder), "Assets/StreamingAssets");

        AssetDatabase.Refresh();

        string cur_dir = System.Environment.CurrentDirectory;
        string build_dir = "";
        string app_name = "";
        BuildTarget build_target = BuildTarget.StandaloneWindows64;
        BuildTargetGroup target_group = BuildTargetGroup.Standalone;

        string root_dir = "";
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith("buildDir"))
            {
                root_dir = arg.Split('@')[1];
                break;
            }
#if UNITY_ANDROID
            root_dir = string.Format("{0}/build/android", System.IO.Directory.GetParent(cur_dir));
#elif UNITY_IPHONE
            root_dir = string.Format("{0}/build/ios", System.IO.Directory.GetParent(cur_dir));
#elif UNITY_STANDALONE_WIN
            root_dir = string.Format("{0}/build/pc", System.IO.Directory.GetParent(cur_dir));
#endif
        }


#if UNITY_ANDROID
        build_dir = string.Format("{0}/{1}_{2}", root_dir, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HHmmss"));
        app_name = string.Format("{0}/{1}_{2}.apk", build_dir, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HHmmss"));
        build_target = BuildTarget.Android;
#elif UNITY_IPHONE
	    build_dir = string.Format("{0}/{1}_{2}", root_dir, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HHmmss"));
        app_name = string.Format("{0}/{1}_{2}.ipa", build_dir, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HHmmss"));
        build_target = BuildTarget.iOS;
#elif UNITY_STANDALONE_WIN
        build_dir = string.Format("{0}/{1}_{2}", root_dir, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HHmmss"));
        app_name = string.Format("{0}/{1}_{2}.exe", build_dir, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HHmmss"));
        build_target = BuildTarget.StandaloneWindows64;
#endif
        System.IO.Directory.CreateDirectory(build_dir);
        List<string> editor_scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
            editor_scenes.Add(scene.path);
        }

        string[] scene_array = editor_scenes.ToArray();
        EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
        //string res = BuildPipeline.BuildPlayer(scene_array, app_name, build_target, BuildOptions.None);
    }

     public static void BuildOneKey()
    {
        // 更新打包环境
        //GitUtility.GitPull();

        PackAllAssetBundle(true, false);

        // Git提交
        GitUtility.GitCommit();

        BuildSrcApp();
    }

    static bool CheckShader()
    {
        string[] shaders = _packTools.GetAssetsByBundle(ResourceConst.bundleShader);
        List<string> shader_infos = ShaderTools.CheckShaderLods(shaders);
        
        if ( shader_infos == null || shader_infos.Count < 1 )
        {
            return true;
        }

        string info = string.Format("{0}个Shader LOD检测非法，打包停止，详情请看Log", shader_infos.Count);

        foreach(string shader_info in shader_infos)
        {
            Debug.LogError(shader_info);
            if (shader_infos.Count  <= 5)
            {
                info = string.Format("{0}\n{1}", info, shader_info);
            }
        }

        EditorUtility.DisplayDialog("Shader检测失败", info, "停止打包");

        return false;
    }
}