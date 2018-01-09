using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


//	NavEditAreaGroup.cs
//	Author: Lu Zexi
//	2014-07-08



namespace Game.NavMesh
{

    public enum SquareArea
    {
        Relive,
        Birth,
        Transfer
    }


    /// <summary>
    /// Nav edit area group.
    /// </summary>
    public class NavEditAreaGroup
    {
        //public int m_iID;
        public NavEditArea m_cFrameArea;
        public List<NavEditArea> m_lstArea = new List<NavEditArea>();
        public List<NavEditArea> m_safeArea = new List<NavEditArea>();
        public Dictionary<SquareArea, List<NavEditSquare>> squareList = new Dictionary<SquareArea, List<NavEditSquare>>();
        public List<NavEditSquarePoint> squarePointList = new List<NavEditSquarePoint>();
        public List<NavEditMonsterArea> monsterAreas = new List<NavEditMonsterArea>();
        public List<NavEditMonsterArea> collectionAreas = new List<NavEditMonsterArea>();
        public List<NavEditArea> shadowAreas = new List<NavEditArea>();
        public List<NavEditArea> jumpAreas = new List<NavEditArea>();
        public List<GameObject> NpcList = new List<GameObject>();


        //public int m_iLastAreaID = 1;


        public NavEditArea GetSafeArea(int areaIndex)
        {
            if (areaIndex >= 0 && areaIndex < this.m_safeArea.Count)
                return this.m_safeArea[areaIndex];
            return null;
        }

        /// <summary>
        /// 创建安全区
        /// </summary>
        /// <returns></returns>
        public NavEditArea CreateSafeArea()
        {
            NavEditArea area = new NavEditArea();
            this.m_safeArea.Add(area);
            return area;
        }

        /// <summary>
        /// 删除安全区
        /// </summary>
        /// <param name="areaIndex"></param>
        public void RemoveSafeArea(int areaIndex)
        {
            this.m_safeArea[areaIndex].Destroy();
            this.m_safeArea.RemoveAt(areaIndex);
        }




        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <returns>The area.</returns>
        /// <param name="areaid">Areaid.</param>
        public NavEditArea GetArea(int areaIndex)
        {
            if (areaIndex >= 0 && areaIndex < this.m_lstArea.Count)
                return this.m_lstArea[areaIndex];
            return null;
        }


        /// <summary>
        /// Adds the new area.
        /// </summary>
        public NavEditArea CreateArea()
        {
            NavEditArea area = new NavEditArea();
            this.m_lstArea.Add(area);
            return area;
        }

        /// <summary>
        /// Adds the frame area.
        /// </summary>
        /// <returns>The frame area.</returns>
        public NavEditArea CreateFrameArea()
        {
            if (this.m_cFrameArea != null)
            {
                this.m_lstArea.Remove(this.m_cFrameArea);
                this.m_cFrameArea.Destroy();
                this.m_cFrameArea = null;
            }
            NavEditArea area = new NavEditArea();
            this.m_cFrameArea = area;
            this.m_lstArea.Insert(0, this.m_cFrameArea);
            return this.m_cFrameArea;
        }

        /// <summary>
        /// Removes the frame area.
        /// </summary>
        public void RemoveFrameArea()
        {
            this.m_lstArea.Remove(this.m_cFrameArea);
            this.m_cFrameArea.Destroy();
            this.m_cFrameArea = null;
        }

        /// <summary>
        /// Removes the area.
        /// </summary>
        /// <param name="index">Index.</param>
        public void RemoveArea(int areaIndex)
        {
            this.m_lstArea[areaIndex].Destroy();
            this.m_lstArea.RemoveAt(areaIndex);
        }

//=========================================square area====================
        public NavEditSquare CreateSquare(SquareArea area)
        {
            NavEditSquare seq = new NavEditSquare();
            if (this.squareList.ContainsKey(area))
            {
                this.squareList[area].Add(seq);
            } else
            {
                List<NavEditSquare> list = new List<NavEditSquare>(); 
                list.Add(seq); 
                this.squareList.Add(area, list);
            } 
            return seq;
        }

        public void RemoveSquare(SquareArea area, int areaIndex)
        { 
            if (this.squareList.ContainsKey(area))
            {
                List<NavEditSquare> list = this.squareList[area];
                if (areaIndex >= 0 && list.Count > areaIndex)
                {
                    list[areaIndex].Destroy();
                    list.RemoveAt(areaIndex);
                }
            } else
            {
                Debug.LogError("删除 " + area.ToString() + "不存在");
            }  
        }

        public NavEditSquare GetSquare(SquareArea area, int areaIndex)
        {
            if (this.squareList.ContainsKey(area))
            {
                List<NavEditSquare> list = this.squareList[area];
                if (areaIndex >= 0 && list.Count > areaIndex)
                {
                    return list[areaIndex];
                }
            } 
            return null;
        }

        public List<NavEditSquare> GetSquareList(SquareArea area)
        {
            if (this.squareList.ContainsKey(area))
            {
                return this.squareList[area]; 
            }
            return null;
        }


        public NavEditSquarePoint CreateSquarePoint()
        {
            NavEditSquarePoint point = new NavEditSquarePoint();
            squarePointList.Add(point);
            return point;
        }

        public void RemoveSquarePoint(int index)
        {
            if (index >= 0 && squarePointList.Count > index)
            {
                squarePointList[index].Destory();
                squarePointList.RemoveAt(index);
            }
        }

