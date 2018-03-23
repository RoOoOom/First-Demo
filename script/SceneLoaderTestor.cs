using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderTestor : MonoBehaviour {
    bool loaded = false;
    public static int hashcode = 0;
    private List<AssetBundle> m_sceneList = new List<AssetBundle>();
    public MoveController player;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this.gameObject);
        //StartCoroutine(LoadOtherScene("other"));
        //StartCoroutine(LoadOtherScene(0));
        SceneManager.LoadSceneAsync("other");
        loaded = true;
        
    }

    void Update()
    {
        if (loaded)
            player.UpdateInput();
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
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                
                //bundle.Unload(true);
                loaded = true;
            }
        }
    }

    IEnumerator LoadOtherScene(int index)
    {
        Caching.CleanCache();
        string path = "file://" + Application.dataPath + "/StreamAsset/other.unity3d";
        WWW www = new WWW(path);
        yield return www;

        if (www.error == null)
        {
            AssetBundle ab = www.assetBundle;
            AsyncOperation asy = SceneManager.LoadSceneAsync("other" , LoadSceneMode.Additive);
            yield return asy;
            loaded = true;
        }
        else
        {
            Debug.LogError(www.error);
        }
    }
}
