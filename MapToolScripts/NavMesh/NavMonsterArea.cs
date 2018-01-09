using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.NavMesh
{
    public class NavEditMonsterArea 
    {
        private NavEditArea area;
        public List<GameObject> allPoints = new List<GameObject>();
        public GameObject parent; 

        public List<Triangle> m_lstTriangle = new List<Triangle>();

        public List<Vector2> randPoints = new List<Vector2>();

        public Polygon polygon;
        public NavEditMonsterArea()
        {
            this.area = new NavEditArea();
        }

        public NavEditMonsterArea(NavEditArea area)
        {
            this.area = area;
        }

        public NavEditArea Area
        {
            get
            {
                return this.area;
            }
        }

        public void RandPoint()
        {
            GeneTriangle();
            randPoints.Clear();
            double minArea = 1;
            int totalShare = 0;
            for (int i = 0; i < this.m_lstTriangle.Count; i++)
            {
                float triArea = (float)this.m_lstTriangle[i].GetArea();
                totalShare += Mathf.CeilToInt(triArea);
            }

            if (totalShare != 0)
            {
                int totalNum = 100;
                for (int i = 0; i < this.m_lstTriangle.Count; i++)
                {
                    int pointNum = Mathf.CeilToInt((float)totalNum / totalShare * Mathf.CeilToInt((float)this.m_lstTriangle[i].GetArea()));
                    randPoints.AddRange(this.m_lstTriangle[i].GeneRandPoint(pointNum));
                }
            } 
        }

        public void AddAreaBorder(GameObject go, int index)
        {
            this.area.Insert(go, index);
            GeneTriangle();
            GenePlgoy(); 
        }


        public bool AddPoint(GameObject go, int index)
        {
            GeneTriangle();
            GenePlgoy();
            //if (PointInPolygon(new Vector2(go.transform.position.x, go.transform.position.y)))
            //{
            //    Debug.LogError("is polygon in");
            //}
            if (PointIsArea(new Vector2(go.transform.position.x, go.transform.position.y)))
            {
                if (index >= this.allPoints.Count - 1)
                {
                    this.allPoints.Add(go);
                }
                else if (index >= 0 && index < this.allPoints.Count)
                {
                    this.allPoints.Insert(index + 1, go);
                }
                return true;
            }
            return false;
            
        } 


        public void RemovePoint(int index)
        {
            if (this.allPoints.Count > index)
            {
                GameObject.DestroyImmediate(this.allPoints[index]);
                this.allPoints.RemoveAt(index);
            }
        }

        public GameObject GetPoint(int index)
        {
            if (this.allPoints.Count > index)
            {
                return this.allPoints[index];
            }
            return null;
        }

        public void Destory()
        {
            this.area.Destroy();
            foreach (var item in this.allPoints)
            {
                GameObject.DestroyImmediate(item);
            }
            this.allPoints.Clear();
        }


        public Polygon GenePlgoy()
        { 
            List<GameObject> allPoints = area.m_lstPoints;
            List<Vector2> allVecPnts = new List<Vector2>();
            for (int j = 0; j < allPoints.Count; j++)
            {
                Vector2 pos = new Vector2(allPoints[j].transform.position.x, allPoints[j].transform.position.y);
                allVecPnts.Add(pos);
            }
            polygon = new Polygon(allVecPnts);
            polygon.CW();
            return polygon;
        }

        private bool PointInPolygon(Vector2 pos)
        { 
            return polygon.PointIsIn(pos);
        }

        public void GeneTriangle()
        { 
            List<Polygon> areas = new List<Polygon>();
            areas.Add(GenePlgoy());
            int startTriID = 0;
            this.m_lstTriangle.Clear();
            List<Triangle> lstTri = new List<Triangle>();
            NavResCode genResult = NavMeshGen.sInstance.CreateNavMesh(areas, ref startTriID, 0, ref lstTri);
            foreach (Triangle item in lstTri)
            {
                this.m_lstTriangle.Add(item);
            } 
        }

        

        private bool PointIsArea(Vector2 pt)
        {
            foreach (var item in this.m_lstTriangle)
            {
                if (item.IsPointIn(pt))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
