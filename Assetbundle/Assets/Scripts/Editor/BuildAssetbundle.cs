using System.IO;
using UnityEngine;
using UnityEditor;

public class BuildAssetbundle 
{
	[MenuItem("Assets/Build Assetbudles")]
	static void BuildAssetbundles()
	{
		string path = string.Format("{0}/{1}", Application.dataPath, "Assetbundle");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
		AssetDatabase.Refresh();
		
	}

	[MenuItem("Assets/LZMABundle")]
	static void BuildLZMABundle()
	{
		string path = string.Format("{0}/{1}", Application.dataPath, "LZMABundle");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.iOS);
		AssetDatabase.Refresh();
	}

	[MenuItem("Assets/LZ4Bundle")]
	static void BuildLZ4Bundle()
	{
		string path = string.Format("{0}/{1}", Application.dataPath, "LZ4Bundle");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
		AssetDatabase.Refresh();
	}

	[MenuItem("Assets/NoneCompressBundle")]
	static void BuildNoneBundle()
	{
		string path = string.Format("{0}/{1}", Application.dataPath, "NoneCompressedBundle");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
		AssetDatabase.Refresh();
	}
}
