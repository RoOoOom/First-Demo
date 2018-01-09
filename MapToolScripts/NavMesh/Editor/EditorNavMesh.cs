using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Game.NavMesh;
using System.IO;


//	EditorNavMesh.cs
//	Author: Lu  Zexi
//	2014-07-08


namespace Game.NavMesh
{ 
    [CustomEditor(typeof(NavMonoEditor))]
	public class EditorNavMesh : Editor
	{
		private const string UNWALK_EXTENSION = "unwalk";
		private const string NAVMESH_EXTENSION = "navmesh";
		private NavMonoEditor m_cNavMono = null;
		public static EditState m_eState = EditState.StateOther;
		public static GameObject m_cParent = null;
        public static NavEditAreaManager.AreaFrame m_frame = NavEditAreaManager.AreaFrame.Area;

		private static int m_iLastSelGroup = -1;
		private static int m_iLastSelArea = -1;
		private static int m_iLastSelPoint = -1;
        private static int m_iLastSelSafeArea = -1;
        private static int m_iLastSelReliveArea = -1;
        private static int m_iLastSelTransferArea = -1;
        private static int m_iLastSelBirthArea = -1;
        private static int m_iLastSelMonsterArea = -1;
        private static int m_iLastSelMonsterBorderArea = -1;

        private static int m_iLastSelCollectionArea = -1;
        private static int m_iLastSelCollectionBorderArea = -1;

        private static int m_iLastSelShadowArea = -1;
        private static int m_iLastSelShadowPoint = -1;

        private static int m_iLastSelJumpArea = -1;
        private static int m_iLastSelJumpPoint = -1; 

        public enum EditState
		{
			StateEditArea,  //可行走去或阻碍区
			StateFindArea, 
			StateFinishArea,
			StateOther,
            StateEditSafeArea, //安全区
            StateEditFinishSafeArea, 
            StateReliveEditArea, //复活区
            StateReliveFinishArea,
            StateTransEditArea,  //传送区
            StateTransFinishArea,
            StateBirthEditArea, //出生点
            StateBirthFinishArea,
            StateMonsterPointEditArea, //怪物刷新点
            StateMonsterPointFinishArea,
            StateMonsterBorderEditArea, //怪物边界点
            StateMonsterBorderFinishArea,

            StateShadowEditArea,        //阴影区域
            StateShadowFinishArea,
            StateJumpEditArea,   //跳跃区域
            StateJumpFinishArea,

            StateNpcEdit,  //Npc编辑
            StateNpcFinish,

            StateCollectionEditArea, //采集物
            StateCollectionFinishArea,
            StateCollectionPointEditArea,
            StateCollectionPointFinishArea,
        }


		// 焦点转移到编辑窗口
		private void FocusEditPanel()
		{
			if (SceneView.sceneViews.Count > 0)
			{
				SceneView myView = (SceneView)SceneView.sceneViews[0];
				myView.Focus();
			}
		}

		void OnEnable()
		{
			if (m_cNavMono == null)
			{
				if (target)
				{
					m_cNavMono = (NavMonoEditor)target;
				}
				else
					Debug.Log("需要一个脚本对象，必须挂载脚本上面");
			}
			if( m_cParent == null )
			{
				GameObject.DestroyImmediate(GameObject.Find("NavMeshParent"));
				m_cParent = new GameObject("NavMeshParent");
				m_cParent.transform.parent = this.m_cNavMono.transform;
			}
		}

        void OnDisable()
        {
            if (m_eState == EditState.StateEditArea || m_eState == EditState.StateFindArea
                || m_eState == EditState.StateEditSafeArea || m_eState == EditState.StateReliveEditArea
                || m_eState == EditState.StateTransEditArea || m_eState == EditState.StateBirthEditArea
                || m_eState == EditState.StateMonsterPointEditArea || m_eState == EditState.StateMonsterBorderEditArea
                || m_eState == EditState.StateCollectionEditArea || m_eState == EditState.StateCollectionPointEditArea
                || m_eState == EditState.StateJumpEditArea || m_eState == EditState.StateShadowEditArea
                || m_eState == EditState.StateNpcEdit)
			{
				if (this.m_cNavMono.gameObject != null)
				{
					Selection.activeGameObject = this.m_cNavMono.gameObject;
					
					//Debug.Log("请先关闭编辑状态(点击FinishArea按钮)，才能选择别的对象");
				}
			}
		}

		void OnDestroy()
		{
			this.m_cNavMono = null;
		}

		private static Vector3 sPos;
		private static Vector3 ePos;
		private static int m_iIndex = 0;
		void OnSceneGUI()
		{
			if (Event.current == null)
				return;
			
			Event e = Event.current;
			
			if (this.m_cNavMono == null)
				return; 

            // only in edit mode will auto add point with mouse click;
            if (m_eState == EditState.StateEditArea)
			{
                EditArea(e);
			}
			else if( m_eState == EditState.StateFindArea )
			{
				if (e.button == 0 && e.type == EventType.MouseDown)
				{
					Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
					//int layerMask = 1 << 9;
					RaycastHit hit;
					if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
					{
                        if (m_iIndex == 0)
                        {
                            sPos = hit.point;
                            m_iIndex++;
                        }
                        else
                        {
                            m_iIndex = 0;
                            ePos = hit.point;
                            Debug.Log("(" + sPos.x /100 + "," + sPos.y * 2 / 100 + ")" + ", " + "(" + ePos.x / 100 + "," + ePos.y * 2 / 100 + ")");
							this.m_cNavMono.Seek(sPos , ePos);
                            string aa = "";
                            for (int i = 1; i < this.m_cNavMono.m_lstFindPath.Count; i++)
                            {
                                aa += "(" + this.m_cNavMono.m_lstFindPath[i].x / 100 + "," + this.m_cNavMono.m_lstFindPath[i].y * 2 / 100 + ")" + ",";
                            }
                            Debug.Log(aa);
                        }


					}
				}
			} else if(m_eState == EditState.StateEditSafeArea)
            {
                EditSafeArea(e);
            }
            else if (m_eState == EditState.StateReliveEditArea)
            {
                EditSquareArea(e, SquareArea.Relive, "复活点");
            }
            else if (m_eState == EditState.StateBirthEditArea)
            {
                EditSquareArea(e, SquareArea.Birth, "出生点");
            }
            else if (m_eState == EditState.StateTransEditArea)
            {
                EditSquareArea(e, SquareArea.Transfer, "传送点");
            } else if (m_eState == EditState.StateMonsterBorderEditArea)
            {
                EditMonsterArea(e);
               // EditSquarePointBorder(e);
            } else if (m_eState == EditState.StateMonsterPointEditArea)
            {
                EditMonsterAreaPoint(e);
                //EditSquarePointArea(e);
            }
            else if (m_eState == EditState.StateCollectionEditArea)
            {
                EditCollectionArea(e);
                // EditSquarePointBorder(e);
            }
            else if (m_eState == EditState.StateCollectionPointEditArea)
            {
                EditCollectionAreaPoint(e);
                //EditSquarePointArea(e);
            } else if (m_eState == EditState.StateShadowEditArea)
            {
                EditShadowArea(e);
            }
            else if (m_eState == EditState.StateJumpEditArea)
            {
                EditJumpArea(e);
            }
            else if (m_eState == EditState.StateNpcEdit)
            {
                EditNPC(e);
            }
        }

