using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderTestor : MonoBehaviour {
    public static int hashcode = 0;
    private List<AssetBundle> m_sceneList = new List<AssetBundle>();
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(LoadOtherScene("other"));
        //StartCoroutine(LoadOtherScene("another"));

        Debug.Log(m_sceneList.Count);
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroyed");
    }

    IEnumerator LoadOtherScene(string sceneName)
    {
        //WWW download = WWW.LoadFromCacheOrDownload("file://" + Application.dataPath + "/StreamAsset/" + sceneName + ".unity3d", hashcode++);
        using (WWW download = new WWW("file://" + Application.dataPath + "/StreamAsset/" + sceneName + ".unity3d"))
        {
            yield return download;
            if (!string.IsNullOrEmpty(download.error))
            {
                Debug.Log(download.error);
            }
            else
            {
                var bundle = download.assetBundle;
                SceneManager.LoadScene(sceneName);
                bundle.Unload(true);
            }
        }
    }
}
