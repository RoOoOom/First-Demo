using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;


//	NavEditAreaManager.cs
//	Author: Lu Zexi
//	2014-07-08


namespace Game.NavMesh
{
	/// <summary>
	/// Nav edit area manager.
	/// </summary>
	public class NavEditAreaManager
	{



        public enum AreaFrame
        {
            Area,
            SafeArea,
            Relive,
            Trans,
            Birth,
            Monster,
            Jump,
            NPC,
            Collection,
            Shadow
        }

        public static string NAVMESH_AREA_NAME = "导航网格";
        public static string NAVMESH_AREA_POINT_NAME = "导航网格点";

        public static string NAVMESH_BLOCK_AREA_NAME = "阻碍网格";
        public static string NAVMESH_BLOCK_AREA_POINT_NAME = "阻碍点";

        public static string SAFE_AREA_NAME = "安全区";
        public static string SAFE_AREA_POINT_NAME = "安全区";

        public static string MONSTER_AREA_NAME = "怪物刷新区";
        public static string MONSTER_AREA_BORDER_NAME = "怪物刷新边界";
        public static string MONSTER_AREA_POINT_NAME = "怪物刷新点";


        public static string COLLECTION_AREA_NAME = "采集物";
        public static string COLLECTION_AREA_BORDER_NAME = "采集物边界";
        public static string COLLECTION_AREA_POINT_NAME = "采集物刷新点";

        public static string SHADOW_AREA_NAME = "阴影区";
        public static string SHADOW_AREA_POINT_NAME = "阴影边界点";

        public static string JUMP_AREA_NAME = "跳跃区";
        public static string JUMP_AREA_POINT_NAME = "跳跃边界点";

        public static string RELIVE_AREA_NAME = "复活区域";
        public static string RELIVE_AREA_POINT_NAME = "复活点";

        public static string BIRTH_AREA_NAME = "出生区域";
        public static string BIRTH_AREA_POINT_NAME = "出生点";

        public static string TRANSFORM_AREA_NAME = "传送区域";
        public static string TRANSFORM_AREA_POINT_NAME = "传送点";

        public static string NPC_NAME = "NPC";


        public const float Scale = 10f;
		public const string EDITVERSION = "NAV_AREA_GROUP_001";
		public int m_iLastGroupID = 0;
		public List<NavEditAreaGroup> m_lstAreaGroup = new List<NavEditAreaGroup>();

		private static NavEditAreaManager s_cInstance;
		public static NavEditAreaManager sInstance
		{
			get
			{
				if( s_cInstance == null )
					s_cInstance = new NavEditAreaManager();
				return s_cInstance;
			}
		}

		public NavEditAreaManager()
		{
			//
		}


        public void Clear()
        {
            foreach (var item in m_lstAreaGroup)
            {
                item.Destroy();
            }
            m_lstAreaGroup.Clear();

        }

		/// <summary>
		/// Gets the group.
		/// </summary>
		/// <returns>The group.</returns>
		/// <param name="groupIndex">Group index.</param>
		public NavEditAreaGroup GetGroup( int groupIndex )
		{
			if( groupIndex >= 0 && groupIndex < this.m_lstAreaGroup.Count )
				return this.m_lstAreaGroup[groupIndex];
			return null;
		}

		/// <summary>
		/// Adds the group.
		/// </summary>
		public void AddGroup()
		{
			NavEditAreaGroup group = new NavEditAreaGroup();
			//group.m_iID = this.m_iLastGroupID++;
			this.m_lstAreaGroup.Add(group);
		}

		/// <summary>
		/// Removes the group.
		/// </summary>
		/// <param name="groupid">Groupid.</param>
		public void RemoveGroup( int groupindex )
		{
			this.m_lstAreaGroup[groupindex].Destroy();
			this.m_lstAreaGroup.RemoveAt(groupindex);
		}

		/// <summary>
		/// Removes all group.
		/// </summary>
		public void RemoveAllGroup()
		{
			foreach( NavEditAreaGroup item in this.m_lstAreaGroup)
			{
				item.Destroy();
			}
			this.m_lstAreaGroup.Clear();
		}

		/// <summary>
		/// Gets the area.
		/// </summary>
		/// <returns>The area.</returns>
		/// <param name="id">Identifier.</param>
		public NavEditArea GetArea( int groupIndex , int areaIndex )
		{
			if( groupIndex >=0 && groupIndex < this.m_lstAreaGroup.Count)
			{
				NavEditAreaGroup group = this.m_lstAreaGroup[groupIndex];
				return group.GetArea(areaIndex);
			}
			return null;
		}

