
/**************************************************************************************************
	Copyright (C) 2018 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：BundleMenuItem.cs;
	作	者：W_X;
	时	间：2018 - 01 - 05;
	注	释：;
**************************************************************************************************/

using UnityEngine;
using UnityEditor;

public class BundleMenuItem
{
    [MenuItem("Bundle/发布工具/[1]更新+准备（自动序列化+自动生成MD5）", priority = -930)]
    static void PullAndPrepare()
    {
        GitUtility.GitPull();
        //EditorUtility.DisplayDialog("更新提示", "更新完毕", "继续");
        //GameConfigEditor.SerializerConfig();
        FileListUtility.BuildFileList(true);
    }

    [MenuItem("Bundle/发布工具/  [1.1] 仅更新（不含打包准备）", priority = -929)]
    static void Pull()
    {
        GitUtility.GitPull();
    }

    [MenuItem("Bundle/发布工具/  [1.2] 仅准备（自动序列化+自动生成MD5）", priority = -928)]
    static void Prepare()
    {
        //GameConfigEditor.SerializerConfig();
        FileListUtility.BuildFileList(true);
    }

    [MenuItem("Bundle/发布工具/[2] 一键打包（打包conf、打包develop、生成apk（含拷贝Data））", priority = -926)]
    static void BuildOneKey()
    {
        PackAssetBundle.BuildOneKey();
    }

    [MenuItem("Bundle/发布工具/  [2.1] 打Data资源包全（conf、develop）", priority = -925)]
    static void PackConfAndDevelop()
    {
        PackAssetBundle.OnlyCopyConf2StreamingAssets();
        PackAssetBundle.PackAllAssetBundle(true, false);
        // Git提交
        GitUtility.GitCommit();
    }

    [MenuItem("Bundle/发布工具/  [2.2] 打Data资源包（conf）", priority = -924)]
    static void BuildPackConf()
    {
        PackAssetBundle.OnlyCopyConf2StreamingAssets();
    }

    [MenuItem("Bundle/发布工具/  [2.3] 仅生成apk（含拷贝Data）", priority = -923)]
    static void BuildApk()
    {
        PackAssetBundle.BuildSrcApp();
    }

    [MenuItem("Bundle/发布工具/[3]版本管理", priority = -916)]
    static void VersionMng()
    {

    }

    [MenuItem("Bundle/发布工具/  [3.1] 对比filelist", priority = -915)]
    static void CompareFileListXml()
    {
        FileListUtility.CompareFileListXml();
    }

    [MenuItem("Bundle/发布工具/[4]其它", priority = -910)]
    static void ReleaseOther()
    {
      
    }

    [MenuItem("Bundle/发布工具/  [4.1] 生成所用资源的MD5;", priority = -909)]
    static void BuildArtFileList()
    {
        FileListUtility.BuildArtFileList();
    }

    [MenuItem("Bundle/发布工具/  [4.2] 一键打包AssetBundle（自动检测Bundle）", priority = -810)]
    public static void PackAssetBundleAndCheck()
    {
        PackAssetBundle.PackAllAssetBundle(true);
    }

    [MenuItem("Bundle/发布工具/  [4.3]将AssetBundle拷贝入StreamingAssets", priority = -809)]
    static void CopyAssetBundle2StreamingAssets()
    {
        PackAssetBundle.CopyAssetBundle2StreamingAssets();
    }

#if !USE_BUNDLE
    [MenuItem("Bundle/其它/[0]Editor下强行切换为Bundle资源模式", priority = -710)]
    static void Switch2Bundle()
    {
        EditorCommon.AddDefine("USE_BUNDLE");
    }
#else
    [MenuItem("Bundle/[3]Editor下恢复为Resource资源模式", priority = -710)]
    static void Switch2Resource()
    {
        EditorCommon.RemoveDefine("USE_BUNDLE");        
    }
#endif

#if GAMECONFIG_WITHOUT_RAW_XML
    [MenuItem("Bundle/[4]Editor下配置读取切换为为Xml源文件", priority = -709)]
    static void Switch2Xml()
    {
        EditorCommon.RemoveDefine("GAMECONFIG_WITHOUT_RAW_XML");
    }
#else
    [MenuItem("Bundle/其它/[1]Editor下配置读取切换为为二进制", priority = -709)]
    static void Switch2Binary()
    {
        EditorCommon.AddDefine("GAMECONFIG_WITHOUT_RAW_XML");
    }
#endif


    /*********************************************************华丽丽的分割线（上面为发布工具，下面为开发工具）*****************************************************************************************************************/
    [MenuItem("Bundle/开发工具/一键打包AssetBundle", priority = 300)]
    public static void PackAssetBundleNoCheck()
    {
        PackAssetBundle.PackAllAssetBundle(false);
    }

    [MenuItem("Bundle/开发工具/检测所有的Bundle包;", priority = 301)]
    public static void CheckAllBundles()
    {
        PackAssetBundleUtlis.CheckAllBundles();
    }

    [MenuItem("Bundle/开发工具/生成FileList", priority = 302)]
    static void BuildFileList()
    {
        FileListUtility.BuildFileList(true);
    }

    [MenuItem("Bundle/开发工具/比较FileList[txt]", priority = 303)]
    static void CompareFileListTxt()
    {
        FileListUtility.CompareFileListTxt();
    }

    [MenuItem("Bundle/开发工具/Test检查所有引用", priority = 304)]
    static void TestDep()
    {
        PackAssetBundle.TestDep();
    }
}