
/**************************************************************************************************
	Copyright (C) 2017 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：ResourceConst.cs;
	作	者：W_X;
	时	间：2017 - 07 - 05;
	注	释：;
**************************************************************************************************/

using UnityEngine;

/// <summary>
/// 加载器类型;
/// </summary>
public enum ELoaderType
{
    LoaderType_Resources = 0,       // 使用Resources进行加载;
    LoaderType_AssetBundle = 1, // 使用AssetBundle进行资源加载;
}


public class ResourceConst
{
    public const string PkgBundleBuildFolder = "_Build";
#if UNITY_ANDROID
    public const string PkgBundleFolder = "DataAndroid";
#elif UNITY_IPHONE
	public const string PkgBundleFolder = "DataIOS";
#elif UNITY_STANDALONE_WIN
	public const string PkgBundleFolder = "DataPC";
#elif UNITY_WP8
	public const string PkgBundleFolder = "DataWP";
#else
	public const string PkgBundleFolder = "DataDefault";
#endif    

#if UNITY_EDITOR && !USE_BUNDLE
    public static ELoaderType LoaderType = ELoaderType.LoaderType_Resources;
#else
    public static ELoaderType LoaderType = ELoaderType.LoaderType_AssetBundle;
#endif

    public const string ConfFolder = "data_dat";
    public const string SceneRoot_Low = "_res";
    public const string SceneRoot_Medium = "_medium";
    public const string SceneRoot_High = "_high";
    public const string SceneInfo_Root = "SceneNodeRefData";

    public const string BundleExtensions = ".bundle";
    public const string ScenePath = "Assets/Scenes/level_process/";

    public const string FileListName = "filelist";
    public const string MD5Name = "md5_info";

    //*******************************************************************
    public const string AssetResourceName = "Assets/Game_Prefab/";

	public const string AllSceneConf = "allsceneconf";
	public const string AllShader = "allshader";

	public static string bundleSceneConf
    {
        get
        {
            return string.Format("{0}{1}{2}", ResourceSceneConfPath, AllSceneConf, BundleExtensions);
        }
    }

    public static string bundleShader
    {
        get
        {
            return string.Format("shader/{0}{1}", AllShader, BundleExtensions);
        }
    }

    public static string BundleFolder
    {
        get
        {
#if UNITY_EDITOR
            return string.Format("{0}/{0}", PkgBundleFolder, PkgBundleFolder);
#else
            return PkgBundleFolder;
#endif
        }
    }
    
#region --------------------------------------------------------resource资源-------------------------------------------------
    //请注意：以下必须以Resource开头,因为打bundle 的时候需要反射这些变量
    public const string ResourceScenePath = "scenes/";// 剧情路径
    public const string ResourceSceneConfPath = "sceneconf/";// 剧情路径
    public const string ResourceAnimation = "Anim/"; // 动画

    public const string ResourceAudio = "prefab/audio/";// 声音
    public const string ResourceBlock = "prefab/block/";// 阻挡
    public const string ResourceCinemaPath = "prefab/cinema/";// 剧情路径
    public const string ResourceType_Fx = "prefab/fx/";// 特效
    public const string ResourceType_Panel = "prefab/ui/panel/";// 界面;
    public const string ResourceType_Texture = "prefab/ui/texture/";// 界面;
    public const string ResourceType_Atlas = "prefab/ui/atlas/";// 界面;

    public const string ResourceCharacterBone = "prefab/character/bone/";// 人物
    public const string ResourceCharacterMat = "prefab/character/material/";// 人物
    public const string ResourceCharacterMesh = "prefab/character/mesh/";// 人物
    public const string ResourceCharacterAnim = "prefab/character/animation/";// 人物
    public const string ResourceSimpleCharacter = "prefab/character/prefab/";// 简单实体资源;
    
    public const string ResourceItemMesh = "prefab/item/mesh/"; // 物品
    public const string ResourceItemMat = "prefab/item/material/"; // 物品
	public const string ResourceShader = "shader/"; // 物品

	//public static string[] PackResourceGroups = new string[]
	//{
	//    ResourceAudio,
	//    ResourceBlock,
	//    ResourceCinemaPath,
	//    ResourceType_Fx,
	//    ResourceCharacterAnim,
	//    ResourceCharacterBone,
	//    ResourceCharacterMat,
	//    ResourceCharacterMesh,
	//    ResourceSimpleCharacter,
	//    ResourceItemMesh,
	//    ResourceItemMat,
	//    ResourceType_Panel,
	//    ResourceType_Texture
	//};

#if UNITY_EDITOR

	public static PackResourceInfo[] PackResourceInfoGroups = new PackResourceInfo[]
    {
        new PackResourceInfo(ResourceAudio, "", false),
        new PackResourceInfo(ResourceBlock, "*.bytes", false),
        new PackResourceInfo(ResourceCinemaPath, "*.prefab", true),
        new PackResourceInfo(ResourceType_Fx, "*.prefab", true),
        new PackResourceInfo(ResourceCharacterAnim, "*.anim", false),
        new PackResourceInfo(ResourceCharacterBone, "*.prefab", true),
        new PackResourceInfo(ResourceCharacterMat, "*.mat", true),
        new PackResourceInfo(ResourceCharacterMesh, "*.prefab", true),
        new PackResourceInfo(ResourceSimpleCharacter, "*.prefab", true),
        new PackResourceInfo(ResourceItemMesh, "*.prefab", true),
        new PackResourceInfo(ResourceItemMat, "*.mat", true),
        new PackResourceInfo(ResourceType_Panel, "*.prefab", true),
        new PackResourceInfo(ResourceType_Texture, "", false),
        new PackResourceInfo(ResourceType_Atlas, "*.prefab", true),

    };

    public class PackResourceInfo
    {
        public string path;
        public string suffix;
        public bool check_dep;

        public PackResourceInfo(string path, string suffix, bool check_dep)
        {
            this.path = path;
            this.suffix = suffix;
            this.check_dep = check_dep;
        }
    }

#endif
    #endregion

    public static string GetResourcePath(string resourceGroups)
    {
        return AssetResourceName + resourceGroups;
    }
}