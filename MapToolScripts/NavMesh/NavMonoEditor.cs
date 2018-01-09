using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Game.NavMesh;

//	NavMonoEditor.cs
//	Author: Lu Zexi
//	2014-07-08



namespace Game.NavMesh
{
	/// <summary>
	/// Nav mono editor.
	/// </summary>
	public class NavMonoEditor : MonoBehaviour
	{
		public static NavTriangle StopTri;
		public static List<NavTriangle> m_lstTriPath = new List<NavTriangle>();
		private List<Triangle> m_lstTriangle = new List<Triangle>();	//navmesh triangle
        private List<Polygon> m_safePolygon = new List<Polygon>(); 

		public List<Vector2> m_lstFindPath = new List<Vector2>();	//findPath;

		public bool m_bShowMesh = true;	//is show mesh
		public bool m_bShowArea = true;
		public int m_iSelGroup;	//the selected group
		public int m_iSelArea;	//the selected area
		public int m_iSelPoint;	//the selected point

        public int m_iSelectSafeArea; //安全区
        public int m_iSelSafePoint; //安全区中的点

        public int m_iSelReliveArea; //复活区
        public int m_iSelRelivePoint;

        public int m_iSelTransferArea; //传送区
        public int m_iSelTransferPoint;

        public int m_iSelBirthArea; //出生点
        public int m_iSelBirthPoint;

        public int m_iSelMonsterArea; //刷怪点 
        public int m_iSelectBorderPoint;
        public int m_iSelMonsterPoint;


        public int m_iSelCollectionArea; //采集物
        public int m_iSelCollectionBorderPoint;
        public int m_iSelCollectionPoint;


        public int m_iSelShadowArea;  //阴影区
        public int m_iSelShadowPoint;

        public int m_iSelJumpArea;  //阴影区
        public int m_iSelJumpPoint;

        public int m_iSelNpc;  //NPC

        public int m_iSelMonsterSquare;


        // Use this for initialization
        void Start ()
		{
			//
		}
	
		// Update is called once per frame
		void Update ()
		{
			//
		}

        public void Clear()
        {
            m_lstTriPath.Clear();
            m_lstTriangle.Clear();
            m_safePolygon.Clear();
            m_lstFindPath.Clear();

            m_bShowMesh = true; //is show mesh
            m_bShowArea = true;
            m_iSelGroup = 0; //the selected group
            m_iSelArea = 0;  //the selected area
            m_iSelPoint = 0;     //the selected point

            m_iSelectSafeArea = 0;  //安全区
            m_iSelSafePoint = 0; //安全区中的点

            m_iSelReliveArea = 0; //复活区
            m_iSelRelivePoint = 0; ;

            m_iSelTransferArea = 0; //传送区
            m_iSelTransferPoint = 0;

            m_iSelBirthArea = 0; //出生点
            m_iSelBirthPoint = 0;

            m_iSelMonsterArea = 0; //刷怪点 
            m_iSelectBorderPoint = 0;
            m_iSelMonsterPoint = 0;


            m_iSelCollectionArea = 0; //采集物
            m_iSelCollectionBorderPoint = 0;
            m_iSelCollectionPoint = 0;


            m_iSelShadowArea = 0;  //阴影区
            m_iSelShadowPoint = 0;

            m_iSelJumpArea = 0;  //阴影区
            m_iSelJumpPoint = 0;

            m_iSelNpc = 0;  //NPC

            m_iSelMonsterSquare = 0;
        }

		void OnDrawGizmos()
		{
			DrawAllAreas(Color.cyan);
			DrawNavMesh();
			DrawFindPath();
			DrawPathTriangle();
            DrawAllSafeAreas(Color.green);
            DrawSquares(SquareArea.Relive, Color.gray);
            DrawSquares(SquareArea.Birth, Color.blue);
            DrawSquares(SquareArea.Transfer, Color.magenta);
            //DrawMonsterBorder();
            //DrawMonsterPoint();
            DrawMonster(Color.yellow);
            DrawCollection(new Color32((byte)225, (byte)201, (byte)150, (byte)255));
            DrawAllShadowAreas(Color.white);
            DrawAllJumpAreas(new Color32((byte)179, (byte)141, (byte)119, (byte)255));
            DrawNpc(new Color32((byte)59, (byte)77, (byte)179, (byte)255));
        }