		/// <summary>
		/// Adds the frame area.
		/// </summary>
		/// <returns>The frame area.</returns>
		/// <param name="groupIndex">Group index.</param>
		public NavEditArea AddFrameArea( int groupIndex )
		{
			return this.m_lstAreaGroup[groupIndex].CreateFrameArea();
		}

		/// <summary>
		/// Adds the new area.
		/// </summary>
		public NavEditArea AddNewArea( int groupIndex )
		{
			return this.m_lstAreaGroup[groupIndex].CreateArea();
		}

		/// <summary>
		/// Removes the area.
		/// </summary>
		/// <param name="areaid">Areaid.</param>
		public void RemoveArea( int groupIndex , int areaIndex )
		{
			this.m_lstAreaGroup[groupIndex].RemoveArea(areaIndex);
		}

		/// <summary>
		/// Adds the new point.
		/// </summary>
		/// <param name="areaid">Areaid.</param>
		/// <param name="obj">Object.</param>
		public void AddNewPoint( int groupIndex , int areaIndex , GameObject obj )
		{
			this.m_lstAreaGroup[groupIndex].m_lstArea[areaIndex].Add(obj);
		}

		/// <summary>
		/// Inserts the point.
		/// </summary>
		/// <param name="areaid">Areaid.</param>
		/// <param name="pointIndex">Point index.</param>
		/// <param name="obj">Object.</param>
		public void InsertPoint( int groupIndex , int areaIndex , int pointIndex , GameObject obj )
		{
			this.m_lstAreaGroup[groupIndex].m_lstArea[areaIndex].Insert(obj ,pointIndex);
		}

		/// <summary>
		/// Removes the point.
		/// </summary>
		/// <param name="areaid">Areaid.</param>
		/// <param name="pointIndex">Point index.</param>
		public void RemovePoint( int groupIndex, int areaIndex , int pointIndex )
		{
			this.m_lstAreaGroup[groupIndex].m_lstArea[areaIndex].RemoveAt(pointIndex);
		}

		/// <summary>
		/// Checks all points.
		/// </summary>
		public void CheckAllPoints()
		{
			foreach( NavEditAreaGroup group in this.m_lstAreaGroup )
			{
				foreach (NavEditArea item in group.m_lstArea )
				{
					int pointNum = item.m_lstPoints.Count;
					for (int i = 0; i < pointNum; i++)
					{
						if (item.m_lstPoints[i] == null)
						{
							item.RemoveAt(i);
							i--;
							pointNum--;
						}
					}
				}
			}
		}

