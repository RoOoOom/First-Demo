using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//	NavEditArea.cs
//	Author: Lu Zexi
//	2014-07-08


namespace Game.NavMesh
{
	/// <summary>
	/// Nav edit Square.
	/// </summary>
	public class NavEditSquare
	{
        private GameObject startPoint;
        private GameObject endPoint;
        public GameObject parent;

        private List<GameObject> allPoints = new List<GameObject>();

        public void RemovePoint(int index)
        {
            if (index < 1)
            {
                DestroyObj(startPoint);
                DestoryAllPointExcept(endPoint);
            } else
            {
                DestroyObj(endPoint);
                DestoryAllPointExcept(startPoint);
            } 
        }

        public void AddPoint(GameObject go)
        { 
            if (startPoint == null)
            {
                startPoint = go;
            } else
            {
                if (startPoint.transform.position.x == go.transform.position.x 
                    || startPoint.transform.position.y == go.transform.position.y)
                {
                    GameObject.DestroyImmediate(go);
                    Debug.LogError("两点的x or y一致，无法构成正四边形");
                } else
                {
                    DestroyObj(endPoint);
                    endPoint = go;
                }
                
            }

            if (endPoint == null || startPoint == null)
            {
                DestoryOtherPoint();
            }
            else
            {
                DestoryOtherPoint();
                allPoints.Add(startPoint);
                GameObject obj1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj1.transform.parent = startPoint.transform.parent;
                obj1.transform.position = new Vector3(startPoint.transform.position.x, endPoint.transform.position.y);
                obj1.name = "point(" + obj1.transform.position.x + "," + obj1.transform.position.y + ")";
                allPoints.Add(obj1);
                allPoints.Add(endPoint);
                GameObject obj2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj2.transform.parent = startPoint.transform.parent;
                obj2.transform.position = new Vector3(endPoint.transform.position.x, startPoint.transform.position.y);
                obj2.name = "point(" + obj2.transform.position.x + "," + obj2.transform.position.y + ")";
                allPoints.Add(obj2);
            }
        } 

        public List<GameObject> AllPoints
        {
            get
            {
                return this.allPoints;
            }
        }

        public GameObject StartPoint
        {
            get
            {
                return startPoint;
            }
        }

        public GameObject EndPoint
        {
            get
            {
                return endPoint;
            }
        }



		/// <summary>
		/// Destroy this instance.
		/// </summary>
		public void Destroy()
		{ 
            DestroyObj(startPoint);
            DestroyObj(endPoint);
            DestoryAllPointExcept(null);
            DestroyObj(parent);
        }

        public void DestroyObj(GameObject go)
        {
            if (go != null)
            {
                GameObject.DestroyImmediate(go);
            }
        }

        private void DestoryAllPointExcept(GameObject go)
        {
             foreach(GameObject obj in allPoints)
            {
                if (obj != go)
                {
                    DestroyObj(obj);
                }
            }
            allPoints.Clear();
        }

        private void DestoryOtherPoint()
        {
            foreach (GameObject obj in allPoints)
            {
                if (obj != startPoint && obj != endPoint)
                {
                    DestroyObj(obj);
                }
            }
            allPoints.Clear();
        }
	}

}