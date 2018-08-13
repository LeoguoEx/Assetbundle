using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public enum EUnloadTest
{
	UnloadFalseAfterInit = 0,
	UnloadFalseBeforeInit = 1,
	UnloadTrueAfterInit = 2,
	UnloadFalseWithoutCleanResources = 3,
	UnloadFalseCleanResources = 4,
	TestAutoBuild = 5,
	UnloadFalseChangeScene = 6,
}

public class Example4 : MonoBehaviour
{
	public EUnloadTest m_unloadTest;
	// Use this for initialization
	void Start () 
	{
		switch (m_unloadTest)
		{
				case EUnloadTest.UnloadFalseAfterInit:
					LoadBundleFromFile();
					break;
				case EUnloadTest.UnloadFalseBeforeInit:
					LoadBundleFromFile1();
					break;
				case EUnloadTest.UnloadTrueAfterInit:
					LoadBundleFromFile2();
					break;
				case EUnloadTest.UnloadFalseWithoutCleanResources:
					StartCoroutine(LoadBundleFromFile3());
					break;
				case EUnloadTest.UnloadFalseCleanResources:
					LoadBundleFromFile4();
					break;
				case EUnloadTest.TestAutoBuild:
					TestAutoBuild();
					break;
				case EUnloadTest.UnloadFalseChangeScene:
					LoadBundleFromFile5();
					break;
		}
	}

	private void LoadBundleFromFile()
	{
		string path = string.Format("{0}/Example/Example2/LZ4Bundle/shader", Application.dataPath);
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		
		path = string.Format("{0}/Example/Example2/LZ4Bundle/materials", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);

		AssetBundle bundle1 = bundle;
		path = string.Format("{0}/Example/Example2/LZ4Bundle/cube", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);
		if (bundle != null)
		{
			GameObject prefab = bundle.LoadAsset<GameObject>("Cube1");
			prefab = Instantiate(prefab);
			prefab.name = "Cube";
		}
		
		bundle1.Unload(false);
	}
	
	private void LoadBundleFromFile1()
	{
		string path = string.Format("{0}/Example/Example2/LZ4Bundle/shader", Application.dataPath);
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		
		path = string.Format("{0}/Example/Example2/LZ4Bundle/materials", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);

		AssetBundle bundle1 = bundle;
		bundle1.Unload(false);
		
		path = string.Format("{0}/Example/Example2/LZ4Bundle/cube", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);
		if (bundle != null)
		{
			GameObject prefab = bundle.LoadAsset<GameObject>("Cube1");
			prefab = Instantiate(prefab);
			prefab.name = "Cube";
		}
	}
	
	private void LoadBundleFromFile2()
	{
		string path = string.Format("{0}/Example/Example2/LZ4Bundle/shader", Application.dataPath);
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		
		path = string.Format("{0}/Example/Example2/LZ4Bundle/materials", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);

		AssetBundle bundle1 = bundle;
		path = string.Format("{0}/Example/Example2/LZ4Bundle/cube", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);
		if (bundle != null)
		{
			GameObject prefab = bundle.LoadAsset<GameObject>("Cube1");
			prefab = Instantiate(prefab);
			prefab.name = "Cube";
		}
		
		bundle1.Unload(true);
	}

	private List<Object[]> m_allassets;
	private IEnumerator LoadBundleFromFile3()
	{
		m_allassets = new List<Object[]>();
		List<GameObject> list = new List<GameObject>();
		List<AssetBundle> bundlelist = new List<AssetBundle>();
		for (int i = 0; i < 10; i++)
		{
			string path = string.Format("{0}/Example/Example2/LZ4Bundle/shader", Application.dataPath);
			AssetBundle bundle = AssetBundle.LoadFromFile(path);
			m_allassets.Add(bundle.LoadAllAssets());
			bundlelist.Add(bundle);

			yield return 1;
			
			path = string.Format("{0}/Example/Example2/LZ4Bundle/materials", Application.dataPath);
			bundle = AssetBundle.LoadFromFile(path);
			m_allassets.Add(bundle.LoadAllAssets());
			bundlelist.Add(bundle);
			yield return 1;

			path = string.Format("{0}/Example/Example2/LZ4Bundle/cube", Application.dataPath);
			bundle = AssetBundle.LoadFromFile(path);
			m_allassets.Add(bundle.LoadAllAssets());
			if (bundle != null)
			{
				GameObject temp = bundle.LoadAsset<GameObject>("Cube1");
				GameObject prefab = Instantiate(temp);
				prefab.name = "Cube";
				list.Add(prefab);
			}
			bundlelist.Add(bundle);
			yield return 1;

			for (int j = bundlelist.Count - 1; j >= 0; j--)
			{
				yield return 1;
				if (bundlelist[j] != null)
				{
					bundlelist[j].Unload(false);
				}
				bundlelist[j] = null;
			}
			bundlelist.Clear();
		}

		for (int i = 0; i < list.Count; i++)
		{
			yield return 1;
			GameObject.Destroy(list[i]);
			list[i] = null;
		}
		list.Clear();

		for (int i = 0; i < m_allassets.Count; i++)
		{
			for (int j = 0; j < m_allassets[i].Length; j++)
			{
				Object asset = m_allassets[i][j];
				if (CanUnload(asset))
				{
					Resources.UnloadAsset(m_allassets[i][j]);
				}
			}
			m_allassets[i] = null;
		}
		m_allassets.Clear();

		yield return 1;
	}
	
	private void LoadBundleFromFile4()
	{
		StartCoroutine(LoadBundleFromFile3());
		
		AssetBundle.UnloadAllAssetBundles(true);
		Resources.UnloadUnusedAssets();
	}
	
	//loadscene
	private void LoadBundleFromFile5()
	{
		StartCoroutine(LoadBundleFromFile3());
		
		//十分必要
		//SceneManager.LoadScene("Example4-1");
		AssetBundle.UnloadAllAssetBundles(true);
		Resources.UnloadUnusedAssets();
	}
	

	private bool CanUnload(Object obj)
	{
		if (obj is GameObject || obj is Component || obj is AssetBundle)
		{
			return false;
		}

		return true;
	}

	private void TestAutoBuild()
	{
		string path = string.Format("{0}/Example/Example2/LZ4Bundle/materials", Application.dataPath);
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		Object[] assets = bundle.LoadAllAssets();
		for (int i = 0; i < assets.Length; i++)
		{
			Debug.LogError("LoadAllAssets  : " + assets[i].name);
		}

		assets = bundle.LoadAssetWithSubAssets("New Material");
		for (int i = 0; i < assets.Length; i++)
		{
			Debug.LogError("LoadAllAssetsSubAssets  : " + assets[i].name);
		}
	}
}