		/// <summary>
		/// Raies the height.
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="z">The z coordinate.</param>
		public float RayHeight( float x , float z )
		{
			float SceneHeight = -100000.0f;
			Ray ray = new Ray();//构造射线
			ray.direction = -Vector3.up;
			ray.origin = new Vector3(x, 100000.0f, z);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))//排除actor
			{
				SceneHeight = hitInfo.point.y;
			}
			return SceneHeight;
		}

		/// <summary>
		/// Saves the area.
		/// </summary>
		/// <param name="path">Path.</param>
		public void SaveAreaGroup( string filePath )
		{
			UTF8Encoding utf8 = new UTF8Encoding();
			
			if (!Directory.Exists(Path.GetDirectoryName(filePath)))
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			
			FileStream fs = File.Create(filePath);
			BinaryWriter binWriter = new BinaryWriter(fs);
			
			//写入版本
			binWriter.Write(utf8.GetBytes(EDITVERSION));
            //写入区域组别数
			binWriter.Write(this.m_lstAreaGroup.Count);
			foreach( NavEditAreaGroup item in this.m_lstAreaGroup )
			{
                //写入行走区
                SaveArea(binWriter, item.m_lstArea); 
            }

            foreach (NavEditAreaGroup item in this.m_lstAreaGroup)
            {
                //写入安全区
                SaveArea(binWriter, item.m_safeArea);
            }

            foreach (NavEditAreaGroup item in this.m_lstAreaGroup)
            {
                //写入传送点
                SaveSquare(binWriter, item.GetSquareList(SquareArea.Transfer));
            }

            foreach (NavEditAreaGroup item in this.m_lstAreaGroup)
            { 
                //写入出生点
                SaveSquare(binWriter, item.GetSquareList(SquareArea.Birth));
            }

            foreach (NavEditAreaGroup item in this.m_lstAreaGroup)
            {
                //写入复活点
                SaveSquare(binWriter, item.GetSquareList(SquareArea.Relive));
            }

            foreach (var item in this.m_lstAreaGroup)
            {
                //写入刷怪点
                SaveMonsterArea(binWriter, item.monsterAreas);
            }

            foreach (var item in this.m_lstAreaGroup)
            {
                //写入采集物
                SaveMonsterArea(binWriter, item.collectionAreas);
            }

            foreach (var item in this.m_lstAreaGroup)
            {
                //写入跳跃区
                SaveArea(binWriter, item.jumpAreas);
            }

            foreach (var item in this.m_lstAreaGroup)
            {
                //写入阴影区域
                SaveArea(binWriter, item.shadowAreas);
            }

            foreach (var item in this.m_lstAreaGroup)
            {
                //写入NPC
                SavePoint(binWriter, item.NpcList);
            }

            binWriter.Close();
			fs.Close();
			
			Debug.Log("保存数据成功!");
		}

        private void SaveArea(BinaryWriter binWriter, List<NavEditArea> areaList)
        {
            //写入区域数
            binWriter.Write(areaList.Count);
            foreach (NavEditArea area in areaList)
            {
                //写入位置数量
                SaveArea(binWriter, area);
                //binWriter.Write(area.m_lstPoints.Count);
                //foreach (GameObject point in area.m_lstPoints)
                //{
                //    Vector3 pos = point.transform.position;
                //    //写入（x,y,z）,读出数据时统一顺序为：x,y,z
                //    binWriter.Write(pos.x / NavUtil.CONSTANT_SCALE);
                //    binWriter.Write(pos.y / NavUtil.CONSTANT_SCALE);
                //    binWriter.Write(pos.z / NavUtil.CONSTANT_SCALE);
                //}
            }
        }

        private void SaveSquare(BinaryWriter binWriter, List<NavEditSquare> list)
        {
            if (list == null)
            {
                binWriter.Write(0);
                return;
            }
            binWriter.Write(list.Count);
            foreach (NavEditSquare squ in list)
            {
                Vector3 startPos = squ.StartPoint.transform.position;
                binWriter.Write(startPos.x / NavUtil.CONSTANT_SCALE);
                binWriter.Write(startPos.y / NavUtil.CONSTANT_SCALE);
                binWriter.Write(startPos.z / NavUtil.CONSTANT_SCALE);
                Vector3 endPos = squ.EndPoint.transform.position;
                binWriter.Write(endPos.x / NavUtil.CONSTANT_SCALE);
                binWriter.Write(endPos.y / NavUtil.CONSTANT_SCALE);
                binWriter.Write(endPos.z / NavUtil.CONSTANT_SCALE); 
            }
        }


        private void SaveMonsterArea(BinaryWriter binWriter, List<NavEditMonsterArea> list)
        {
            if (list == null)
            {
                binWriter.Write(0);
                return;
            }
            binWriter.Write(list.Count);
            foreach (NavEditMonsterArea monster in list)
            {
                //写入位置数量
                SaveArea(binWriter, monster.Area);
                SavePoint(binWriter, monster.allPoints);  
            }
        }

        private void SaveArea(BinaryWriter binWriter, NavEditArea area)
        {
            if (area == null)
            {
                binWriter.Write(0);
                return;
            }
            //写入位置数量
            SavePoint(binWriter, area.m_lstPoints); 
        }

        private void SavePoint(BinaryWriter binWriter, List<GameObject> list)
        {
            if (null == list)
            {
                binWriter.Write(0);
                return;
            }
            binWriter.Write(list.Count);
            foreach (GameObject point in list)
            {
                Vector3 pos = point.transform.position;
                //写入（x,y,z）,读出数据时统一顺序为：x,y,z
                binWriter.Write(pos.x / NavUtil.CONSTANT_SCALE);
                binWriter.Write(pos.y / NavUtil.CONSTANT_SCALE);
                binWriter.Write(pos.z / NavUtil.CONSTANT_SCALE);
            }
        }


		/// <summary>
		/// Loads the area.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="parent">Parent.</param>
		public void LoadAreaGroup( string filePath , GameObject parentPoint, float scale = 1f )
		{
			if (null == parentPoint)
			{
				Debug.LogError("父节点为空，不合法");
				return;
			}
			// check file exist
            if (!File.Exists(filePath))
            {
                Debug.LogError("路径错误：" + filePath);
                return;
            }

			RemoveAllGroup();

			//打开文件
			FileStream fs = File.Open(filePath, FileMode.Open);
			BinaryReader binReader = new BinaryReader(fs);
			
			try
			{
				// 读取版本
				string currVersion = new string(binReader.ReadChars(EDITVERSION.Length));
				if (currVersion == EDITVERSION)
				{
                    //读取区域组别数
					int areaGroupCount = binReader.ReadInt32();
					for( int k = 0 ; k < areaGroupCount ; k++ )
					{ 
                        NavEditAreaGroup group = new NavEditAreaGroup();
                        LoadArea(binReader, group, parentPoint, scale, group.m_lstArea, AreaFrame.Area);
                        if (group.m_lstArea.Count > 0)
                        {
                            group.m_cFrameArea = group.m_lstArea[0];
                        } 
                        this.m_lstAreaGroup.Add(group); 
                    }

                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        GameObject go = new GameObject();
                        go.transform.parent = parentPoint.transform;
                        go.name = "" + i;
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        LoadArea(binReader, group, parentPoint, scale, group.m_safeArea, AreaFrame.SafeArea);
                    }

                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        //传送点
                        LoadSquare(binReader, group, parentPoint, scale, SquareArea.Transfer);
                    }

                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        //出生点
                        LoadSquare(binReader, group, parentPoint, scale, SquareArea.Birth);
                    }


                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        //复活点
                        LoadSquare(binReader, group, parentPoint, scale, SquareArea.Relive);
                    }

                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        //刷怪
                        LoadMonsterArea(binReader, group, parentPoint, scale, group.monsterAreas, AreaFrame.Monster);
                    }

                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        //采集物
                        LoadMonsterArea(binReader, group, parentPoint, scale, group.collectionAreas, AreaFrame.Collection);
                    }

                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        //跳跃区
                        LoadArea(binReader, group, parentPoint, scale, group.jumpAreas, AreaFrame.Jump);
                    }

                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        //阴影区域
                        LoadArea(binReader, group, parentPoint, scale, group.shadowAreas, AreaFrame.Shadow);
                    }

                    for (int i = 0; i < this.m_lstAreaGroup.Count; i++)
                    {
                        //写入NPC
                        NavEditAreaGroup group = this.m_lstAreaGroup[i];
                        int count = binReader.ReadInt32();
                        for (int j = 0; j < count; j++)
                        {
                            group.NpcList.Add(LoadPoint(binReader, parentPoint, scale, NPC_NAME));
                        } 
                    }
                }
				else
				{
					Debug.LogError("版本不匹配！");
				}
			}
			catch (EndOfStreamException e)
			{
				Debug.LogError(e.Message);
			}
			finally
			{
				binReader.Close();
				fs.Close();
			}
			
			Debug.Log("加载数据成功!");
		}


        private void LoadArea(BinaryReader binReader, NavEditAreaGroup group, GameObject parentPoint, float scale, List<NavEditArea> areaList, AreaFrame frame)
        { 
            // 读取区域数
            int areaCount = binReader.ReadInt32();

            for (int i = 0; i < areaCount; i++)
            {
                GameObject go = new GameObject();
                go.transform.parent = parentPoint.transform; 
                switch (frame)
                {
                    case AreaFrame.Area:
                        if (i == 0)
                        {
                            go.name = NAVMESH_AREA_NAME + i;
                        } else
                        {
                            go.name = NAVMESH_BLOCK_AREA_NAME + i;
                        }
                        break;
                    case AreaFrame.SafeArea:
                        go.name = SAFE_AREA_POINT_NAME + i;
                        break;
                    case AreaFrame.Monster:
                        go.name = MONSTER_AREA_NAME + i;
                        break;
                    case AreaFrame.Jump:
                        go.name = JUMP_AREA_NAME + i;
                        break; 
                    case AreaFrame.Collection:
                        go.name = COLLECTION_AREA_NAME + i;
                        break;
                    case AreaFrame.Shadow:
                        go.name = SHADOW_AREA_NAME + i;
                        break;
                    default:
                        break;
                } 
                
                areaList.Add(LoadArea(binReader, go, scale));
            }
        }

        private void LoadSquare(BinaryReader binReader, NavEditAreaGroup group, GameObject parent, float scale, SquareArea area)
        {
            int squareCount = binReader.ReadInt32();
            if (squareCount == 0)
            {
                return;
            }
            List<NavEditSquare> list = new List<NavEditSquare>();  

            for (int i = 0; i < squareCount; i++)
            {
                NavEditSquare square = new NavEditSquare();
                GameObject SquParent = new GameObject();
                SquParent.transform.parent = parent.transform;
                string prePointName = "";
                switch (area)
                {
                    case SquareArea.Relive:
                        SquParent.name = RELIVE_AREA_NAME + i;
                        prePointName = RELIVE_AREA_POINT_NAME;
                        break;
                    case SquareArea.Birth:
                        SquParent.name = BIRTH_AREA_NAME + i;
                        prePointName = BIRTH_AREA_POINT_NAME;
                        break;
                    case SquareArea.Transfer:
                        SquParent.name = TRANSFORM_AREA_NAME + i;
                        prePointName = TRANSFORM_AREA_POINT_NAME;
                        break;
                    default:
                        break;
                } 
                square.parent = SquParent;

                //float x = binReader.ReadSingle() * scale;
                //float y = binReader.ReadSingle() * scale;
                //float z = binReader.ReadSingle() * scale;

                //GameObject startObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //startObj.transform.parent = SquParent.transform;
                //startObj.transform.position = new UnityEngine.Vector3(x, y, z);
                //startObj.transform.localScale *= Scale;
                //square.AddPoint(startObj);

                //float x2 = binReader.ReadSingle() * scale;
                //float y2 = binReader.ReadSingle() * scale;
                //float z2 = binReader.ReadSingle() * scale;

                //GameObject startObj2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //startObj2.transform.parent = SquParent.transform;
                //startObj2.transform.position = new UnityEngine.Vector3(x2, y2, z2);
                //startObj2.transform.localScale *= Scale;
                //square.AddPoint(startObj2);
                GameObject startObj = LoadPoint(binReader, SquParent, scale, prePointName);
                square.AddPoint(startObj);
                GameObject endObj = LoadPoint(binReader, SquParent, scale, prePointName);
                square.AddPoint(endObj);
                list.Add(square);
            }
            group.squareList.Add(area, list); 
        }
         

        private void LoadMonsterArea(BinaryReader binReader, NavEditAreaGroup group, GameObject parent, float scale, List<NavEditMonsterArea> areaList, AreaFrame frame)
        {
            int monsterCount = binReader.ReadInt32();
            if (monsterCount == 0)
            {
                return;
            } 
            for (int i = 0; i < monsterCount; i++)
            {
                GameObject go = new GameObject();
                go.transform.parent = parent.transform;
                string preName = "";
                switch (frame)
                { 
                    case AreaFrame.Monster:
                        go.name = MONSTER_AREA_NAME + i;
                        preName = MONSTER_AREA_POINT_NAME;
                        break; 
                    case AreaFrame.Collection:
                        go.name = COLLECTION_AREA_NAME + i;
                        preName = COLLECTION_AREA_POINT_NAME;
                        break; 
                    default:
                        break;
                }
                NavEditArea area = LoadArea(binReader, go, scale);
                NavEditMonsterArea monster = new NavEditMonsterArea(area);
                int pointCount = binReader.ReadInt32();
                for (int j = 0; j < pointCount; j++)
                {
                    GameObject point = LoadPoint(binReader, go, scale, preName);
                    monster.AddPoint(point, j);
                }
                areaList.Add(monster); 
            }
        }

        private NavEditArea LoadArea(BinaryReader binReader, GameObject parent, float scale)
        {
            NavEditArea area = new NavEditArea();
            //读取位置数
            int pointCount = binReader.ReadInt32();

            for (int j = 0; j < pointCount; j++)
            { 
                GameObject point = LoadPoint(binReader, parent, scale, null);
                area.m_lstPoints.Add(point);
            }
            return area;
        }

        private GameObject LoadPoint(BinaryReader binReader, GameObject parent, float scale, string preName)
        {
            float x = binReader.ReadSingle() * scale;
            float y = binReader.ReadSingle() * scale;
            float z = binReader.ReadSingle() * scale;

            // auto generate point gameobject
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.parent = parent.transform;
            point.transform.position = new UnityEngine.Vector3(x, y, z);
            point.transform.localScale *= Scale;
            if (preName != null && preName != "")
            {
                point.name = preName + "(" + x + "," + y + ")";
            }
            return point;
        }

        /// <summary>
		/// Loads the 3d area.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="parent">Parent.</param>
		public void Load3DAreaGroup(string filePath, GameObject parentPoint, float scale = 1f)
        {
            if (null == parentPoint)
            {
                Debug.LogError("父节点为空，不合法");
                return;
            }
            // check file exist
            if (!File.Exists(filePath))
            {
                Debug.LogError("路径错误：" + filePath);
                return;
            }

            RemoveAllGroup();

            //打开文件
            FileStream fs = File.Open(filePath, FileMode.Open);
            BinaryReader binReader = new BinaryReader(fs);

            try
            {
                // 读取版本
                string currVersion = new string(binReader.ReadChars(EDITVERSION.Length));
                if (currVersion == EDITVERSION)
                {
                    //读取区域组别数
                    int areaGroupCount = binReader.ReadInt32();
                    for (int k = 0; k < areaGroupCount; k++)
                    {
                        NavEditAreaGroup group = new NavEditAreaGroup();
                        // 读取区域数
                        int areaCount = binReader.ReadInt32();

                        for (int i = 0; i < areaCount; i++)
                        {
                            NavEditArea area = new NavEditArea();

                            //读取位置数
                            int pointCount = binReader.ReadInt32();

                            for (int j = 0; j < pointCount; j++)
                            {
                                // 读取位置（x,y,z）
                                float x = binReader.ReadSingle() * scale;
                                float y = binReader.ReadSingle() * scale;
                                float z = binReader.ReadSingle() * scale;

                                // auto generate point gameobject
                                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                point.transform.parent = parentPoint.transform;
                                point.transform.position = new UnityEngine.Vector3(x, z, y);
                                point.transform.localScale *= Scale;

                                area.m_lstPoints.Add(point);
                            }

                            group.m_lstArea.Add(area);
                        }

                        if (group.m_lstArea.Count > 0)
                        {
                            group.m_cFrameArea = group.m_lstArea[0];
                        }

                        this.m_lstAreaGroup.Add(group);
                    }
                }
                else
                {
                    Debug.LogError("版本不匹配！");
                }
            }
            catch (EndOfStreamException e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                binReader.Close();
                fs.Close();
            }

            Debug.Log("加载数据成功!");
        }

        public List<Polygon> GetSafeAreaPolygon()
        {
            List<Polygon> listPolygon = new List<Polygon>();
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
                for (int j = 0; j < group.m_safeArea.Count; j++)
                {
                    Polygon areas = GetAreaPolygon(group.m_safeArea[j]);
                    if (areas.GetPoints().Count == 0)
                    {
                        UnityEditor.EditorUtility.DisplayDialog("提示", "group " + i + "， 安全区" + j + " 未设置信息, 请设置信息后导出", "OK");
                    }
                    areas.CW();
                    listPolygon.Add(areas);
                }

                //foreach (var item in group.m_safeArea)
                //{ 
                //    Polygon areas = GetAreaPolygon(item);
                //    areas.CW();
                //    listPolygon.Add(areas);
                //} 
            }
            return listPolygon;
        }

        public List<Polygon> GetJumpAreaPolygon()
        {
            List<Polygon> listPolygon = new List<Polygon>();
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
                for (int j = 0; j < group.jumpAreas.Count; j++)
                {
                    Polygon areas = GetAreaPolygon(group.jumpAreas[j]);
                    if (areas.GetPoints().Count == 0)
                    {
                        UnityEditor.EditorUtility.DisplayDialog("提示", "group " + i + "， 跳跃点" + j + " 未设置信息, 请设置信息后导出", "OK");
                    }
                    areas.CW();
                    listPolygon.Add(areas);
                }
                //foreach (var item in group.jumpAreas)
                //{
                //    Polygon areas = GetAreaPolygon(item);
                //    areas.CW();
                //    listPolygon.Add(areas);
                //}

            }
            return listPolygon;
        }


        public List<Polygon> GetShadowAreaPolygon()
        {
            List<Polygon> listPolygon = new List<Polygon>();
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
                for (int j = 0; j < group.shadowAreas.Count; j++)
                {
                    Polygon areas = GetAreaPolygon(group.shadowAreas[j]);
                    if (areas.GetPoints().Count == 0)
                    {
                        UnityEditor.EditorUtility.DisplayDialog("提示", "group " + i + "， 阴影区" + j + " 未设置信息, 请设置信息后导出", "OK");
                    }
                    areas.CW();
                    listPolygon.Add(areas);
                }
                //foreach (var item in group.shadowAreas)
                //{
                //    Polygon areas = GetAreaPolygon(item);
                //    if (areas.GetPoints().Count == 0)
                //    {
                //        UnityEditor.EditorUtility.DisplayDialog("提示", "group " + i + "， 阴影区" , "OK");
                //    }
                //    areas.CW();
                //    listPolygon.Add(areas);
                //}

            }
            return listPolygon;
        }


        public Dictionary<int, List<Polygon>> GetAreaPolygon()
        {
            Dictionary<int, List<Polygon>> dic = new Dictionary<int, List<Polygon>>();
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
                List<Polygon> areas = GetAreas(group.m_lstArea);
                dic.Add(i, areas);
            }
            return dic;
        }




        public List<Triangle> GetAreaTriangle()
        { 
            List<Triangle> lstTri = new List<Triangle>();
            List<Triangle> allTri = new List<Triangle>();
            int startTriID = 0;
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i]; 
                List<Polygon> areas = GetAreas(group.m_lstArea); 
                lstTri.Clear();
                NavResCode genResult = NavMeshGen.sInstance.CreateNavMesh(areas, ref startTriID, i, ref lstTri);
                foreach (Triangle item in lstTri)
                {
                    allTri.Add(item);
                }
            }
            return allTri;
        }

        public Dictionary<int, List<Triangle>> GetGroupAreaTriangle()
        {
            Dictionary<int, List<Triangle>> dic = new Dictionary<int, List<Triangle>>();
            List<Triangle> lstTri = new List<Triangle>(); 
            int startTriID = 0;
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                List<Triangle> allTri = new List<Triangle>();
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
                List<Polygon> areas = GetAreas(group.m_lstArea); 
                lstTri.Clear();
                NavResCode genResult = NavMeshGen.sInstance.CreateNavMesh(areas, ref startTriID, i, ref lstTri);
                foreach (Triangle item in lstTri)
                {
                    allTri.Add(item);
                }
                dic.Add(i, allTri);
            }
            return dic;
        }



        public List<NavEditSquare> GetSquare(SquareArea area)
        {
            List<NavEditSquare> list = new List<NavEditSquare>();
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
                List<NavEditSquare> squList = group.GetSquareList(area);
                if (squList == null)
                {
                    continue;
                }
                foreach (var item in squList)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public List<NavEditMonsterArea> GetMonsterArea()
        {
            List<NavEditMonsterArea> list = new List<NavEditMonsterArea>();
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];  
                List<NavEditMonsterArea> monsterAreas = group.monsterAreas;
                for (int j = 0; j < monsterAreas.Count; j++)
                {
                    NavEditMonsterArea item = monsterAreas[j];
                    if (item.Area.m_lstPoints.Count == 0)
                    {
                        UnityEditor.EditorUtility.DisplayDialog("提示", "group " + i + "， 怪物刷新区" + j +" 未设置信息, 请设置信息后导出", "OK");
                    }
                    item.GeneTriangle();
                    list.Add(item); 
                }



                //foreach (var item in monsterAreas)
                //{

                //    item.GeneTriangle();
                //    list.Add(item);
                //}
            }
            return list;
        }

        public List<NavEditMonsterArea> GetCollectionArea()
        {
            List<NavEditMonsterArea> list = new List<NavEditMonsterArea>();
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
                List<NavEditMonsterArea> monsterAreas = group.collectionAreas;

                for (int j = 0; j < monsterAreas.Count; j++)
                {
                    NavEditMonsterArea item = monsterAreas[j];
                    if (item.Area.m_lstPoints.Count == 0)
                    {
                        UnityEditor.EditorUtility.DisplayDialog("提示", "group " + i + "， 采集物刷新区" + j + " 未设置信息, 请设置信息后导出", "OK");
                    }
                    item.GeneTriangle();
                    list.Add(item);
                }
                //foreach (var item in monsterAreas)
                //{
                //    item.GeneTriangle();
                //    list.Add(item);
                //}
            }
            return list;
        }

        public List<Vector3> GetNpcPostionList()
        {
            List<Vector3> list = new List<Vector3>();
            for (int i = 0; i < m_lstAreaGroup.Count; i++)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
                List<GameObject> npcList = group.NpcList;
                foreach (var item in npcList)
                { 
                    list.Add(item.transform.position);
                }
            }
            return list;
        }




        /// <summary>
        /// get the safe area point
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private List<Polygon> GetAreas(List<NavEditArea> lst)
        {
            List<Polygon> areas = new List<Polygon>();
            for (int i = 0; i < lst.Count; i++)
            { 
                areas.Add(GetAreaPolygon(lst[i]));
            }

            return areas;
        }

        private Polygon GetAreaPolygon(NavEditArea area)
        {
            List<GameObject> allPoints = area.m_lstPoints;
            List<Vector2> allVecPnts = new List<Vector2>();
            for (int j = 0; j < allPoints.Count; j++)
            {
                Vector2 pos = new Vector2(allPoints[j].transform.position.x, allPoints[j].transform.position.y);
                allVecPnts.Add(pos);
            }
            return new Polygon(allVecPnts); 
        }

        public List<Triangle> GeneTriangle(Polygon ploygon)
        {
            List<Triangle> lstTri = new List<Triangle>();
            List<Polygon> polyList = new List<Polygon>();
            polyList.Add(ploygon);
            int startTriID = 0;
            NavMeshGen.sInstance.CreateNavMesh(polyList, ref startTriID, 0, ref lstTri);
            return lstTri;
        }

        public List<Triangle> GeneTriangle(List<Polygon> polyList)
        {
            List<Triangle> lstTri = new List<Triangle>();  
            int startTriID = 0;
            NavMeshGen.sInstance.CreateNavMesh(polyList, ref startTriID, 0, ref lstTri);
            return lstTri;
        }

        int size = 20;
        public List<List<Vector2>> GeneTile(Polygon poly)
        {
            List<Vector2> square = GetPolygonSquare(poly);
            if (square == null)
            {
                return null;
            }
            int leftX = Mathf.CeilToInt(square[0].x / 20) * 20;
            int bottomY = Mathf.CeilToInt(square[0].y / 20) * 20;
            int rightX = Mathf.CeilToInt(square[2].x / 20) * 20;
            int TopY = Mathf.CeilToInt(square[2].y / 20) * 20;

            int x = leftX;
            int y = bottomY;

            int i = 0;
            List<List<Vector2>> tileList = new List<List<Vector2>>();
            while (x <= rightX)
            {
                y = bottomY;
                while(y <= TopY)
                {
                    //List<Vector2> list = new List<Vector2>();
                    //Vector2 vec = new Vector2(x, y);
                    //Vector2 v2 = new Vector2(x, y + size);
                    //Vector2 v3 = new Vector2(x + size, y + size);
                    //Vector2 v4 = new Vector2(x + size, y);
                    //list.Add(vec);
                    //list.Add(v2);
                    //list.Add(v3);
                    //list.Add(v4);
                    //tileList.Add(list);


                    Vector2 vec = new Vector2(x, y);
                    if (poly.PointIsIn(vec))
                    {
                        List<Vector2> list = new List<Vector2>();
                        Vector2 v2 = new Vector2(x, y + size);
                        Vector2 v3 = new Vector2(x + size, y + size);
                        Vector2 v4 = new Vector2(x + size, y);
                        if (poly.PointIsIn(v2) && poly.PointIsIn(v3) && poly.PointIsIn(v4))
                        {
                            if (!poly.LineIntersectNotEndPoint(new Line2D(vec, v2)) && !poly.LineIntersectNotEndPoint(new Line2D(v2, v3))
                                && !poly.LineIntersectNotEndPoint(new Line2D(v3, v4)) && !poly.LineIntersectNotEndPoint(new Line2D(v4, vec)))
                            { 
                                list.Add(vec);
                                list.Add(v2);
                                list.Add(v3);
                                list.Add(v4);
                                tileList.Add(list);
                            }
                        }
                    }
                    y += size;


                }
                x += size;
            }
            return tileList;
        } 


        public List<Vector2> GetPolygonSquare(Polygon poly)
        { 
            if (poly == null)
            {
                return null;
            }
            List<Vector2> list = poly.GetPoints();
            if (list.Count < 3)
            {
                return null;
            }
            List<Vector2> vecList = new List<Vector2>();
            Vector2 min = list[0];
            Vector2 max = list[0];
            foreach (var item in list)
            {
                if (item.x < min.x)
                {
                    min.x = item.x;
                }
                if (item.y < min.y)
                {
                    min.y = item.y;
                }
                if (item.x > max.x)
                {
                    max.x = item.x;
                }
                if (item.y > max.y)
                {
                    max.y = item.y;
                }
            }
            vecList.Add(min);
            vecList.Add(new Vector2(min.x, max.y));
            vecList.Add(max);
            vecList.Add(new Vector2(max.x, min.y));
            return vecList;
        }

    }
}