        private void DrawNpc(Color color)
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    List<GameObject> list = group.NpcList;
                    for (int j = 0; j < list.Count; j++)
                    {
                        DrawSphere(list[j].transform.position, 50, color);
                    }
                }
            }
        }


        private void DrawMonster(Color color)
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    List<NavEditMonsterArea> list = group.monsterAreas;
                    for (int j = 0; j < list.Count; j++)
                    {
                        NavEditMonsterArea monster = list[j];
                        DrawArea(monster.Area, false, color);

                        for (int pi = 0; pi < monster.allPoints.Count; pi++)
                        {
                            DrawSphere(monster.allPoints[pi].transform.position, 50, color);
                        }

                        for (int r = 0; r < list[j].randPoints.Count; r++)
                        {
                            DrawSphere(list[j].randPoints[r], 20, color);
                        }
                        //List<List<Vector2>> tileList = NavEditAreaManager.sInstance.GeneTile(monster.polygon);
                        //if (tileList != null)
                        //{
                        //    foreach (var item in tileList)
                        //    {
                        //        DrawTile(item, color);
                        //    }
                        //} 
                    } 
                }
            }
        }


        private void DrawTile(List<Vector2> tiles, Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < tiles.Count; i++)
            {
                if (i != tiles.Count - 1)
                {
                    if (tiles[i + 1] == null)
                    {
                        Debug.LogError("there a null in the point gameobj lst.");
                        return;
                    }
                    Gizmos.DrawLine(tiles[i], tiles[i + 1]);
                }
                else
                {
                    Gizmos.DrawLine(tiles[i], tiles[0]);
                }
            }
        }


        private void DrawCollection(Color color)
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    List<NavEditMonsterArea> list = group.collectionAreas;
                    for (int j = 0; j < list.Count; j++)
                    {
                        NavEditMonsterArea collection = list[j];
                        DrawArea(collection.Area, false, color);

                        for (int pi = 0; pi < collection.allPoints.Count; pi++)
                        {
                            DrawSphere(collection.allPoints[pi].transform.position, 50, color);
                        }

                        for (int r = 0; r < list[j].randPoints.Count; r++)
                        {
                            DrawSphere(list[j].randPoints[r], 20, color);
                        }
                    }
                }
            }
        }








        private void DrawMonsterPoint(Color color)
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    List<NavEditSquarePoint> list = group.squarePointList;
                    for (int j = 0; j < list.Count; j++)
                    {
                        List<GameObject> points = list[j].points;
                        for (int n = 0; n < points.Count; n++)
                        {
                            DrawSphere(points[n].transform.position, 50, color);
                        }
                    }
                }
            }
        }


        private void DrawMonsterBorder()
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    List<NavEditSquarePoint> list = group.squarePointList;
                    for (int j = 0; j < list.Count; j++)
                    {
                        NavEditSquarePoint point = list[j];
                        DrawSquare(point.Square, Color.yellow);
                    }
                }
            }
        }


        private void DrawSquares(SquareArea square, Color color)
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    List<NavEditSquare> list = group.GetSquareList(square);
                    if (list != null)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            NavEditSquare edit = list[j];
                            DrawSquare(edit, color);
                        }
                    } 
                }
            }
        }

        private void DrawSquare(NavEditSquare square, Color color)
        {
            if (square == null)
                return;
            Gizmos.color = color;

            for (int i = 0; i < square.AllPoints.Count; i++)
            {
                if (i != square.AllPoints.Count - 1)
                {
                    if (square.AllPoints[i + 1] == null)
                    { 
                        Debug.LogError("there a null in the point gameobj lst.");
                        return;
                    }
                    Gizmos.DrawLine(square.AllPoints[i].transform.position, square.AllPoints[i + 1].transform.position);
                } else
                { 
                    Gizmos.DrawLine(square.AllPoints[i].transform.position, square.AllPoints[0].transform.position);
                }
            }
        }

        private void DrawSphere(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(center, radius);
        } 

        //========================== draw area ========================================

        /// <summary>
        /// Draws all areas.
        /// </summary>
        private void DrawAllAreas(Color color)
		{
			if(this.m_bShowArea)
			{
				for( int i = 0 ; i<NavEditAreaManager.sInstance.m_lstAreaGroup.Count ; i++ )
				{
					NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
					bool selectGroup = this.m_iSelGroup == i;
					for(int j = 0 ; j< group.m_lstArea.Count ; j++ )
					{
						NavEditArea area = group.GetArea(j);
						bool selectArea = false;
						if(selectGroup)
							selectArea = this.m_iSelArea == j;
                        if (j == 0 && group.m_cFrameArea != null)
                        {
                            DrawArea(area, selectArea, color);
                        } else
                        {
                            DrawArea(area, selectArea, Color.red);
                        } 
                    }
				}

                //Dictionary<int, List<Polygon>> polygon = NavEditAreaManager.sInstance.GetAreaPolygon();
                //foreach (var item in polygon)
                //{
                //    List<Polygon> list = item.Value;
                //    if (list.Count > 0)
                //    {
                //        List<List<Vector2>> tileList = NavEditAreaManager.sInstance.GeneTile(list[0]);
                //        if (tileList != null)
                //        {
                //            foreach (var tile in tileList)
                //            {
                //                DrawTile(tile, color);
                //            }
                //        }
                //    }
                //}
               
            }
		}

		/// <summary>
		/// Draws the area.
		/// </summary>
		/// <param name="areaid">Areaid.</param>
		private void DrawArea( NavEditArea area , bool selectarea, Color color)
		{	
			if(area == null )
				return;

			List<GameObject> allPoints = area.m_lstPoints;
			if (allPoints.Count <= 0)
				return;

            Gizmos.color = color;

			//if ( selectarea )
			//	Gizmos.color = selectColor;
			//else
			//	Gizmos.color = otherColor;
			
			for (int i = 0; i < allPoints.Count; i++)
			{
				if (allPoints[i] == null)
				{
					NavEditAreaManager.sInstance.CheckAllPoints();
					Debug.LogError("there a null in the point gameobj lst." +i);
					return;
				}
				else
				{
					if (i != allPoints.Count - 1)
					{
						if (allPoints[i + 1] == null)
						{
							NavEditAreaManager.sInstance.CheckAllPoints();
							Debug.LogError("there a null in the point gameobj lst.");
							return;
						}
						Gizmos.DrawLine(allPoints[i].transform.position, allPoints[i + 1].transform.position);
					}
					else
					{
						Gizmos.DrawLine(allPoints[i].transform.position, allPoints[0].transform.position);
					}
				}
			}
		}

