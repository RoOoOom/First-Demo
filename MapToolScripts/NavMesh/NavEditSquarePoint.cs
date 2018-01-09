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
    public class NavEditSquarePoint
    {
        private NavEditSquare square;
        public List<GameObject> points = new List<GameObject>(); 


        public NavEditSquarePoint(GameObject parent)
        {
            square = new NavEditSquare();
            square.parent = parent;
        }

        public NavEditSquarePoint()
        {
            square = new NavEditSquare();
        }

        public NavEditSquare Square
        {
            get
            {
                return this.square;
            }
        }

        public GameObject Parent
        {
            set
            {
                this.square.parent = value;
            } 
            get
            {
                return this.square.parent;
            }
        }

        public void AddBorder(GameObject go)
        {
            square.AddPoint(go);
        }

        public void RemoveBorder(int index)
        {
            square.RemovePoint(index);
            DestoryAllPoint();
        }

        public GameObject GetStartBorder()
        {
            return square.StartPoint;
        }

        public GameObject GetEndBorder()
        {
            return square.EndPoint;
        }

        public void AddPoint(GameObject point)
        {
            if (square.StartPoint == null || square.EndPoint == null)
            {
                return;
            }
            Vector3 position = point.transform.position;
            point.transform.position = new Vector3(Mathf.CeilToInt(position.x), Mathf.CeilToInt(position.y), Mathf.CeilToInt(position.z));
            float startX = square.StartPoint.transform.position.x;
            float startY = square.StartPoint.transform.position.y;

            float endX = square.EndPoint.transform.position.x;
            float endY = square.EndPoint.transform.position.y; 

             
            points.Add(point);
        }

        public void RemovePoint(int index)
        {
            if (points.Count > index)
            {
                GameObject.DestroyImmediate(points[index]);
                points.RemoveAt(index);
            }
        }

        public void Destory()
        {
            if (square != null)
            {
                square.Destroy();
            }
            DestoryAllPoint();
        }

        public void DestoryAllPoint()
        {
            foreach (GameObject point in points)
            {
                GameObject.DestroyImmediate(point);
            }
            points.Clear();
        }


    }

}