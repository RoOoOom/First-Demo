using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class CombineMeshTestor : MonoBehaviour {
    private Dictionary<Material, List<CombineInstance>> m_mtrlMeshfilter = new Dictionary<Material, List<CombineInstance>>();
	// Use this for initialization
	void Start () {
        //CombineMeshDysn();
        //this.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CreateMeshOnly()
    {
        m_mtrlMeshfilter.Clear();

        MeshFilter[] mfArray = GetComponentsInChildren<MeshFilter>();
        MeshRenderer[] mrArray = GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < mfArray.Length; i++)
        {
            CombineInstance combIns = new CombineInstance();
            combIns.mesh = mfArray[i].sharedMesh;
            combIns.transform = mfArray[i].transform.localToWorldMatrix;

            if (!m_mtrlMeshfilter.ContainsKey(mrArray[i].sharedMaterial))
            {
                List<CombineInstance> mf = new List<CombineInstance>();
                mf.Add(combIns);
                m_mtrlMeshfilter.Add(mrArray[i].sharedMaterial, mf);
            }
            else
            {
                List<CombineInstance> mf;
                m_mtrlMeshfilter.TryGetValue(mrArray[i].sharedMaterial, out mf);
                mf.Add(combIns);
            }
        }

        int index = 0;
        foreach ( Material mtrl in m_mtrlMeshfilter.Keys )  
        {
            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(m_mtrlMeshfilter[mtrl].ToArray());
            CreateMeshObj(newMesh, index);
        }
    }
    
    public void CombineMeshDysn()
    {
        m_mtrlMeshfilter.Clear();

        MeshFilter[] mfArray = GetComponentsInChildren<MeshFilter>();
        MeshRenderer[] mrArray = GetComponentsInChildren<MeshRenderer>();
       
        Debug.Log(mfArray.Length);
        //CombineInstance[] ciArray = new CombineInstance[mfArray.Length];
        

        for (int i = 0; i<mfArray.Length ; i++)
        {
            CombineInstance combIns = new CombineInstance();
            combIns.mesh = mfArray[i].sharedMesh;
            combIns.transform = mfArray[i].transform.localToWorldMatrix;
            if (!m_mtrlMeshfilter.ContainsKey(mrArray[i].sharedMaterial))
            {
                List<CombineInstance> mf = new List<CombineInstance>();
                mf.Add(combIns);
                m_mtrlMeshfilter.Add(mrArray[i].sharedMaterial, mf);
            }
            else
            {
                List<CombineInstance> mf;
                m_mtrlMeshfilter.TryGetValue(mrArray[i].sharedMaterial, out mf);
                mf.Add(combIns);
            }
            mfArray[i].gameObject.SetActive(false);
        }
        int index = 0;
        foreach ( Material mtrl in m_mtrlMeshfilter.Keys )
        {
            GameObject lgo = new GameObject("Level");
            MeshFilter mf = lgo.AddComponent<MeshFilter>();
            MeshRenderer mr = lgo.AddComponent<MeshRenderer>();

            mr.sharedMaterial = mtrl;
            
            mf.sharedMesh = new Mesh();
            mf.sharedMesh.CombineMeshes(m_mtrlMeshfilter[mtrl].ToArray());
            CreateMeshObj(mf.sharedMesh , index++);
            lgo.transform.parent = this.transform;
        }
    }

    public void CreateMeshObj( Mesh mesh,int index)
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("g").Append(this.name + index.ToString()).Append("\n");

        foreach (Vector3 vertex in mesh.vertices)
        {
            builder.AppendLine(string.Format("v {0} {1} {2}", -vertex.x, vertex.y, vertex.z));
        }
        builder.Append("\n");
        Debug.Log("法线数量 " + mesh.normals.Length);
        foreach (Vector3 normal in mesh.normals)
        {
            builder.AppendLine(string.Format("vn {0} {1} {2}",normal.x, normal.y, normal.z));
        }
        builder.Append("\n");
        foreach (Vector2 uv in mesh.uv)
        {
            builder.AppendLine(string.Format("vt {0} {1}",uv.x , uv.y));
        }

        for (int m = 0; m < mesh.subMeshCount; m++)
        {
            builder.Append("\n");

            int[] triangles = mesh.GetTriangles(m);

            for (int i = triangles.Length-1; i >= 0; i-=3 )
            {
                builder.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", triangles[i] + 1, triangles[i - 1] + 1, triangles[i - 2] + 1));
            }
        }

        using ( StreamWriter sw = new StreamWriter(Application.dataPath +"/Combine/" + this.name + index.ToString() + ".obj" ) )
        {
            sw.Write(builder.ToString());
        }
    }
}