//======================= draw editor find triangle ======================================
		/// <summary>
		/// Draws the path triangle.
		/// </summary>
		private void DrawPathTriangle()
		{
			if(m_lstTriPath != null && m_lstTriPath.Count > 0 )
			{
				foreach (NavTriangle tri in m_lstTriPath)
				{
					Gizmos.color = Color.blue;
					
					Vector3 p1 = new Vector3(tri.GetPoint(0).x, tri.GetPoint(0).y, 0);
					Vector3 p2 = new Vector3(tri.GetPoint(1).x, tri.GetPoint(1).y, 0);
					Vector3 p3 = new Vector3(tri.GetPoint(2).x, tri.GetPoint(2).y, 0);
					Gizmos.DrawLine(p1, p2);
					Gizmos.DrawLine(p2, p3);
					Gizmos.DrawLine(p3, p1);
				}
			}
			if(StopTri != null)
			{
				Gizmos.color = Color.red;
				
				Vector3 p1 = new Vector3(StopTri.GetPoint(0).x, StopTri.GetPoint(0).y, 0);
				Vector3 p2 = new Vector3(StopTri.GetPoint(1).x, StopTri.GetPoint(1).y, 0);
				Vector3 p3 = new Vector3(StopTri.GetPoint(2).x, StopTri.GetPoint(2).y, 0);
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p3);
				Gizmos.DrawLine(p3, p1);
			}
		}

