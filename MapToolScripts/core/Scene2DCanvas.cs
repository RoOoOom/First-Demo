using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.NavMesh;

public class Scene2DCanvas : MonoBehaviour {
    public string name = "1_xinshoucun";
    public Transform nav_mesh = null;
    public int size = 128;
    public int size_x = 128;
    public int size_z = 128;
    //
    public int tiles_x = 1;
    public int tiles_z = 1;
    //
    public float angle_degree = 30.0f;
    public string mapId;
    private List<Triangle> lst;

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public static Texture2D LoadPNG(string filePath)
    {
        return Resources.Load<Texture2D>(filePath); ;
    }
    // 00001- 
    string FormatIndex(int a)
    {
        string ret = string.Format("{0}", a);
        if (a >= 10000)
        {
            return ret;
        }
        if (a >= 1000)
        {
            return "0" + ret;
        }
        if (a >= 100)
        {
            return "00" + ret;
        }
        if (a >= 10)
        {
            return "000" + ret;
        }
        return "0000" + ret;
    }
    void OnEnable()
    {
        tiles_x = size_x / size;
        tiles_z = size_z / size;
        float scale = 25.6f;
        //transform.localScale = new Vector3(scale, 1.0f, scale);
        
        // 先销毁
        //foreach (Transform c in transform)
        //{
        //    GameObject.Destroy(c.gameObject);
        //}
        // 构建tile mesh
        GameObject prefab = Resources.Load("Tile") as GameObject;
        float angle_radian = Mathf.PI * angle_degree / 180.0f;
        float s = 1.0f / Mathf.Sin(angle_radian);
        // 边缘
        Camera.main.transform.eulerAngles = new Vector3(angle_degree, 0.0f, 0.0f);
        CameraController cc = Camera.main.gameObject.GetComponent<CameraController>();
        cc.alpha_ = Mathf.PI * 0.5f - angle_radian;
        cc.distance_ = Camera.main.orthographicSize / Mathf.Tan(angle_radian);
        cc.map_right_bottom_.x = tiles_x * scale;
        cc.map_right_bottom_.z = -tiles_z * scale * s;
        cc.camera_inner_left_top_.x = Camera.main.orthographicSize * Camera.main.aspect;
        cc.camera_inner_left_top_.z = -Camera.main.orthographicSize * (2.0f * s - Mathf.Sin(angle_radian));
        cc.camera_inner_right_bottom_.x = cc.map_right_bottom_.x - cc.camera_inner_left_top_.x;
        cc.camera_inner_right_bottom_.z = cc.map_right_bottom_.z + Camera.main.orthographicSize * Mathf.Sin(angle_radian);


        //transform.localScale = new Vector3((float)size_x / 100, 1.0f, (float)size_z / 100);
        //transform.Translate((float)size_x / 100 * 0.5f, 0.0f, -(float)size_z / 100 * 0.5f * s);


        List<NavTriangle> navList = new List<NavTriangle>(); 
        string path = Application.streamingAssetsPath + "/NavMeshMap/" + mapId + ".navmesh";
        NavResCode code = NavUtil.LoadNavMeshFromResource(path, out lst);
        if (null != lst && lst.Count > 0)
        {
            for (int i = 0; i < lst.Count; i++)
            { 
                navList.Add(lst[i].CloneNavTriangle());
            }
        }
        Seeker.instance.NavMeshData = navList; 
    }

    int count = 0;

    private void DrawNavMesh()
    { 
        if (lst == null)
        {
            return;
        }
        if (lst.Count != 0)
        {
            foreach (Triangle tri in lst)
            {
                Gizmos.color = Color.black;

                Vector3 p1 = new Vector3(tri.GetPoint(0).x, 0, tri.GetPoint(0).y);
                Vector3 p2 = new Vector3(tri.GetPoint(1).x, 0, tri.GetPoint(1).y);
                Vector3 p3 = new Vector3(tri.GetPoint(2).x, 0, tri.GetPoint(2).y);

                if (count < 1)
                {
                    GameObject obj = new GameObject();
                    obj.transform.position = p1;
                    obj.name = "1";
                    GameObject obj1 = new GameObject();
                    obj1.transform.position = p2;
                    obj.name = "2";
                    GameObject obj2 = new GameObject();
                    obj2.transform.position = p3;
                    obj.name = "3";
                    count++;
                }

                //Vector3 p1 = new Vector3(tri.GetPoint(0).x, tri.GetPoint(0).y, 0);
                //Vector3 p2 = new Vector3(tri.GetPoint(1).x, tri.GetPoint(1).y, 0);
                //Vector3 p3 = new Vector3(tri.GetPoint(2).x, tri.GetPoint(2).y, 0);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p1);
            }
        }
    }


    void OnDrawGizmos()
    { 
        DrawNavMesh(); 
    }
}
