using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class Example2 
{

	[MenuItem("Assetbundles/Example2/LZMABundle")]
	static void BuildLZMABundle()
	{
		string path = GetPath("LZMABundle");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.iOS);
		AssetDatabase.Refresh();
	}

	[MenuItem("Assetbundles/Example2/LZ4Bundle")]
	static void BuildLZ4Bundle()
	{
		string path = GetPath("LZ4Bundle");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
		AssetBundleBuild[] bundles = new AssetBundleBuild[100];
		BuildPipeline.BuildAssetBundles(path, bundles, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
		AssetDatabase.Refresh();
	}

	[MenuItem("Assetbundles/Example2/NoneCompressBundle")]
	static void BuildNoneBundle()
	{
		string path = GetPath("NoneCompressedBundle");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
		AssetDatabase.Refresh();
	}

	static string GetPath(string name)
	{          
		string path = string.Format("{0}/Example/Example2/{1}", Application.dataPath, name);
		return path;
	}
}