        private void EditArea(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditArea area = group.GetArea(this.m_cNavMono.m_iSelArea);
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point; 
                    // generate obj
                    GameObject point = CreatePoint(placePos, area.parent, NavEditAreaManager.Scale, NavEditAreaManager.NAVMESH_AREA_POINT_NAME);  
                    area.Insert(point, this.m_cNavMono.m_iSelPoint);
                    this.m_cNavMono.m_iSelPoint++;
                    if (this.m_cNavMono.m_iSelPoint >= area.m_lstPoints.Count)
                        this.m_cNavMono.m_iSelPoint = area.m_lstPoints.Count - 1;
                }
                e.Use();
            }
        }

        private void EditSafeArea(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditArea area = group.GetSafeArea(this.m_cNavMono.m_iSelectSafeArea);
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    //placePos.y = pointHeight;

                    // generate obj
                    GameObject point = CreatePoint(placePos, area.parent, NavEditAreaManager.Scale, NavEditAreaManager.SAFE_AREA_POINT_NAME); 
                    area.Insert(point, this.m_cNavMono.m_iSelSafePoint);
                    this.m_cNavMono.m_iSelSafePoint++;
                    if (this.m_cNavMono.m_iSelSafePoint >= area.m_lstPoints.Count)
                        this.m_cNavMono.m_iSelSafePoint = area.m_lstPoints.Count - 1;
                }
                e.Use();
            }
        }

        private void EditShadowArea(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditArea area = group.GetShadowArea(this.m_cNavMono.m_iSelShadowArea);
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    // generate obj
                    GameObject point = CreatePoint(placePos, area.parent, NavEditAreaManager.Scale, NavEditAreaManager.SHADOW_AREA_POINT_NAME);
                    area.Insert(point, this.m_cNavMono.m_iSelShadowPoint);
                    this.m_cNavMono.m_iSelShadowPoint++;
                    if (this.m_cNavMono.m_iSelShadowPoint >= area.m_lstPoints.Count)
                        this.m_cNavMono.m_iSelShadowPoint = area.m_lstPoints.Count - 1;
                }
                e.Use();
            }
        }

        private void EditNPC(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return; 

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    // generate obj
                    GameObject point = CreatePoint(placePos, m_cParent, NavEditAreaManager.Scale, NavEditAreaManager.SHADOW_AREA_POINT_NAME);
                    group.AddNPC(point, this.m_cNavMono.m_iSelNpc);
                    this.m_cNavMono.m_iSelNpc++;
                    if (this.m_cNavMono.m_iSelNpc >= group.NpcList.Count)
                        this.m_cNavMono.m_iSelNpc = group.NpcList.Count - 1;
                }
                e.Use();
            }
        }

        private void EditJumpArea(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditArea area = group.GetJumpArea(this.m_cNavMono.m_iSelJumpArea);
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    // generate obj
                    GameObject point = CreatePoint(placePos, area.parent, NavEditAreaManager.Scale, NavEditAreaManager.JUMP_AREA_POINT_NAME);
                    area.Insert(point, this.m_cNavMono.m_iSelJumpPoint);
                    this.m_cNavMono.m_iSelJumpPoint++;
                    if (this.m_cNavMono.m_iSelJumpPoint >= area.m_lstPoints.Count)
                        this.m_cNavMono.m_iSelJumpPoint = area.m_lstPoints.Count - 1;
                }
                e.Use();
            }
        }

        private void EditMonsterArea(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditMonsterArea area = group.GetMonsterArea(this.m_cNavMono.m_iSelMonsterArea);
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    //placePos.y = pointHeight;

                    // generate obj
                    GameObject point = CreatePoint(placePos, area.Area.parent, NavEditAreaManager.Scale, NavEditAreaManager.MONSTER_AREA_BORDER_NAME);
                    //area.Area.Insert(point, this.m_cNavMono.m_iSelectBorderPoint);
                    area.AddAreaBorder(point, this.m_cNavMono.m_iSelectBorderPoint);
                    this.m_cNavMono.m_iSelectBorderPoint++;
                    if (this.m_cNavMono.m_iSelectBorderPoint >= area.Area.m_lstPoints.Count)
                        this.m_cNavMono.m_iSelectBorderPoint = area.Area.m_lstPoints.Count - 1;
                }
                e.Use();
            }
        }

        private void EditMonsterAreaPoint(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditMonsterArea area = group.GetMonsterArea(this.m_cNavMono.m_iSelMonsterArea);
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    //placePos.y = pointHeight;

                    // generate obj
                    GameObject point = CreatePoint(placePos, area.parent, NavEditAreaManager.Scale, NavEditAreaManager.MONSTER_AREA_POINT_NAME);
                    bool result = area.AddPoint(point, this.m_cNavMono.m_iSelMonsterPoint);
                    if (result)
                    {
                        this.m_cNavMono.m_iSelMonsterPoint++;
                    } else
                    {

                        DestroyImmediate(point);
                    }
                    if (this.m_cNavMono.m_iSelMonsterPoint >= area.allPoints.Count)
                        this.m_cNavMono.m_iSelMonsterPoint = area.allPoints.Count - 1;
                }
                e.Use();
            }
        }


        private void EditCollectionArea(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditMonsterArea area = group.GetCollectionArea(this.m_cNavMono.m_iSelCollectionArea);
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    //placePos.y = pointHeight;

                    // generate obj
                    GameObject point = CreatePoint(placePos, area.Area.parent, NavEditAreaManager.Scale, NavEditAreaManager.COLLECTION_AREA_BORDER_NAME);
                    area.Area.Insert(point, this.m_cNavMono.m_iSelCollectionBorderPoint);
                    this.m_cNavMono.m_iSelCollectionBorderPoint++;
                    if (this.m_cNavMono.m_iSelCollectionBorderPoint >= area.Area.m_lstPoints.Count)
                        this.m_cNavMono.m_iSelCollectionBorderPoint = area.Area.m_lstPoints.Count - 1;
                }
                e.Use();
            }
        }

        private void EditCollectionAreaPoint(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditMonsterArea area = group.GetCollectionArea(this.m_cNavMono.m_iSelCollectionArea);
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    //placePos.y = pointHeight;

                    // generate obj
                    GameObject point = CreatePoint(placePos, area.parent, NavEditAreaManager.Scale, NavEditAreaManager.COLLECTION_AREA_POINT_NAME);
                    bool result = area.AddPoint(point, this.m_cNavMono.m_iSelMonsterPoint);
                    if (result)
                    {
                        this.m_cNavMono.m_iSelMonsterPoint++; 
                    } else
                    { 
                        DestroyImmediate(point);
                    }
                    if (this.m_cNavMono.m_iSelMonsterPoint >= area.allPoints.Count)
                        this.m_cNavMono.m_iSelMonsterPoint = area.allPoints.Count - 1;
                }
                e.Use();
            }
        }



        private void EditSquareArea(Event e, SquareArea square, string name)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditSquare area = null;
                switch (square)
                {
                    case SquareArea.Relive:
                        area = group.GetSquare(square, this.m_cNavMono.m_iSelReliveArea);
                        break;
                    case SquareArea.Birth:
                        area = group.GetSquare(square, this.m_cNavMono.m_iSelBirthArea);
                        break;
                    case SquareArea.Transfer:
                        area = group.GetSquare(square, this.m_cNavMono.m_iSelTransferArea);
                        break;
                    default:
                        break;
                } 
                if (area == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    //placePos.y = pointHeight;

                    // generate obj
                    GameObject point = CreatePoint(placePos, area.parent, NavEditAreaManager.Scale, name); 
                    area.AddPoint(point); 
                   
                    switch (square)
                    {
                        case SquareArea.Relive:
                            this.m_cNavMono.m_iSelRelivePoint++;
                            if (this.m_cNavMono.m_iSelRelivePoint >= 1)
                                this.m_cNavMono.m_iSelRelivePoint = 1;
                            break;
                        case SquareArea.Birth:
                            this.m_cNavMono.m_iSelBirthPoint++;
                            if (this.m_cNavMono.m_iSelBirthPoint >= 1)
                                this.m_cNavMono.m_iSelBirthPoint = 1;
                            break;
                        case SquareArea.Transfer:
                            this.m_cNavMono.m_iSelTransferPoint++;
                            if (this.m_cNavMono.m_iSelTransferPoint >= 1)
                                this.m_cNavMono.m_iSelTransferPoint = 1;
                            break;
                        default:
                            break;
                    } 
                }
                e.Use();
            }
        }

        private void EditSquarePointBorder(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditSquarePoint sp = group.GetSquarePoint(this.m_cNavMono.m_iSelMonsterArea); 
                if (sp == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    //placePos.y = pointHeight;

                    // generate obj
                    GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    point.transform.position = PositionToInt(placePos);
                    point.transform.parent = sp.Parent.transform;
                    //m_cParent.transform;
                    point.transform.localScale *= NavEditAreaManager.Scale;
                    point.name = "point(" + point.transform.position.x + "," + point.transform.position.y + ")";
                    sp.AddBorder(point);

                    this.m_cNavMono.m_iSelMonsterSquare++;
                    if (this.m_cNavMono.m_iSelMonsterSquare >= 1)
                        this.m_cNavMono.m_iSelMonsterSquare = 1; 
                }
                e.Use();
            }
        }


        private void EditSquarePointArea(Event e)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
                if (group == null)
                    return;
                NavEditSquarePoint sp = group.GetSquarePoint(this.m_cNavMono.m_iSelMonsterArea);
                if (sp == null)
                    return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //int layerMask = 1 << 9;
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    Vector3 placePos = hit.point;
                    //placePos.y = pointHeight;

                    // generate obj
                    GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    point.transform.position = PositionToInt(placePos);
                    point.transform.parent = sp.Parent.transform;
                    //m_cParent.transform;
                    point.transform.localScale *= NavEditAreaManager.Scale;
                    point.name = "point(" + point.transform.position.x + "," + point.transform.position.y + ")";
                    sp.AddPoint(point);

                    this.m_cNavMono.m_iSelMonsterPoint++;
                    if (this.m_cNavMono.m_iSelMonsterPoint >= 1)
                        this.m_cNavMono.m_iSelMonsterPoint = 1;
                }
                e.Use();
            }
        }


        private GameObject CreatePoint(Vector3 position, GameObject parent, float Scale, string preName)
        {
            // generate obj
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = PositionToInt(position);
            point.transform.parent = parent.transform;
            //m_cParent.transform;
            point.transform.localScale *= Scale;
            point.name = GetPointName(preName, point);
            return point;
        }

        public static string GetPointName(string preName, GameObject point)
        {
            return preName + "(" + point.transform.position.x + "," + Math.Abs(point.transform.position.y) + ")";
        }

        private Vector3 PositionToInt(Vector3 position)
        {
            return new Vector3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
        }


        private Vector2 groupUIPos;
		private Vector2 areaUIPos;
		private Vector2 pointUIPos;
        private Vector2 safeUIPos;
        private Vector2 safePointUIPos;

        private Vector2 reliveUIPos;
        private Vector2 relivePointUIPos;

        private Vector2 transferUIPos;
        private Vector2 transferPointUIPos;

        private Vector2 birthUIPos;
        private Vector2 birthPointUIPos;

        private Vector2 monsterAreaUIPos;
        private Vector2 monsterPointUIPos;

        private Vector2 collectionAreaUIPos;
        private Vector2 collectionPointUIPos; 

        private Vector2 shadowAreaUIPos;
        private Vector2 shadowAreaPointUIPos;

        private Vector2 jumpAreaUIPos;
        private Vector2 jumpAreaPointUIPos;

        private Vector2 npcIIPos;
        override public void OnInspectorGUI()
		{
			EditorGUILayout.BeginVertical();
			{
				NavEditAreaGroup group = NavEditAreaManager.sInstance.GetGroup(this.m_cNavMono.m_iSelGroup);
				NavEditArea area = null;
                NavEditArea safeArea = null;
                NavEditSquare relive = null;
                NavEditSquare birth = null;
                NavEditSquare transfer = null;
                // NavEditSquarePoint monster = null;
                NavEditMonsterArea monster = null;
                NavEditMonsterArea collection = null;
                NavEditArea shadowArea = null;
                NavEditArea jumpArea = null;

                if ( group != null)
				{
					area = group.GetArea(this.m_cNavMono.m_iSelArea);
                    safeArea = group.GetSafeArea(this.m_cNavMono.m_iSelectSafeArea);
                    relive = group.GetSquare(SquareArea.Relive, this.m_cNavMono.m_iSelReliveArea);
                    birth = group.GetSquare(SquareArea.Birth, this.m_cNavMono.m_iSelBirthArea);
                    transfer = group.GetSquare(SquareArea.Transfer, this.m_cNavMono.m_iSelTransferArea);
                    // monster = group.GetSquarePoint(this.m_cNavMono.m_iSelMonsterArea);
                    monster = group.GetMonsterArea(this.m_cNavMono.m_iSelMonsterArea);
                    collection = group.GetCollectionArea(this.m_cNavMono.m_iSelCollectionArea);
                    shadowArea = group.GetShadowArea(this.m_cNavMono.m_iSelShadowArea);
                    jumpArea = group.GetJumpArea(this.m_cNavMono.m_iSelJumpArea);
                }

                //========================= groups =============================
                ShowGroups();


                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.Area;
                    if (GUILayout.Button(NavEditAreaManager.NAVMESH_AREA_NAME, GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.Area;
                        m_eState = EditState.StateOther;
                    }
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.SafeArea;
                    if (GUILayout.Button("安全区", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.SafeArea;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    }
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.Trans;
                    if (GUILayout.Button("传送点", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.Trans;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.Monster;
                    if (GUILayout.Button("刷怪点", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.Monster;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    }
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.Birth;
                    if (GUILayout.Button("出生点", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.Birth;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    }
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.Relive;
                    if (GUILayout.Button("复活点", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.Relive;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.Shadow;
                    if (GUILayout.Button("阴影区", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.Shadow;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    }
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.Collection;
                    if (GUILayout.Button("采集物", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.Collection;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    }
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.Jump;
                    if (GUILayout.Button("跳跃点", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.Jump;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = m_frame != NavEditAreaManager.AreaFrame.NPC;
                    if (GUILayout.Button("NPC", GUILayout.Height(30)))
                    {
                        m_frame = NavEditAreaManager.AreaFrame.NPC;
                        m_eState = EditState.StateOther;
                        this.m_cNavMono.m_iSelGroup = 0;
                    } 
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();

                switch (m_frame)
                {
                    case NavEditAreaManager.AreaFrame.Area:
                        ShowArea(group);
                        ShowAreaPoint(group, area);
                        break;
                    case NavEditAreaManager.AreaFrame.SafeArea:
                        ShowSafeArea(group);
                        ShowSafeAreaPoint(group, safeArea);
                        break;
                    case NavEditAreaManager.AreaFrame.Monster:
                        //ShowSquarePointArea(group);
                        //ShowSquarePointBundleArea(group, monster);
                        //ShowSquarePointPoint(group, monster);
                        ShowMonsterArea(group);
                        ShowMonsterAreaBorder(group, monster);
                        ShowMonsterAreaPoint(group, monster);
                        break;
                    case NavEditAreaManager.AreaFrame.Relive:
                        ShowSquareArea(group, SquareArea.Relive, NavEditAreaManager.RELIVE_AREA_NAME);
                        ShowSquarePoint(group, relive, SquareArea.Relive, NavEditAreaManager.RELIVE_AREA_POINT_NAME);
                        break;
                    case NavEditAreaManager.AreaFrame.Trans:
                        ShowSquareArea(group, SquareArea.Transfer, NavEditAreaManager.TRANSFORM_AREA_NAME);
                        ShowSquarePoint(group, transfer, SquareArea.Transfer, NavEditAreaManager.TRANSFORM_AREA_POINT_NAME);
                        break;
                    case NavEditAreaManager.AreaFrame.Birth:
                        ShowSquareArea(group, SquareArea.Birth, NavEditAreaManager.BIRTH_AREA_NAME);
                        ShowSquarePoint(group, birth, SquareArea.Birth, NavEditAreaManager.BIRTH_AREA_POINT_NAME);
                        break;
                    case NavEditAreaManager.AreaFrame.Collection:
                        ShowCollectionArea(group);
                        ShowCollectionAreaPoint(group, collection);
                        break;
                    case NavEditAreaManager.AreaFrame.Shadow:
                        ShowShadowArea(group);
                        ShowShadowAreaPoint(group, shadowArea);
                        break;
                    case NavEditAreaManager.AreaFrame.Jump:
                        ShowJumpArea(group);
                        ShowJumpAreaPoint(group, jumpArea);
                        break;
                    case NavEditAreaManager.AreaFrame.NPC:
                        ShowNpc(group);
                        break;
                    default:
                        break;
                }  
            }
            NavmeshOperate();
            EditorGUILayout.EndVertical();

          

        }


        private void ShowGroups()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("All Groups");
                groupUIPos = GUILayout.BeginScrollView(groupUIPos, GUILayout.Height(60));
                {
                    List<string> lst = new List<string>();
                    for (int i = 0; i < NavEditAreaManager.sInstance.m_lstAreaGroup.Count; i++)
                    {
                        lst.Add("Group(" + i.ToString() + ")");
                    }
                    this.m_cNavMono.m_iSelGroup = GUILayout.SelectionGrid(
                        this.m_cNavMono.m_iSelGroup,
                        lst.ToArray(), 1);
                    if (m_iLastSelGroup != this.m_cNavMono.m_iSelGroup)
                    {
                        m_iLastSelGroup = this.m_cNavMono.m_iSelGroup;
                        this.m_cNavMono.m_iSelArea = 0;
                        this.m_cNavMono.m_iSelPoint = 0;
                        this.m_cNavMono.m_iSelectSafeArea = 0;
                        this.m_cNavMono.m_iSelSafePoint = 0;
                        //FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Create Group"))
                    {
                        Debug.Log("create group");
                        NavEditAreaManager.sInstance.AddGroup();
                        this.m_cNavMono.m_iSelGroup = NavEditAreaManager.sInstance.m_lstAreaGroup.Count - 1;
                    }
                    if (GUILayout.Button("Delete Group"))
                    {
                        Debug.Log("delete group");
                        NavEditAreaManager.sInstance.RemoveGroup(this.m_cNavMono.m_iSelGroup);
                        this.m_cNavMono.m_iSelGroup--;
                        if (this.m_cNavMono.m_iSelGroup < 0)
                            this.m_cNavMono.m_iSelGroup = 0;
                        this.m_cNavMono.m_iSelArea = 0;
                        this.m_cNavMono.m_iSelPoint = 0;
                        this.m_cNavMono.m_iSelectSafeArea = 0;
                        this.m_cNavMono.m_iSelSafePoint = 0;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void ShowArea(NavEditAreaGroup group)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.NAVMESH_AREA_NAME);
                areaUIPos = GUILayout.BeginScrollView(areaUIPos, GUILayout.Height(60));
                {
                    if (group != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < group.m_lstArea.Count; i++)
                        {   if(i == 0)
                            {
                                lst.Add(NavEditAreaManager.NAVMESH_AREA_NAME + i.ToString());
                                GameObject go = CreateObjNotDupName(NavEditAreaManager.NAVMESH_AREA_NAME + i, m_cParent.transform);
                                group.m_lstArea[i].parent = go;
                            } else
                            {
                                lst.Add(NavEditAreaManager.NAVMESH_BLOCK_AREA_NAME + i.ToString());
                                GameObject go = CreateObjNotDupName(NavEditAreaManager.NAVMESH_BLOCK_AREA_NAME + i, m_cParent.transform);
                                group.m_lstArea[i].parent = go;
                            }
                           
                        }
                        this.m_cNavMono.m_iSelArea = GUILayout.SelectionGrid(
                            this.m_cNavMono.m_iSelArea,
                            lst.ToArray(), 1);
                        if (m_iLastSelArea != this.m_cNavMono.m_iSelArea)
                        {
                            m_iLastSelArea = this.m_cNavMono.m_iSelArea;
                            this.m_cNavMono.m_iSelPoint = 0;
                        }
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = group != null;
                    if (GUILayout.Button("创建" + NavEditAreaManager.NAVMESH_AREA_NAME))
                    {
                        Debug.Log("create frame area");
                        NavEditArea createArea = group.CreateFrameArea();
                        this.m_cNavMono.m_iSelArea = 0;
                        this.m_cNavMono.m_iSelPoint = 0;

                        GameObject go = CreateObjNotDupName(NavEditAreaManager.NAVMESH_AREA_NAME  + "0", m_cParent.transform); 
                        createArea.parent = go;
                        m_eState = EditState.StateEditArea;
                    }
                    if (GUILayout.Button("创建" + NavEditAreaManager.NAVMESH_BLOCK_AREA_NAME))
                    {
                        Debug.Log("create area");
                        NavEditArea createArea = group.CreateArea();
                        this.m_cNavMono.m_iSelArea = group.m_lstArea.Count - 1;
                        this.m_cNavMono.m_iSelPoint = 0;

                        GameObject go = CreateObjNotDupName(NavEditAreaManager.NAVMESH_BLOCK_AREA_NAME + this.m_cNavMono.m_iSelArea, m_cParent.transform); 
                        createArea.parent = go;
                        m_eState = EditState.StateEditArea;
                    }
                    if (GUILayout.Button("删除" + NavEditAreaManager.NAVMESH_AREA_NAME + "或" + NavEditAreaManager.NAVMESH_BLOCK_AREA_NAME))
                    {
                        Debug.Log("delete area");
                        group.RemoveArea(this.m_cNavMono.m_iSelArea);
                        this.m_cNavMono.m_iSelArea--;
                        if (this.m_cNavMono.m_iSelArea < 0)
                            this.m_cNavMono.m_iSelArea = 0;
                        this.m_cNavMono.m_iSelPoint = 0;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void ShowAreaPoint(NavEditAreaGroup group, NavEditArea area)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("所有网格点");
                pointUIPos = GUILayout.BeginScrollView(pointUIPos, GUILayout.Height(100));
                {
                    if (group != null && area != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < area.m_lstPoints.Count; i++)
                        {
                            if (this.m_cNavMono.m_iSelArea == 0)
                            {
                                lst.Add(GetPointName(NavEditAreaManager.NAVMESH_AREA_POINT_NAME + i, area.m_lstPoints[i]));
                            } else
                            {
                                lst.Add(GetPointName(NavEditAreaManager.NAVMESH_BLOCK_AREA_POINT_NAME + i, area.m_lstPoints[i]));
                            }
                            
                        }
                        this.m_cNavMono.m_iSelPoint = GUILayout.SelectionGrid(
                            this.m_cNavMono.m_iSelPoint,
                            lst.ToArray(), 1);
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_eState != EditState.StateEditArea;
                if (GUILayout.Button("编辑网格点", GUILayout.Height(20)))
                {
                    m_eState = EditState.StateEditArea;
                }
                GUI.enabled = m_eState != EditState.StateFindArea;
                if (GUILayout.Button("查找路径", GUILayout.Height(20)))
                {
                    m_eState = EditState.StateFindArea;
                }
                GUI.enabled = (m_eState == EditState.StateEditArea || m_eState == EditState.StateFindArea);
                if (GUILayout.Button("结束网格点编辑", GUILayout.Height(20)))
                {
                    m_eState = EditState.StateFinishArea;
                }
                if (GUILayout.Button("删除网格点", GUILayout.Height(20)))
                {
                    Debug.Log("delete point");
                    if (area != null)
                    {
                        area.RemoveAt(this.m_cNavMono.m_iSelPoint);
                        this.m_cNavMono.m_iSelPoint--;
                        if (this.m_cNavMono.m_iSelPoint < 0)
                            this.m_cNavMono.m_iSelPoint = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }


        private void ShowSafeArea(NavEditAreaGroup group)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.SAFE_AREA_NAME);
                safeUIPos = GUILayout.BeginScrollView(safeUIPos, GUILayout.Height(60));
                {
                    if (group != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < group.m_safeArea.Count; i++)
                        {
                            lst.Add(NavEditAreaManager.SAFE_AREA_NAME + i.ToString());
                            GameObject go = CreateObjNotDupName(NavEditAreaManager.SAFE_AREA_NAME + i, m_cParent.transform); 
                            group.m_safeArea[i].parent = go;
                        }
                        this.m_cNavMono.m_iSelectSafeArea = GUILayout.SelectionGrid(
                            this.m_cNavMono.m_iSelectSafeArea,
                            lst.ToArray(), 1);
                        if (m_iLastSelSafeArea != this.m_cNavMono.m_iSelectSafeArea)
                        {
                            m_iLastSelSafeArea = this.m_cNavMono.m_iSelectSafeArea;
                            this.m_cNavMono.m_iSelSafePoint = 0;
                        }
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = group != null; 
                    if (GUILayout.Button("创建" + NavEditAreaManager.SAFE_AREA_NAME))
                    {
                        Debug.Log("create safe area");
                        NavEditArea createArea = group.CreateSafeArea();
                        if (group.m_safeArea.Count == 0)
                        {
                            this.m_cNavMono.m_iSelectSafeArea = 0;
                        }
                        else
                        {
                            this.m_cNavMono.m_iSelectSafeArea = group.m_safeArea.Count - 1;
                        } 
                        this.m_cNavMono.m_iSelSafePoint = 0;
                        GameObject go = CreateObjNotDupName(NavEditAreaManager.SAFE_AREA_NAME + this.m_cNavMono.m_iSelectSafeArea, m_cParent.transform); 
                        createArea.parent = go;
                        m_eState = EditState.StateEditSafeArea;
                    }
                    if (GUILayout.Button("删除" + NavEditAreaManager.SAFE_AREA_NAME))
                    {
                        Debug.Log("delete safe area");
                        group.RemoveSafeArea(this.m_cNavMono.m_iSelectSafeArea); 
                        this.m_cNavMono.m_iSelectSafeArea--;
                        if (this.m_cNavMono.m_iSelectSafeArea < 0)
                            this.m_cNavMono.m_iSelectSafeArea = 0;
                        this.m_cNavMono.m_iSelSafePoint = 0; 
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void ShowSafeAreaPoint(NavEditAreaGroup group, NavEditArea area)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.SAFE_AREA_POINT_NAME);
                safePointUIPos = GUILayout.BeginScrollView(safePointUIPos, GUILayout.Height(80));
                {
                    if (group != null && area != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < area.m_lstPoints.Count; i++)
                        { 
                            lst.Add(GetPointName(NavEditAreaManager.SAFE_AREA_POINT_NAME + i, area.m_lstPoints[i]));
                        }
                        this.m_cNavMono.m_iSelSafePoint = GUILayout.SelectionGrid(
                            this.m_cNavMono.m_iSelSafePoint,
                            lst.ToArray(), 1);
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            { 
                GUI.enabled = m_eState != EditState.StateEditSafeArea;
                if (GUILayout.Button("编辑" + NavEditAreaManager.SAFE_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateEditSafeArea;
                } 
                GUI.enabled = (m_eState == EditState.StateEditSafeArea);
                if (GUILayout.Button("结束" + NavEditAreaManager.SAFE_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateEditFinishSafeArea;
                }
                if (GUILayout.Button("删除" + NavEditAreaManager.SAFE_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    Debug.Log("delete safe point");
                    if (area != null)
                    {
                        area.RemoveAt(this.m_cNavMono.m_iSelSafePoint);
                        this.m_cNavMono.m_iSelSafePoint--;
                        if (this.m_cNavMono.m_iSelSafePoint < 0)
                            this.m_cNavMono.m_iSelSafePoint = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }


        private void ShowShadowArea(NavEditAreaGroup group)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.SHADOW_AREA_NAME);
                shadowAreaUIPos = GUILayout.BeginScrollView(shadowAreaUIPos, GUILayout.Height(60));
                {
                    if (group != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < group.shadowAreas.Count; i++)
                        {
                            lst.Add(NavEditAreaManager.SHADOW_AREA_NAME + i.ToString());
                            GameObject go = CreateObjNotDupName(NavEditAreaManager.SHADOW_AREA_NAME + i, m_cParent.transform);
                            group.shadowAreas[i].parent = go;

                        }
                        this.m_cNavMono.m_iSelShadowArea = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelShadowArea, lst.ToArray(), 1);
                        if (m_iLastSelShadowArea != this.m_cNavMono.m_iSelShadowArea)
                        {
                            m_iLastSelShadowArea = this.m_cNavMono.m_iSelShadowArea;
                            this.m_cNavMono.m_iSelShadowPoint = 0;
                        }
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = group != null; 
                    if (GUILayout.Button("创建" + NavEditAreaManager.SHADOW_AREA_NAME))
                    {
                        Debug.Log("create area");
                        NavEditArea createArea = group.CreateShadowArea();
                        this.m_cNavMono.m_iSelShadowArea = group.shadowAreas.Count - 1;
                        this.m_cNavMono.m_iSelShadowPoint = 0;

                        GameObject go = CreateObjNotDupName(NavEditAreaManager.SHADOW_AREA_NAME + this.m_cNavMono.m_iSelShadowArea, m_cParent.transform);
                        createArea.parent = go;
                        m_eState = EditState.StateShadowEditArea;
                    }
                    if (GUILayout.Button("删除" + NavEditAreaManager.SHADOW_AREA_NAME))
                    {
                        Debug.Log("delete area");
                        group.RemoveShadowArea(this.m_cNavMono.m_iSelShadowArea);
                        this.m_cNavMono.m_iSelShadowArea--;
                        if (this.m_cNavMono.m_iSelShadowArea < 0)
                            this.m_cNavMono.m_iSelShadowArea = 0;
                        this.m_cNavMono.m_iSelShadowPoint = 0;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void ShowShadowAreaPoint(NavEditAreaGroup group, NavEditArea area)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.SHADOW_AREA_POINT_NAME);
                shadowAreaPointUIPos = GUILayout.BeginScrollView(shadowAreaPointUIPos, GUILayout.Height(100));
                {
                    if (group != null && area != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < area.m_lstPoints.Count; i++)
                        {
                            lst.Add(GetPointName(NavEditAreaManager.SHADOW_AREA_POINT_NAME + i, area.m_lstPoints[i]));

                        }
                        this.m_cNavMono.m_iSelShadowPoint = GUILayout.SelectionGrid(
                            this.m_cNavMono.m_iSelShadowPoint,
                            lst.ToArray(), 1);
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_eState != EditState.StateShadowEditArea;
                if (GUILayout.Button("编辑" + NavEditAreaManager.SHADOW_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateShadowEditArea;
                } 
                GUI.enabled = (m_eState == EditState.StateShadowEditArea);
                if (GUILayout.Button("结束" + NavEditAreaManager.SHADOW_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateShadowFinishArea;
                }
                if (GUILayout.Button("删除" + NavEditAreaManager.SHADOW_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    if (area != null)
                    {
                        area.RemoveAt(this.m_cNavMono.m_iSelShadowPoint);
                        this.m_cNavMono.m_iSelShadowPoint--;
                        if (this.m_cNavMono.m_iSelShadowPoint < 0)
                            this.m_cNavMono.m_iSelShadowPoint = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }


        private void ShowJumpArea(NavEditAreaGroup group)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.JUMP_AREA_NAME);
                jumpAreaUIPos = GUILayout.BeginScrollView(jumpAreaUIPos, GUILayout.Height(60));
                {
                    if (group != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < group.jumpAreas.Count; i++)
                        {
                            lst.Add(NavEditAreaManager.JUMP_AREA_NAME + i.ToString());
                            GameObject go = CreateObjNotDupName(NavEditAreaManager.JUMP_AREA_NAME + i, m_cParent.transform);
                            group.jumpAreas[i].parent = go;

                        }
                        this.m_cNavMono.m_iSelJumpArea = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelJumpArea, lst.ToArray(), 1);
                        if (m_iLastSelJumpArea != this.m_cNavMono.m_iSelJumpArea)
                        {
                            m_iLastSelJumpArea = this.m_cNavMono.m_iSelJumpArea;
                            this.m_cNavMono.m_iSelJumpPoint = 0;
                        }
                        //FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = group != null;
                    if (GUILayout.Button("创建" + NavEditAreaManager.JUMP_AREA_NAME))
                    {
                        Debug.Log(NavEditAreaManager.JUMP_AREA_NAME);
                        NavEditArea createArea = group.CreateJumpArea();
                        this.m_cNavMono.m_iSelJumpArea = group.jumpAreas.Count - 1;
                        this.m_cNavMono.m_iSelJumpPoint = 0;

                        GameObject go = CreateObjNotDupName(NavEditAreaManager.JUMP_AREA_NAME + this.m_cNavMono.m_iSelJumpArea, m_cParent.transform);
                        createArea.parent = go;
                        m_eState = EditState.StateJumpEditArea;
                    }
                    if (GUILayout.Button("删除" + NavEditAreaManager.JUMP_AREA_NAME))
                    {
                        Debug.Log("delete " + NavEditAreaManager.JUMP_AREA_NAME);
                        group.RemoveJumpArea(this.m_cNavMono.m_iSelJumpArea);
                        this.m_cNavMono.m_iSelJumpArea--;
                        if (this.m_cNavMono.m_iSelJumpArea < 0)
                            this.m_cNavMono.m_iSelJumpArea = 0;
                        this.m_cNavMono.m_iSelJumpPoint = 0;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void ShowJumpAreaPoint(NavEditAreaGroup group, NavEditArea area)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.JUMP_AREA_POINT_NAME);
                jumpAreaPointUIPos = GUILayout.BeginScrollView(jumpAreaPointUIPos, GUILayout.Height(100));
                {
                    if (group != null && area != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < area.m_lstPoints.Count; i++)
                        {
                            lst.Add(GetPointName(NavEditAreaManager.JUMP_AREA_POINT_NAME + i, area.m_lstPoints[i]));
                        }
                        this.m_cNavMono.m_iSelJumpPoint = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelJumpPoint, lst.ToArray(), 1);
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_eState != EditState.StateJumpEditArea;
                if (GUILayout.Button("编辑" + NavEditAreaManager.JUMP_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateJumpEditArea;
                }
                GUI.enabled = (m_eState == EditState.StateJumpEditArea);
                if (GUILayout.Button("结束" + NavEditAreaManager.JUMP_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateJumpFinishArea;
                }
                if (GUILayout.Button("删除" + NavEditAreaManager.JUMP_AREA_POINT_NAME, GUILayout.Height(20)))
                { 
                    if (area != null)
                    {
                        area.RemoveAt(this.m_cNavMono.m_iSelJumpPoint);
                        this.m_cNavMono.m_iSelJumpPoint--;
                        if (this.m_cNavMono.m_iSelJumpPoint < 0)
                            this.m_cNavMono.m_iSelJumpPoint = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }






        private void InitSquareArea(SquareArea area, List<string> list)
        {
            switch (area)
            {
                case SquareArea.Relive:
                    this.m_cNavMono.m_iSelReliveArea = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelReliveArea, list.ToArray(), 1);
                    if (m_iLastSelReliveArea != this.m_cNavMono.m_iSelReliveArea)
                    {
                        m_iLastSelReliveArea = this.m_cNavMono.m_iSelReliveArea;
                        this.m_cNavMono.m_iSelRelivePoint = 0;
                    }
                    break;
                case SquareArea.Birth:
                    this.m_cNavMono.m_iSelBirthArea = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelBirthArea, list.ToArray(), 1);
                    if (m_iLastSelBirthArea != this.m_cNavMono.m_iSelBirthArea)
                    {
                        m_iLastSelBirthArea = this.m_cNavMono.m_iSelBirthArea;
                        this.m_cNavMono.m_iSelBirthPoint = 0;
                    }
                    break;
                case SquareArea.Transfer:
                    this.m_cNavMono.m_iSelTransferArea = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelTransferArea, list.ToArray(), 1);
                    if (m_iLastSelTransferArea != this.m_cNavMono.m_iSelTransferArea)
                    {
                        m_iLastSelTransferArea = this.m_cNavMono.m_iSelTransferArea;
                        this.m_cNavMono.m_iSelTransferPoint = 0;
                    }
                    break;
                default:
                    break;
            } 
        }

        private void ShowSquareArea(NavEditAreaGroup group, SquareArea area, string name)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(name);
                switch (area)
                {
                    case SquareArea.Relive:
                        reliveUIPos = GUILayout.BeginScrollView(reliveUIPos, GUILayout.Height(100));
                        break;
                    case SquareArea.Birth:
                        birthUIPos = GUILayout.BeginScrollView(birthUIPos, GUILayout.Height(100));
                        break;
                    case SquareArea.Transfer:
                        transferUIPos = GUILayout.BeginScrollView(transferUIPos, GUILayout.Height(100));
                        break;
                    default:
                        break;
                }
                
                {
                    if (group != null)
                    {
                        List<string> lst = new List<string>();
                        List<NavEditSquare> list = null;
                        if (group.squareList.TryGetValue(area, out list))
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                lst.Add(name + " Area" + i.ToString() + ")");
                                GameObject go = CreateObjNotDupName(area.ToString() + "Area" + i, m_cParent.transform); 
                                list[i].parent = go;
                            }
                            InitSquareArea(area, lst);
                            //FocusEditPanel();
                        } 
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = group != null;
                    if (GUILayout.Button("创建 " + name))
                    {
                        Debug.Log("create " + name + " area");
                        NavEditSquare createSequare = group.CreateSquare(area);
                        GameObject go = null;
                        switch (area)
                        {
                            case SquareArea.Relive:
                                if (group.squareList[area].Count == 0)
                                {
                                    this.m_cNavMono.m_iSelReliveArea = 0;
                                }
                                else
                                {
                                    this.m_cNavMono.m_iSelReliveArea = group.squareList[area].Count - 1;
                                }
                                this.m_cNavMono.m_iSelRelivePoint = 0;
                                go = CreateObjNotDupName(area.ToString() + "Area" + this.m_cNavMono.m_iSelReliveArea, m_cParent.transform);
                                createSequare.parent = go;
                                break;
                            case SquareArea.Birth:
                                if (group.squareList[area].Count == 0)
                                {
                                    this.m_cNavMono.m_iSelBirthArea = 0;
                                }
                                else
                                {
                                    this.m_cNavMono.m_iSelBirthArea = group.squareList[area].Count - 1;
                                }
                                this.m_cNavMono.m_iSelBirthPoint = 0;

                                go = CreateObjNotDupName(area.ToString() + "Area" + this.m_cNavMono.m_iSelBirthArea, m_cParent.transform); 
                                createSequare.parent = go;
                                break;
                            case SquareArea.Transfer:
                                if (group.squareList[area].Count == 0)
                                {
                                    this.m_cNavMono.m_iSelTransferArea = 0;
                                }
                                else
                                {
                                    this.m_cNavMono.m_iSelTransferArea = group.squareList[area].Count - 1;
                                } 
                                this.m_cNavMono.m_iSelTransferPoint = 0;
                                go = CreateObjNotDupName(area.ToString() + "Area" + this.m_cNavMono.m_iSelTransferArea, m_cParent.transform);
                                createSequare.parent = go;
                                break;
                            default:
                                break;
                        }
                        m_eState = GetSquareEditState(area);

                    }
                    if (GUILayout.Button("删除 " + name))
                    {
                        Debug.Log("delete "  + name + " area");
                        switch (area)
                        {
                            case SquareArea.Relive:
                                group.RemoveSquare(area, this.m_cNavMono.m_iSelReliveArea);
                                this.m_cNavMono.m_iSelReliveArea--;
                                if (this.m_cNavMono.m_iSelReliveArea < 0)
                                    this.m_cNavMono.m_iSelReliveArea = 0;
                                this.m_cNavMono.m_iSelRelivePoint = 0;
                                break;
                            case SquareArea.Birth:
                                group.RemoveSquare(area, this.m_cNavMono.m_iSelBirthArea);
                                this.m_cNavMono.m_iSelBirthArea--;
                                if (this.m_cNavMono.m_iSelBirthArea < 0)
                                    this.m_cNavMono.m_iSelBirthArea = 0;
                                this.m_cNavMono.m_iSelBirthPoint = 0;
                                break;
                            case SquareArea.Transfer:
                                group.RemoveSquare(area, this.m_cNavMono.m_iSelTransferArea);
                                this.m_cNavMono.m_iSelTransferArea--;
                                if (this.m_cNavMono.m_iSelTransferArea < 0)
                                    this.m_cNavMono.m_iSelTransferArea = 0;
                                this.m_cNavMono.m_iSelTransferPoint = 0;
                                break;
                            default:
                                break;
                        } 
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }


        private void ShowSquarePoint(NavEditAreaGroup group, NavEditSquare square, SquareArea area, string name)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(name + "点");
                switch (area)
                {
                    case SquareArea.Relive:
                        relivePointUIPos = GUILayout.BeginScrollView(relivePointUIPos, GUILayout.Height(80));
                        break;
                    case SquareArea.Birth:
                        birthPointUIPos = GUILayout.BeginScrollView(birthPointUIPos, GUILayout.Height(80));
                        break;
                    case SquareArea.Transfer:
                        transferPointUIPos = GUILayout.BeginScrollView(transferPointUIPos, GUILayout.Height(80));
                        break;
                    default:
                        break;
                }
                
                {
                    if (group != null && square != null)
                    {
                        List<string> lst = new List<string>();
                        if (square.StartPoint != null)
                        {
                            lst.Add(GetPointName(name, square.StartPoint));
                        }
                        if (square.EndPoint != null)
                        {
                            lst.Add(GetPointName(name, square.EndPoint));
                        }
                        switch (area)
                        {
                            case SquareArea.Relive:
                                this.m_cNavMono.m_iSelRelivePoint = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelRelivePoint, lst.ToArray(), 1);
                                break;
                            case SquareArea.Birth:
                                this.m_cNavMono.m_iSelBirthPoint = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelBirthPoint, lst.ToArray(), 1);
                                break;
                            case SquareArea.Transfer:
                                this.m_cNavMono.m_iSelTransferPoint = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelTransferPoint, lst.ToArray(), 1);
                                break;
                            default:
                                break;
                        }
                        
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                EditState state = GetSquareEditState(area);
                GUI.enabled = m_eState != state;
                if (GUILayout.Button("编辑" + name, GUILayout.Height(20)))
                {
                    m_eState = state;
                }
                GUI.enabled = (m_eState == state);
                if (GUILayout.Button("结束 " + name, GUILayout.Height(20)))
                {
                    m_eState = GetSquareFinishState(area) ;
                }
                if (GUILayout.Button("删除 " + name + " 点", GUILayout.Height(20)))
                {
                    Debug.Log("delete " + name + " point");
                    if (square != null)
                    {
                        switch (area)
                        {
                            case SquareArea.Relive:
                                square.RemovePoint(this.m_cNavMono.m_iSelRelivePoint);
                                this.m_cNavMono.m_iSelRelivePoint--;
                                if (this.m_cNavMono.m_iSelRelivePoint < 0)
                                    this.m_cNavMono.m_iSelRelivePoint = 0;
                                break;
                            case SquareArea.Birth:
                                square.RemovePoint(this.m_cNavMono.m_iSelBirthPoint);
                                this.m_cNavMono.m_iSelBirthPoint--;
                                if (this.m_cNavMono.m_iSelBirthPoint < 0)
                                    this.m_cNavMono.m_iSelBirthPoint = 0;
                                break;
                            case SquareArea.Transfer:
                                square.RemovePoint(this.m_cNavMono.m_iSelTransferPoint);
                                this.m_cNavMono.m_iSelTransferPoint--;
                                if (this.m_cNavMono.m_iSelTransferPoint < 0)
                                    this.m_cNavMono.m_iSelTransferPoint = 0;
                                break;
                            default:
                                break;
                        }
                       
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }


        private EditState GetSquareEditState(SquareArea area)
        {
            EditState state = EditState.StateOther;
            switch (area)
            {
                case SquareArea.Relive:
                    state = EditState.StateReliveEditArea;
                    break;
                case SquareArea.Birth:
                    state = EditState.StateBirthEditArea;
                    break;
                case SquareArea.Transfer:
                    state = EditState.StateTransEditArea;
                    break;
                default:
                    break;
            }

            return state;
        }

        private EditState GetSquareFinishState(SquareArea area)
        {
            EditState state = EditState.StateOther;
            switch (area)
            {
                case SquareArea.Relive:
                    state = EditState.StateReliveFinishArea;
                    break;
                case SquareArea.Birth:
                    state = EditState.StateBirthFinishArea;
                    break;
                case SquareArea.Transfer:
                    state = EditState.StateTransFinishArea;
                    break;
                default:
                    break;
            }

            return state;
        }



        private void ShowSquarePointArea(NavEditAreaGroup group)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("All Monster");
                monsterAreaUIPos = GUILayout.BeginScrollView(monsterAreaUIPos, GUILayout.Height(100));
                {
                    if (group != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < group.squarePointList.Count; i++)
                        {
                            lst.Add("MonsterArea(" + i.ToString() + ")"); 
                            GameObject go = CreateObjNotDupName("MonsterArea" + i, m_cParent.transform);
                            group.squarePointList[i].Parent = go;
                        }
                        this.m_cNavMono.m_iSelMonsterArea = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelMonsterArea,lst.ToArray(), 1);
                        if (m_iLastSelMonsterArea != this.m_cNavMono.m_iSelMonsterArea)
                        {
                            m_iLastSelMonsterArea = this.m_cNavMono.m_iSelMonsterArea;
                            this.m_cNavMono.m_iSelMonsterPoint = 0;
                        }
                    }
                }
                GUILayout.EndScrollView(); 
            }
            GUILayout.EndVertical();


            GUILayout.BeginHorizontal();
            {
                GUI.enabled = (group != null); 
                if (GUILayout.Button("Create Monster Area"))
                {
                    Debug.Log("create Monster area");
                    NavEditSquarePoint sp = group.CreateSquarePoint();
                    if (group.squarePointList.Count == 0)
                    {
                        this.m_cNavMono.m_iSelMonsterArea = 0;
                    } else
                    {
                        this.m_cNavMono.m_iSelMonsterArea = group.squarePointList.Count - 1;
                    } 
                    this.m_cNavMono.m_iSelMonsterPoint = 0; 
                    GameObject go = CreateObjNotDupName("MonsterArea" + this.m_cNavMono.m_iSelMonsterArea, m_cParent.transform);
                    sp.Parent = go;
                }
                
                if (GUILayout.Button("Delete Monster Area"))
                {
                    Debug.Log("delete Monster area");
                    group.RemoveSquarePoint(this.m_cNavMono.m_iSelMonsterArea);
                    this.m_cNavMono.m_iSelMonsterArea--;
                    if (this.m_cNavMono.m_iSelMonsterArea < 0)
                        this.m_cNavMono.m_iSelMonsterArea = 0;
                    this.m_cNavMono.m_iSelMonsterPoint = 0;
                }
            }
            GUILayout.EndHorizontal(); 
        }


        private void ShowSquarePointBundleArea(NavEditAreaGroup group, NavEditSquarePoint sp)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("刷怪边界点");
                GUILayout.BeginScrollView(areaUIPos, GUILayout.Height(60));
                {
                    if (group != null && sp != null)
                    {
                        List<string> lst = new List<string>();
                        GameObject startPoint = sp.GetStartBorder();
                        if (startPoint != null)
                        {
                            lst.Add(GetPointName(name, startPoint));
                        }
                        GameObject endPoint = sp.GetEndBorder();
                        if (endPoint != null)
                        {
                            lst.Add(GetPointName(name, endPoint));
                        }
                        this.m_cNavMono.m_iSelMonsterSquare = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelMonsterSquare,
                            lst.ToArray(), 1);
                        if (m_iLastSelMonsterBorderArea != this.m_cNavMono.m_iSelMonsterSquare)
                        {
                            m_iLastSelMonsterBorderArea = this.m_cNavMono.m_iSelMonsterSquare;
                            this.m_cNavMono.m_iSelMonsterPoint = 0;
                        }
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();


                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = m_eState != EditState.StateMonsterBorderEditArea;
                    if (GUILayout.Button("Editor Border", GUILayout.Height(20)))
                    {
                        m_eState = EditState.StateMonsterBorderEditArea;
                    } 
                    GUI.enabled = (m_eState == EditState.StateMonsterBorderEditArea);
                    if (GUILayout.Button("Finish Border", GUILayout.Height(20)))
                    {
                        m_eState = EditState.StateMonsterBorderFinishArea;
                    }
                    if (GUILayout.Button("Delete Border", GUILayout.Height(20)))
                    {
                        Debug.Log("delete Border");
                        if (sp != null)
                        {
                            sp.RemoveBorder(this.m_cNavMono.m_iSelMonsterSquare);
                            this.m_cNavMono.m_iSelMonsterSquare--;
                            if (this.m_cNavMono.m_iSelMonsterSquare < 0)
                                this.m_cNavMono.m_iSelMonsterSquare = 0;
                        }
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }


        private void ShowSquarePointPoint(NavEditAreaGroup group, NavEditSquarePoint sp)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("All Monster Points");
                pointUIPos = GUILayout.BeginScrollView(pointUIPos, GUILayout.Height(100));
                {
                    if (group != null && sp != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < sp.points.Count; i++)
                        {
                            lst.Add(name + "Monster Border Point(" + sp.points[i].transform.position.x + "," + sp.points[i].transform.position.y + ")");
                        }
                        this.m_cNavMono.m_iSelMonsterPoint = GUILayout.SelectionGrid(
                            this.m_cNavMono.m_iSelMonsterPoint,
                            lst.ToArray(), 1); 
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_eState != EditState.StateMonsterPointEditArea;
                if (GUILayout.Button("Editor Point", GUILayout.Height(20)))
                {
                    m_eState = EditState.StateMonsterPointEditArea;
                } 
                GUI.enabled = (m_eState == EditState.StateMonsterPointEditArea);
                if (GUILayout.Button("Finish Point", GUILayout.Height(20)))
                {
                    m_eState = EditState.StateMonsterPointFinishArea;
                }
                if (GUILayout.Button("Delete Point", GUILayout.Height(20)))
                {
                    Debug.Log("delete point");
                    if (sp != null)
                    {
                        sp.RemovePoint(this.m_cNavMono.m_iSelMonsterPoint);
                        this.m_cNavMono.m_iSelMonsterPoint--;
                        if (this.m_cNavMono.m_iSelMonsterPoint < 0)
                            this.m_cNavMono.m_iSelMonsterPoint = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }



        private void ShowMonsterArea(NavEditAreaGroup group)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.MONSTER_AREA_NAME);
                monsterAreaUIPos = GUILayout.BeginScrollView(monsterAreaUIPos, GUILayout.Height(60));
                {
                    if (group != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < group.monsterAreas.Count; i++)
                        {
                            lst.Add(NavEditAreaManager.MONSTER_AREA_NAME + "(" + i.ToString() + ")");
                            GameObject go = CreateObjNotDupName(NavEditAreaManager.MONSTER_AREA_NAME + i, m_cParent.transform);
                            group.monsterAreas[i].Area.parent = go;
                            GameObject goBorder = CreateObjNotDupName(NavEditAreaManager.MONSTER_AREA_POINT_NAME + i, go.transform);
                            group.monsterAreas[i].parent = goBorder;

                        }

                        this.m_cNavMono.m_iSelMonsterArea = GUILayout.SelectionGrid(
                            this.m_cNavMono.m_iSelMonsterArea,
                            lst.ToArray(), 1);
                        if (m_iLastSelMonsterArea != this.m_cNavMono.m_iSelMonsterArea)
                        {
                            m_iLastSelMonsterArea = this.m_cNavMono.m_iSelMonsterArea;
                            this.m_cNavMono.m_iSelectBorderPoint = 0;
                            this.m_cNavMono.m_iSelMonsterPoint = 0;
                        }
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = group != null; 
                    if (GUILayout.Button("创建" + NavEditAreaManager.MONSTER_AREA_NAME))
                    {
                        Debug.Log("创建" + NavEditAreaManager.MONSTER_AREA_NAME);
                        NavEditMonsterArea createArea = group.CreateMonsterArea();
                        this.m_cNavMono.m_iSelMonsterArea = group.monsterAreas.Count - 1;
                        this.m_cNavMono.m_iSelectBorderPoint = 0;
                        this.m_cNavMono.m_iSelMonsterPoint = 0;

                        GameObject go = CreateObjNotDupName(NavEditAreaManager.MONSTER_AREA_NAME + this.m_cNavMono.m_iSelMonsterArea, m_cParent.transform);
                        createArea.Area.parent = go;
                         m_eState = EditState.StateMonsterBorderEditArea;
                    }
                    GUI.enabled = m_eState != EditState.StateMonsterBorderEditArea;
                    if (GUILayout.Button("编辑" + NavEditAreaManager.MONSTER_AREA_NAME, GUILayout.Height(20)))
                    {
                        m_eState = EditState.StateMonsterBorderEditArea;
                    }
                    GUI.enabled = (m_eState == EditState.StateMonsterBorderEditArea);
                    if (GUILayout.Button("结束" + NavEditAreaManager.MONSTER_AREA_NAME, GUILayout.Height(20)))
                    {
                        m_eState = EditState.StateMonsterBorderFinishArea;
                    } 
                    if (GUILayout.Button("删除" + NavEditAreaManager.MONSTER_AREA_NAME))
                    { 
                        group.RemoveMonsterArea(this.m_cNavMono.m_iSelMonsterArea);
                        this.m_cNavMono.m_iSelMonsterArea--;
                        if (this.m_cNavMono.m_iSelMonsterArea < 0)
                            this.m_cNavMono.m_iSelMonsterArea = 0;
                        this.m_cNavMono.m_iSelectBorderPoint = 0;
                        this.m_cNavMono.m_iSelMonsterPoint = 0;
                    }
                    GUI.enabled = true;
                    if (GUILayout.Button("生成随机点"))
                    {
                        NavEditMonsterArea monArea = group.GetMonsterArea(this.m_cNavMono.m_iSelMonsterArea);
                        monArea.RandPoint(); 
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }


        private void ShowMonsterAreaBorder(NavEditAreaGroup group, NavEditMonsterArea monsterArea)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.MONSTER_AREA_BORDER_NAME);
                monsterPointUIPos = GUILayout.BeginScrollView(monsterPointUIPos, GUILayout.Height(80));
                {
                    if (group != null && monsterArea != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < monsterArea.Area.m_lstPoints.Count; i++)
                        { 
                            lst.Add(NavEditAreaManager.MONSTER_AREA_BORDER_NAME + "  Point" + i + "(" + monsterArea.Area.m_lstPoints[i].transform.position.x + "," + monsterArea.Area.m_lstPoints[i].transform.position.y + ")" );
                        }
                        this.m_cNavMono.m_iSelectBorderPoint = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelectBorderPoint, lst.ToArray(), 1);
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_eState != EditState.StateMonsterBorderEditArea;
                if (GUILayout.Button("编辑" + NavEditAreaManager.MONSTER_AREA_BORDER_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateMonsterBorderEditArea;
                }

                GUI.enabled = (m_eState == EditState.StateMonsterBorderEditArea);
                if (GUILayout.Button("结束" + NavEditAreaManager.MONSTER_AREA_BORDER_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateMonsterBorderFinishArea;
                }
                if (GUILayout.Button("删除" + NavEditAreaManager.MONSTER_AREA_BORDER_NAME, GUILayout.Height(20)))
                {
                    Debug.Log("delete safe point");
                    if (monsterArea != null)
                    {
                        monsterArea.Area.RemoveAt(this.m_cNavMono.m_iSelectBorderPoint);
                        this.m_cNavMono.m_iSelectBorderPoint--;
                        if (this.m_cNavMono.m_iSelectBorderPoint < 0)
                            this.m_cNavMono.m_iSelectBorderPoint = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }

        private void ShowMonsterAreaPoint(NavEditAreaGroup group, NavEditMonsterArea monsterArea)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.MONSTER_AREA_POINT_NAME);
                monsterPointUIPos = GUILayout.BeginScrollView(monsterPointUIPos, GUILayout.Height(80));
                {
                    if (group != null && monsterArea != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < monsterArea.allPoints.Count; i++)
                        { 
                            string name = GetPointName(NavEditAreaManager.MONSTER_AREA_POINT_NAME + i, monsterArea.allPoints[i]);
                            monsterArea.allPoints[i].name = name;
                            lst.Add(name);
                        }
                        this.m_cNavMono.m_iSelMonsterPoint = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelMonsterPoint, lst.ToArray(), 1);
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_eState != EditState.StateMonsterPointEditArea;
                if (GUILayout.Button("编辑" + NavEditAreaManager.MONSTER_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateMonsterPointEditArea;
                }
                GUI.enabled = (m_eState == EditState.StateMonsterPointEditArea);
                if (GUILayout.Button("结束" + NavEditAreaManager.MONSTER_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateMonsterPointFinishArea;
                }
                if (GUILayout.Button("删除" + NavEditAreaManager.MONSTER_AREA_POINT_NAME, GUILayout.Height(20)))
                { 
                    if (monsterArea != null)
                    {
                        monsterArea.RemovePoint(this.m_cNavMono.m_iSelMonsterPoint);
                        this.m_cNavMono.m_iSelMonsterPoint--;
                        if (this.m_cNavMono.m_iSelMonsterPoint < 0)
                            this.m_cNavMono.m_iSelMonsterPoint = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }



//======================================采集物

        private void ShowCollectionArea(NavEditAreaGroup group)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.COLLECTION_AREA_NAME);
                collectionAreaUIPos = GUILayout.BeginScrollView(collectionAreaUIPos, GUILayout.Height(60));
                {
                    if (group != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < group.collectionAreas.Count; i++)
                        {
                            lst.Add(NavEditAreaManager.COLLECTION_AREA_NAME + "(" + i.ToString() + ")");
                            GameObject go = CreateObjNotDupName(NavEditAreaManager.COLLECTION_AREA_NAME + i, m_cParent.transform);
                            group.collectionAreas[i].Area.parent = go;
                            GameObject goBorder = CreateObjNotDupName(NavEditAreaManager.COLLECTION_AREA_POINT_NAME + i, go.transform);
                            group.collectionAreas[i].parent = goBorder;

                        }

                        this.m_cNavMono.m_iSelCollectionArea = GUILayout.SelectionGrid(
                            this.m_cNavMono.m_iSelCollectionArea,
                            lst.ToArray(), 1);
                        if (m_iLastSelCollectionArea != this.m_cNavMono.m_iSelCollectionArea)
                        {
                            m_iLastSelCollectionArea = this.m_cNavMono.m_iSelCollectionArea;
                            this.m_cNavMono.m_iSelCollectionBorderPoint = 0;
                            this.m_cNavMono.m_iSelCollectionPoint = 0;
                        }
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = group != null;
                    if (GUILayout.Button("创建" + NavEditAreaManager.COLLECTION_AREA_NAME))
                    {
                        Debug.Log("创建" + NavEditAreaManager.COLLECTION_AREA_NAME);
                        NavEditMonsterArea createArea = group.CreateCollectionArea();
                        this.m_cNavMono.m_iSelCollectionArea = group.collectionAreas.Count - 1;
                        this.m_cNavMono.m_iSelCollectionBorderPoint = 0;
                        this.m_cNavMono.m_iSelCollectionPoint = 0;

                        GameObject go = CreateObjNotDupName(NavEditAreaManager.COLLECTION_AREA_NAME + this.m_cNavMono.m_iSelMonsterArea, m_cParent.transform);
                        createArea.Area.parent = go;
                        m_eState = EditState.StateCollectionEditArea;
                    }
                    GUI.enabled = m_eState != EditState.StateCollectionEditArea;
                    if (GUILayout.Button("编辑" + NavEditAreaManager.COLLECTION_AREA_NAME, GUILayout.Height(20)))
                    {
                        m_eState = EditState.StateCollectionEditArea;
                    }
                    GUI.enabled = (m_eState == EditState.StateCollectionEditArea);
                    if (GUILayout.Button("结束" + NavEditAreaManager.COLLECTION_AREA_NAME, GUILayout.Height(20)))
                    {
                        m_eState = EditState.StateCollectionFinishArea;
                    }
                    if (GUILayout.Button("删除" + NavEditAreaManager.COLLECTION_AREA_NAME))
                    {
                        group.RemovCollectionArea(this.m_cNavMono.m_iSelCollectionArea);
                        this.m_cNavMono.m_iSelCollectionArea--;
                        if (this.m_cNavMono.m_iSelCollectionArea < 0)
                            this.m_cNavMono.m_iSelCollectionArea = 0;
                        this.m_cNavMono.m_iSelCollectionArea = 0;
                        this.m_cNavMono.m_iSelCollectionPoint = 0;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void ShowCollectionAreaPoint(NavEditAreaGroup group, NavEditMonsterArea collectionArea)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.COLLECTION_AREA_POINT_NAME);
                collectionPointUIPos = GUILayout.BeginScrollView(collectionPointUIPos, GUILayout.Height(80));
                {
                    if (group != null && collectionArea != null)
                    {
                        List<string> lst = new List<string>();
                        for (int i = 0; i < collectionArea.allPoints.Count; i++)
                        {
                            string name = GetPointName(NavEditAreaManager.COLLECTION_AREA_POINT_NAME + i, collectionArea.allPoints[i]);
                            collectionArea.allPoints[i].name = name;
                            lst.Add(name);
                        }
                        this.m_cNavMono.m_iSelCollectionPoint = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelCollectionPoint, lst.ToArray(), 1);
                        //							FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_eState != EditState.StateCollectionPointEditArea;
                if (GUILayout.Button("编辑" + NavEditAreaManager.COLLECTION_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateCollectionPointEditArea;
                }
                GUI.enabled = (m_eState == EditState.StateCollectionPointEditArea);
                if (GUILayout.Button("结束" + NavEditAreaManager.COLLECTION_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateCollectionPointFinishArea;
                }
                if (GUILayout.Button("删除" + NavEditAreaManager.COLLECTION_AREA_POINT_NAME, GUILayout.Height(20)))
                {
                    if (collectionArea != null)
                    {
                        collectionArea.RemovePoint(this.m_cNavMono.m_iSelCollectionPoint);
                        this.m_cNavMono.m_iSelCollectionPoint--;
                        if (this.m_cNavMono.m_iSelCollectionPoint < 0)
                            this.m_cNavMono.m_iSelCollectionPoint = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }


        private void ShowNpc(NavEditAreaGroup group)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(NavEditAreaManager.NPC_NAME);
                npcIIPos = GUILayout.BeginScrollView(npcIIPos, GUILayout.Height(80));
                {
                    if (group != null)
                    {
                       
                        List<string> lst = new List<string>();
                        for (int i = 0; i < group.NpcList.Count; i++)
                        {
                            string name = GetPointName(NavEditAreaManager.NPC_NAME + i, group.NpcList[i]);
                            group.NpcList[i].name = name;
                            lst.Add(name);
                        }
                        this.m_cNavMono.m_iSelNpc = GUILayout.SelectionGrid(this.m_cNavMono.m_iSelNpc, lst.ToArray(), 1);
                        FocusEditPanel();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            {
                GUI.enabled = m_eState != EditState.StateNpcEdit;
                if (GUILayout.Button("编辑" + NavEditAreaManager.NPC_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateNpcEdit;
                }
                GUI.enabled = (m_eState == EditState.StateNpcEdit);
                if (GUILayout.Button("结束" + NavEditAreaManager.NPC_NAME, GUILayout.Height(20)))
                {
                    m_eState = EditState.StateNpcFinish;
                }
                if (GUILayout.Button("删除" + NavEditAreaManager.NPC_NAME, GUILayout.Height(20)))
                {
                    if (group != null)
                    {
                        group.RemoveNPC(this.m_cNavMono.m_iSelNpc);
                        this.m_cNavMono.m_iSelNpc--;
                        if (this.m_cNavMono.m_iSelNpc < 0)
                            this.m_cNavMono.m_iSelNpc = 0;
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }







        private void NavmeshOperate()
        {
            GUILayout.Label("NavMesh");
            GUILayout.BeginHorizontal();
            {
                this.m_cNavMono.m_bShowMesh = GUILayout.Toggle(this.m_cNavMono.m_bShowMesh, "NavMesh Show");
                this.m_cNavMono.m_bShowArea = GUILayout.Toggle(this.m_cNavMono.m_bShowArea, "Area Show");
            }
            GUILayout.EndHorizontal();
            

            if (GUILayout.Button("Create NavMesh", GUILayout.Height(30)))
            {
                this.m_cNavMono.CreateNavMesh();
            }

            GUILayout.Label("Area Group Save/Load");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save AreaGroup", GUILayout.Height(30)))
                {
                    Debug.Log("save area group");
                    string pathfile = EditorUtility.SaveFilePanel("Save Area Group", Application.dataPath, "map", UNWALK_EXTENSION);
                    NavEditAreaManager.sInstance.SaveAreaGroup(pathfile);
                    EditorUtility.DisplayDialog("提示", "信息保存成功", "OK");
                }
                if (GUILayout.Button("Load AreaGroup", GUILayout.Height(30)))
                {
                    Debug.Log("load area group");
                    string pathfile = EditorUtility.OpenFilePanel("Open Area Group", Application.dataPath, UNWALK_EXTENSION);
                    NavEditAreaManager.sInstance.LoadAreaGroup(pathfile, m_cParent, 360);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("NavMesh Save/Load");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save NavMesh", GUILayout.Height(30)))
                {
                    Debug.Log("save navmesh");
                    string pathfile = EditorUtility.SaveFilePanel("Save NavMesh", Application.dataPath, "map", NAVMESH_EXTENSION);
                    this.m_cNavMono.SaveNavMesh(pathfile, SaveFileType.Binary);
                }
                if (GUILayout.Button("Load NavMesh", GUILayout.Height(30)))
                {
                    Debug.Log("load navmesh");
                    string pathfile = EditorUtility.OpenFilePanel("Open NavMesh", Application.dataPath, NAVMESH_EXTENSION);
                    this.m_cNavMono.LoadNavMesh(pathfile);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Load 3D area group / Save mesh data");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Load", GUILayout.Height(30)))
                {
                    Debug.Log("loading area group and create mesh data");
                    string pathfile = EditorUtility.OpenFilePanel("Open Area Group", Application.dataPath, UNWALK_EXTENSION);
                    NavEditAreaManager.sInstance.LoadAreaGroup(pathfile, m_cParent, 3.6f);
                    this.m_cNavMono.CreateNavMesh();
                    Debug.Log("loaded");
                }
                if (GUILayout.Button("Save", GUILayout.Height(30)))
                {
                    if (ChooseMap.curMapId == 0)
                    {
                        EditorUtility.DisplayDialog("提示", "please load map /Root -> Load Map", "OK");
                    }
                    else
                    {
                        Debug.Log("saving mesh data ");
                        string pathfile = EditorUtility.SaveFilePanel("Save NavMesh", Application.dataPath, "map", NAVMESH_EXTENSION);
                        this.m_cNavMono.SaveNavMesh(pathfile, SaveFileType.String);
                        String dirName = Path.GetDirectoryName(pathfile);
                        if (!Directory.Exists(dirName))
                            Directory.CreateDirectory(dirName);
                        DirectoryInfo info = new DirectoryInfo(dirName); 
                        NavMeshGen.sInstance.WriteClientCommonFile(info, pathfile);
                        EditorUtility.DisplayDialog("提示", "信息保存成功", "OK");
                        Debug.Log("saved");
                    }

                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("导出单个地图数据");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("导出单个地图数据", GUILayout.Height(30)))
                {
                    string pathfile = EditorUtility.OpenFilePanel("请选择地图文件信息地址", Application.dataPath, UNWALK_EXTENSION);
                    if (pathfile == null || pathfile == "")
                    {
                        return;
                    }
                    string server = EditorUtility.OpenFolderPanel("请选择服务器文件保存地址", Application.dataPath, ".erl");
                    if (server == null || server == "")
                    {
                        return;
                    }
                    string client = EditorUtility.OpenFolderPanel("请选择客服端文件保存", Application.dataPath, ".lua");
                    if (client == null || client == "")
                    {
                        return;
                    }
                    
                    if(File.Exists(pathfile))
                    {
                        FileInfo fileinfo = new FileInfo(pathfile);
                        string name = fileinfo.Name;
                        string mapId = name.Split('.')[0];

                        NavEditAreaManager.sInstance.LoadAreaGroup(pathfile, m_cParent, 360f);
                        this.m_cNavMono.CreateNavMesh();
                        string serverPath = server + "//" + mapId + ".erl";
                        NavMeshGen.sInstance.SaveNavMeshToFile(serverPath, mapId, SaveFileType.Binary);

                        NavEditAreaManager.sInstance.LoadAreaGroup(pathfile, m_cParent, 3.6f);
                        this.m_cNavMono.CreateNavMesh();
                        string clientPath = client + "//" + mapId + ".lua";
                        NavMeshGen.sInstance.SaveNavMeshToFile(clientPath, mapId, SaveFileType.String); 

                        string path = client + "//DataMask.lua"; 
                        String dirName = Path.GetDirectoryName(pathfile);
                        if (!Directory.Exists(dirName))
                            Directory.CreateDirectory(dirName);
                        DirectoryInfo info = new DirectoryInfo(dirName); 
                        NavMeshGen.sInstance.WriteClientCommonFile(info, path);
                        EditorUtility.DisplayDialog("提示", "信息保存成功", "OK");
                    }

                   
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.Label("导出所有数据");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("导出", GUILayout.Height(30)))
                {
                    string dir = EditorUtility.OpenFolderPanel("请选择地图信息文件夹", Application.dataPath, UNWALK_EXTENSION);
                    if (dir == null || dir == "")
                    {
                        return;
                    }
                    string server = EditorUtility.OpenFolderPanel("请选择服务器文件保存地址", Application.dataPath, ".erl");
                    if (server == null || server == "")
                    {
                        return;
                    }
                    string client = EditorUtility.OpenFolderPanel("请选择客服端文件保存", Application.dataPath, ".lua");
                    if (client == null || client == "")
                    {
                        return;
                    }
                    DirectoryInfo info = new DirectoryInfo(dir);
                    FileInfo[] files = info.GetFiles();
                    foreach (var file in files)
                    {
                        string name = file.Name;
                        if (name.EndsWith(".unwalk"))
                        {
                            string mapId = name.Split('.')[0];
                            NavEditAreaManager.sInstance.LoadAreaGroup(file.FullName, m_cParent, 3.6f);
                            this.m_cNavMono.CreateNavMesh(); 
                            string clientPath = client + "//" + mapId + ".lua";
                            NavMeshGen.sInstance.SaveNavMeshToFile(clientPath, mapId, SaveFileType.String);

                            NavEditAreaManager.sInstance.LoadAreaGroup(file.FullName, m_cParent, 360f);
                            this.m_cNavMono.CreateNavMesh();
                            string serverPath = server + "//" + mapId + ".erl";
                            NavMeshGen.sInstance.SaveNavMeshToFile(serverPath, mapId, SaveFileType.Binary);
                        } 
                    }
                    string path = client + "//DataMask.lua";
                    NavMeshGen.sInstance.WriteClientCommonFile(info, path);
                    EditorUtility.DisplayDialog("提示", "信息保存成功", "OK"); 
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.Label("重置");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("重置", GUILayout.Height(30)))
                { 
                    NavEditAreaManager.sInstance.Clear();
                    this.m_cNavMono.Clear();
                }
            }
            GUILayout.EndHorizontal();
        } 

        private GameObject CreateObjNotDupName(string name, Transform parent)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null && obj.transform.parent == parent)
            {
                return obj;
            }
            GameObject go = new GameObject();
            go.transform.parent = parent;
            go.name = name;
            return go;
        }

	} 
}