//======================= draw NavMesh ======================================

		/// <summary>
		/// Draws the nav mesh.
		/// </summary>
		private void DrawNavMesh()
		{
			if (this.m_bShowMesh)
			{
				if (this.m_lstTriangle.Count != 0)
				{
					foreach (Triangle tri in m_lstTriangle)
					{
						Gizmos.color = Color.black;

						Vector3 p1 = new Vector3(tri.GetPoint(0).x, tri.GetPoint(0).y, 0);
						Vector3 p2 = new Vector3(tri.GetPoint(1).x, tri.GetPoint(1).y, 0);
						Vector3 p3 = new Vector3(tri.GetPoint(2).x, tri.GetPoint(2).y, 0);
						Gizmos.DrawLine(p1, p2);
						Gizmos.DrawLine(p2, p3);
						Gizmos.DrawLine(p3, p1);
					}
				}
			}
		}

        private void DrawAllJumpAreas(Color color)
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    for (int j = 0; j < group.jumpAreas.Count; j++)
                    {
                        NavEditArea area = group.GetJumpArea(j);
                        bool selectArea = false;
                        DrawArea(area, selectArea, color);
                    }
                }
            }
        }


        private void DrawAllShadowAreas(Color color)
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    for (int j = 0; j < group.shadowAreas.Count; j++)
                    {
                        NavEditArea area = group.GetShadowArea(j);
                        bool selectArea = false; 
                        DrawArea(area, selectArea, color);
                    }
                }
            }
        }


        //======================= draw all safe areas ======================================
        private void DrawAllSafeAreas(Color color)
        {
            if (this.m_bShowArea)
            {
                for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                {
                    NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(i);
                    bool selectGroup = this.m_iSelGroup == i;
                    for (int j = 0; j < group.m_safeArea.Count; j++)
                    {
                        NavEditArea area = group.GetSafeArea(j);
                        bool selectArea = false;
                        if (selectGroup)
                            selectArea = this.m_iSelectSafeArea == j;
                        DrawArea(area, selectArea, color);
                    }
                }
            }
        }


        //======================== create navmesh ==========================

        /// <summary>
        /// Gets the un walk areas.
        /// </summary>
        /// <returns>The un walk areas.</returns>
        private List<Polygon> GetUnWalkAreas( List<NavEditArea> lst )
		{
			List<Polygon> areas = new List<Polygon>();
			for (int i = 0; i < lst.Count; i++)
			{
				List<GameObject> allPoints = lst[i].m_lstPoints;
				List<Vector2> allVecPnts = new List<Vector2>();
				for (int j = 0; j < allPoints.Count; j++)
				{
					Vector2 pos = new Vector2(allPoints[j].transform.position.x, allPoints[j].transform.position.y);
					allVecPnts.Add(pos);
				}
				areas.Add(new Polygon(allVecPnts));
			}
			
			return areas;
		}

        /// <summary>
        /// get the safe area point
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private List<Polygon> GetSafeAreas(List<NavEditArea> lst)
        {
            List<Polygon> areas = new List<Polygon>();
            for (int i = 0; i < lst.Count; i++)
            {
                List<GameObject> allPoints = lst[i].m_lstPoints;
                List<Vector2> allVecPnts = new List<Vector2>();
                for (int j = 0; j < allPoints.Count; j++)
                {
                    Vector2 pos = new Vector2(allPoints[j].transform.position.x, allPoints[j].transform.position.y);
                    allVecPnts.Add(pos);
                }
                Polygon poly = new Polygon(allVecPnts);
                areas.Add(poly);
            }

            return areas;
        }

        /// <summary>
        /// 创建导航网格
        /// </summary>
        public void CreateNavMesh()
		{
			Debug.Log("start...");

			List<Triangle> lstTri = new List<Triangle>();
			this.m_lstTriangle.Clear(); 
            this.m_safePolygon.Clear(); 

            int startTriID = 0;
			for( int i = 0 ; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
			{
				NavEditAreaGroup group = NavEditAreaManager.sInstance.m_lstAreaGroup[i];
				lstTri.Clear();
				List<Polygon> areas = GetUnWalkAreas(group.m_lstArea); 
				NavResCode genResult = NavMeshGen.sInstance.CreateNavMesh(areas , ref startTriID , i , ref lstTri );
				foreach( Triangle item in lstTri )
				{
					this.m_lstTriangle.Add(item);
				}
				if (genResult != NavResCode.Success)
					Debug.LogError("faile");

                List<Polygon> safePoly = GetSafeAreas(group.m_safeArea);
                foreach(Polygon poly in safePoly)
                {
                    this.m_safePolygon.Add(poly);
                }
            }

			Debug.Log("triangle count "+ this.m_lstTriangle.Count); 

			Debug.Log("end!");
		}

//========================= save or load navmesh =========================

		/// <summary>
		/// Saves the nav mesh.
		/// </summary>
		/// <param name="path">Path.</param>
		public void SaveNavMesh( string path, SaveFileType type = SaveFileType.String)
		{
			NavResCode code = NavMeshGen.sInstance.SaveNavMeshToFile(path , this.m_lstTriangle, type);
			if( code != NavResCode.Success )
			{
				Debug.LogError( "save navmesh error: " + code.ToString());
			}
		}

		/// <summary>
		/// Loads the nav mesh.
		/// </summary>
		/// <param name="path">Path.</param>
		public void LoadNavMesh( string path )
		{
			List<Triangle> lst;
			NavResCode code = NavMeshGen.sInstance.LoadNavMeshFromFile(path , out lst);
			this.m_lstTriangle = lst;
            for (int i = 0; i < this.m_lstTriangle.Count; i++)
            {
                Debug.LogError(this.m_lstTriangle[i].ToString());
            }
			if(code != NavResCode.Success)
			{
				Debug.LogError("load navmesh error: " + code.ToString());
			}
		}

//=============================== seeker test =================================		
		/// <summary>
		/// Draws the find path.
		/// </summary>
		public void DrawFindPath()
		{
			for( int i = 1 ; i < this.m_lstFindPath.Count ; i++)
			{
				Gizmos.color = Color.blue;
				Vector3 spos = new Vector3(this.m_lstFindPath[i].x , this.m_lstFindPath[i].y, 0);
				Vector3 epos = new Vector3(this.m_lstFindPath[i-1].x , this.m_lstFindPath[i-1].y, 0);
				Gizmos.DrawLine(spos , epos);
			}
		}

		/// <summary>
		/// Seek the specified sPos and ePos.
		/// </summary>
		/// <param name="sPos">S position.</param>
		/// <param name="ePos">E position.</param>
		public void Seek( Vector3 sPos , Vector3 ePos )
		{
			//
			List<NavTriangle> lst = new List<NavTriangle>();
			foreach( Triangle item in this.m_lstTriangle )
			{
				NavTriangle navTri = item.CloneNavTriangle();
				lst.Add(navTri);
				//Debug.Log( "src " + item.GetGroupID() + " :" + item.GetNeighbor(0)+"--" + item.GetNeighbor(1) + "--" + item.GetNeighbor(2) );
				//Debug.Log( "tar " + navTri.GetGroupID() + " :" + navTri.GetNeighbor(0)+"--" + navTri.GetNeighbor(1) + "--" + navTri.GetNeighbor(2) );
			}
			Seeker.instance.NavMeshData = lst;
			List<Vector2> lstpath;
            Vector2 ssPos = new Vector2(sPos.x, sPos.y);
            Vector2 eePos = new Vector2(ePos.x, ePos.y);
            Seeker.instance.Seek(ssPos, eePos, out lstpath, 0);
			//Debug.Log(lstpath.Count + " lstpath");
			this.m_lstFindPath = lstpath;
		}
	}

}