        public NavEditSquarePoint GetSquarePoint(int index)
        {
            if (index >= 0 && this.squarePointList.Count > index)
            {
                return this.squarePointList[index];
            }
            return null;
        }
//==========================================================怪物刷新区域
        public NavEditMonsterArea CreateMonsterArea()
        {
            NavEditMonsterArea area = new NavEditMonsterArea();
            this.monsterAreas.Add(area);
            return area;
        }

        public void RemoveMonsterArea(int index)
        {
            if (index >= 0 && this.monsterAreas.Count > index)
            {
                this.monsterAreas[index].Destory();
                this.monsterAreas.RemoveAt(index);
            }
        }

        public NavEditMonsterArea GetMonsterArea(int index)
        {
            if (index >= 0 && this.monsterAreas.Count > index)
            {
                return this.monsterAreas[index];
            }
            return null;
        }

        public List<NavEditMonsterArea> GetAllMonsterArea()
        {
            return this.monsterAreas;
        }

//==========================================================采集物刷新区域
        public NavEditMonsterArea CreateCollectionArea()
        {
            NavEditMonsterArea area = new NavEditMonsterArea();
            this.collectionAreas.Add(area);
            return area;
        }

        public void RemovCollectionArea(int index)
        {
            if (index >= 0 && this.collectionAreas.Count > index)
            {
                this.collectionAreas[index].Destory();
                this.collectionAreas.RemoveAt(index);
            }
        }

        public NavEditMonsterArea GetCollectionArea(int index)
        {
            if (index >= 0 && this.collectionAreas.Count > index)
            {
                return this.collectionAreas[index];
            }
            return null;
        }

        public List<NavEditMonsterArea> GetAllCollectionArea()
        {
            return this.collectionAreas;
        }

        //==========================================================阴影区
        public NavEditArea GetShadowArea(int areaIndex)
        {
            if (areaIndex >= 0 && areaIndex < this.shadowAreas.Count)
                return this.shadowAreas[areaIndex];
            return null;
        }
         
        public NavEditArea CreateShadowArea()
        {
            NavEditArea area = new NavEditArea();
            this.shadowAreas.Add(area);
            return area;
        }
         
        public void RemoveShadowArea(int areaIndex)
        {
            if (areaIndex >= 0 && this.shadowAreas.Count > areaIndex)
            {
                this.shadowAreas[areaIndex].Destroy();
                this.shadowAreas.RemoveAt(areaIndex);
            } 
        }

//==========================================================跳跃区
        public NavEditArea GetJumpArea(int areaIndex)
        {
            if (areaIndex >= 0 && areaIndex < this.jumpAreas.Count)
                return this.jumpAreas[areaIndex];
            return null;
        }

        public NavEditArea CreateJumpArea()
        {
            NavEditArea area = new NavEditArea();
            this.jumpAreas.Add(area);
            return area;
        }

        public void RemoveJumpArea(int areaIndex)
        {
            if (areaIndex >= 0 && this.jumpAreas.Count > areaIndex)
            {
                this.jumpAreas[areaIndex].Destroy();
                this.jumpAreas.RemoveAt(areaIndex);
            }
        }


//==========================================================NPC
        public GameObject GetNPC(int areaIndex)
        {
            if (areaIndex >= 0 && areaIndex < this.NpcList.Count)
                return this.NpcList[areaIndex];
            return null;
        }

        public void AddNPC(GameObject npc, int index)
        {
            if (index >= this.NpcList.Count - 1)
            {
                this.NpcList.Add(npc);
            }
            else if (index >= 0 && index < this.NpcList.Count)
            {
                this.NpcList.Insert(index + 1, npc);
            } 
        }

        public void RemoveNPC(int areaIndex)
        {
            if (areaIndex >= 0 && this.NpcList.Count > areaIndex)
            {
                GameObject.DestroyImmediate(this.NpcList[areaIndex]);
                this.NpcList.RemoveAt(areaIndex);
            }
        }


        /// <summary>
        /// Destroy this instance.
        /// </summary>
        public void Destroy()
		{
			foreach( NavEditArea item in this.m_lstArea )
			{
				item.Destroy();
			}
            foreach (NavEditArea item in this.m_safeArea)
            {
                item.Destroy();
            }
            foreach(var item in squareList)
            {
                List<NavEditSquare> list = item.Value; 
                for (int i = 0; i < list.Count; i++)
                { 
                    list[i].Destroy();  
                }
            }

            foreach (var item in this.squarePointList)
            {
                item.Destory();
            }

            foreach (var item in this.monsterAreas)
            {
                item.Destory();
            }

            foreach (var item in this.collectionAreas)
            {
                item.Destory();
            }

            foreach (var item in this.shadowAreas)
            {
                item.Destroy();
            }

            foreach (var item in this.jumpAreas)
            {
                item.Destroy();
            }

            foreach (var item in this.NpcList)
            {
                GameObject.DestroyImmediate(item);
            }


            this.m_lstArea.Clear();
            this.m_safeArea.Clear();
            this.squareList.Clear();
            this.squarePointList.Clear();
            this.monsterAreas.Clear();
            this.collectionAreas.Clear();
            this.shadowAreas.Clear();
            this.jumpAreas.Clear();
            this.NpcList.Clear();
			this.m_cFrameArea = null;
		}

	}

}