using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

public class Example1 
{
	[MenuItem("Assetbundles/Example1/Build Assetbudles")]
	static void BuildAssetbundles()
	{
		string path = string.Format("{0}/{1}", Application.dataPath, "Example/Example1/Assetbundle");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
		AssetDatabase.Refresh();
	}
}
