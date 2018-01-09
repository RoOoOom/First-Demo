using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Config;

public class ChooseMap : MonoBehaviour
{
    public int map;
    public MapResourceInfoTbl tpl;
    public int gridSize = 100;
    public bool isShowGrid = false;

    private MeshRenderer _renderer;
    public static int curMapId;
    public static MapResourceInfoTbl CurTpl;
    public static int width = 0;
    public static int height = 0;


    public void OnEnable()
    {
        CurTpl = tpl;
    }

    public void Init()
    {
        GameObject map = GameObject.Find("Map");
        _renderer = map.GetComponent<MeshRenderer>();
        CurTpl = tpl;
    }

    public void LoadMap()
    {
        Texture texture = Resources.Load("pic/" + map.ToString()) as Texture;
        curMapId = map; 
        if (null != texture)
        {
            _renderer.sharedMaterial.mainTexture = texture;
        }
        else
        {
            Debug.LogError(" =======>> pic文件夹下没有该地图：" + map);
        }

        if (null != tpl)
        {
            CurTpl = tpl;
            MapResourceInfo info = tpl.data.Find(it => it.maskId == map);
            if (null != info)
            {
                _renderer.gameObject.transform.localScale = new Vector3(info.width, info.height, 1f);
                _renderer.gameObject.transform.localPosition = new Vector3(info.width / 2, -info.height / 2, 0f);
                width = info.width;
                height = info.height;
            }
            else
            {
                Debug.LogError(" =======>> 没有该地图配置信息：" + map);
            }
        }
        else
        {
            Debug.LogError(" =========>> 缺少地图配置文件！");
        }
    }

    void OnDrawGizmos()
    {
        if (isShowGrid)
        {
            GeneGrid(width, height);
        }
      
    }

    private void GeneGrid(int width, int height)
    {
        Gizmos.color = Color.white;
        int wCount = Mathf.FloorToInt((float)width / gridSize);
        for (int i = 0; i < wCount; i++)
        {
            Gizmos.DrawLine(new Vector3(i * gridSize, 0, 0), new Vector3(i * gridSize, -height, 0));
        }

        int hCount = Mathf.FloorToInt((float)height / gridSize);
        for (int i = 0; i < hCount; i++)
        {
            Gizmos.DrawLine(new Vector3(0, -i * gridSize, 0), new Vector3(width, -i * gridSize, 0));
        }
    }
}
