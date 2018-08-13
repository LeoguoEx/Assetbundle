using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum LoadBundleType
{
	LoadFromMemory = 0,
	LoadFromFile = 1,
}

public class Example3 : MonoBehaviour
{
	public LoadBundleType LoadBundleType;

	private AssetBundleManifest m_manifest;
	private List<AssetBundle> m_bundleList = new List<AssetBundle>();
	
	// Use this for initialization
	void Start ()
	{
		m_manifest = LoadManifest();
		switch (LoadBundleType)
		{
			case LoadBundleType.LoadFromMemory:
				LoadFromMemory();
				break;
			case LoadBundleType.LoadFromFile:
				LoadFromFile();
				break;
		}
	}

	private AssetBundleManifest LoadManifest()
	{
		return null;
	}

	#region LoadFromMemory
	private void LoadFromMemory()
	{
		StartCoroutine(LoadFromMemoryEnumerator());
	}
	
	IEnumerator LoadFromMemoryEnumerator()
	{
		string path = string.Format("file://{0}/Example/Example2/LZ4Bundle/shader", Application.dataPath);
		var uwr = UnityWebRequest.Get(path);
		yield return uwr.SendWebRequest();
		//byte[] decryptedBytes = MyDecription(uwr.downloadHandler.data);
		AssetBundle bundle = AssetBundle.LoadFromMemory(uwr.downloadHandler.data);
		if (bundle != null)
		{
			//bundle.LoadAllAssets();
			m_bundleList.Add(bundle);
		}

		
		yield return new WaitForEndOfFrame();
		
		path = string.Format("file://{0}/Example/Example2/LZ4Bundle/cube", Application.dataPath);
		uwr = UnityWebRequest.Get(path);
		yield return uwr.SendWebRequest();
		//byte[] decryptedBytes = MyDecription(uwr.downloadHandler.data);
		AssetBundle bundle1 = AssetBundle.LoadFromMemory(uwr.downloadHandler.data);
		m_bundleList.Add(bundle1);
		
		yield return 1;
		
		path = string.Format("file://{0}/Example/Example2/LZ4Bundle/materials", Application.dataPath);
		uwr = UnityWebRequest.Get(path);
		yield return uwr.SendWebRequest();
		//byte[] decryptedBytes = MyDecription(uwr.downloadHandler.data);
		bundle = AssetBundle.LoadFromMemory(uwr.downloadHandler.data);
		if (bundle != null)
		{
			//bundle.LoadAllAssets();
			m_bundleList.Add(bundle);
		}

		yield return 1;
		
		if (bundle1 != null)
		{
			GameObject prefab = bundle1.LoadAsset<GameObject>("Cube1");
			prefab = Instantiate(prefab);
			prefab.name = "Cube";
		}
		
	}

	#endregion

	#region AssetBundle.LoadFromFile

	private void LoadFromFile()
	{
		string path = string.Format("{0}/Example/Example2/LZ4Bundle/shader", Application.dataPath);
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		
		path = string.Format("{0}/Example/Example2/LZ4Bundle/materials", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);
		
		
		path = string.Format("{0}/Example/Example2/LZ4Bundle/cube", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);
		if (bundle != null)
		{
			GameObject prefab = bundle.LoadAsset<GameObject>("Cube1");
			prefab = Instantiate(prefab);
			prefab.name = "Cube";
		}
	}

	#endregion
	
	#region AssetBundle.LoadFromFile

	private void UnityWebRequestDownload()
	{
		UnityWebRequestAssetBundle.GetAssetBundle("");
		
		string path = string.Format("{0}/Example/Example2/LZ4Bundle/shader", Application.dataPath);
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		
		path = string.Format("{0}/Example/Example2/LZ4Bundle/materials", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);
		
		
		path = string.Format("{0}/Example/Example2/LZ4Bundle/cube", Application.dataPath);
		bundle = AssetBundle.LoadFromFile(path);
		if (bundle != null)
		{
			GameObject prefab = bundle.LoadAsset<GameObject>("Cube1");
			prefab = Instantiate(prefab);
			prefab.name = "Cube";
		}
	}

	#endregion
	
}
