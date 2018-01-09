/********************************************************************
	created:	2011/12/19
	created:	19:12:2011   13:31
	filename: 	SGWeb\DependProjects\NavMesh\NavMeshGen.cs
	file path:	SGWeb\DependProjects\NavMesh
	file base:	NavMeshGen
	file ext:	cs
	author:		Ivan
	
	purpose:	这个类用来生成导航网格(保存，加载)，必须传入不可行走区域。
                请加载NavMeshTestProject项目来运行单元测试，保证方法的正确
*********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

namespace Game.NavMesh
{
    public sealed class NavMeshGen
    {
        // 使用者必须实现自己的加载数据函数，否则使用默认的C#的文件加载 [1/10/2012 Ivan]
        public delegate UnityEngine.Object GetResources(string fileName);
        // 更改为单例模式
        static readonly NavMeshGen s_cInstance = new NavMeshGen();

        static NavMeshGen() { }

        public static NavMeshGen sInstance
        {
            get
            {
                return s_cInstance;
            }
        }

        // version info
        private const string NAVMESH_VERSION = "SGNAVMESH_01";

        public List<Line2D> allEdges { get; set; }  //所有阻挡区域的边
        public List<Vector2> allPoints { get; set; }//所有顶点列表
		public Line2D startEdge {get;set;}	//start edge.

        public NavMeshGen()
        {
            allEdges = new List<Line2D>();
            allPoints = new List<Vector2>();
        }

        /// <summary>
        /// 保存导航网格信息到文件里面
        /// </summary>
        /// <param name="filePath">文件全路径</param>
        /// <returns></returns>
        public NavResCode SaveNavMeshToFile(string filePath, List<Triangle> navTriangles, SaveFileType type = SaveFileType.String)
        {
            if (navTriangles == null || navTriangles.Count == 0)
                return NavResCode.Failed;

            if (type == SaveFileType.Binary)
            {
               // WriteToBinaryFile(filePath, navTriangles);
                WriteToErlangFile(filePath);
            } else if(type == SaveFileType.String)
            {
                WriteToLuaFile(filePath); 
                //WriteToBinaryFile(filePath, navTriangles);
            }  
            return NavResCode.Success;
        }


        public NavResCode SaveNavMeshToFile(string filePath, string name, SaveFileType type = SaveFileType.String)
        { 
            if (type == SaveFileType.Binary)
            { 
                WriteToErlangFile(filePath, name);
            }
            else if (type == SaveFileType.String)
            {
                WriteToLuaFile(filePath, name);
                //WriteToBinaryFile(filePath, navTriangles);
            }
            return NavResCode.Success;
        }


        //private void WriteToBinaryFile(string filePath, List<Triangle> navTriangles)
        //{
            

        //    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        //        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        //    FileStream fs = File.Create(filePath);
        //    BinaryWriter binWriter = new BinaryWriter(fs);

        //    UTF8Encoding utf8 = new UTF8Encoding();
        //    // save version
        //    binWriter.Append(utf8.GetBytes(NAVMESH_VERSION));
        //    // save triangle count
        //    binbuilder.Append(navTriangles.Count);

        //    // 遍历导航网格并保存
        //    foreach (Triangle navTri in navTriangles)
        //    {
        //        if (navTri != null)
        //        {
        //            //保存网格ID
        //            binbuilder.Append(navTri.GetID());
        //            //保存网格的三角形顶点
        //            for (int i = 0; i < 3; i++)
        //            {
        //                binbuilder.Append(navTri.GetPoint(i).x);
        //                binbuilder.Append(navTri.GetPoint(i).y);
        //            }

        //            // 保存所有邻居边
        //            for (int i = 0; i < 3; i++)
        //            {
        //                binbuilder.Append(navTri.GetNeighbor(i));
        //            }

        //            // 保存每条边中点距离
        //            for (int i = 0; i < 3; i++)
        //            {
        //                binbuilder.Append(navTri.GetWallDis(i));
        //            }

        //            // 保存中心点位置
        //            binbuilder.Append(navTri.GetCenter().x);
        //            binbuilder.Append(navTri.GetCenter().y);

        //            //保存区域id
        //            binbuilder.Append(navTri.GetGroupID());
        //        }
        //    }

        //    // close file
        //    binWriter.Close();
        //    fs.Close();
        //}

        private void WriteToLuaFile(string filePath, string mapId)
        {
            UTF8Encoding utf8 = new UTF8Encoding();

           
            StringBuilder builder = new StringBuilder();

            Config.MapResourceInfoTbl tpl = UnityEditor.AssetDatabase.LoadAssetAtPath<Config.MapResourceInfoTbl>("Assets/Resources/tpl/Config.MapResourceInfoTbl.asset");
            Config.MapResourceInfo info = tpl.data.Find(it => it.maskId == int.Parse(mapId));
            string name = "DataMask" + mapId;
            builder.Append(name + " = {");
            builder.Append("\n"); 
            builder.Append("\trec = { width = " + info.width + ", height = " + info.height * 2 + "},");
            builder.Append("\n");

            builder.Append("\tsmall_rec = { width = " + info.smallWidth + ", height = " + info.smallHeight * 2 + "},");
            builder.Append("\n");

            builder.Append("\n");
            WriteStringArea(builder);
            builder.Append(",");
            builder.Append("\n");
            WriteStringSafeArea(builder);
            builder.Append(",");
            builder.Append("\n");
            WriteStringSquare(builder, SquareArea.Transfer);
            builder.Append(",");
            builder.Append("\n");
            WriteStringSquare(builder, SquareArea.Birth);
            builder.Append(",");
            builder.Append("\n");
            WriteStringSquare(builder, SquareArea.Relive);
            builder.Append(",");
            builder.Append("\n");

            WriteStringMonster("monster", builder , NavEditAreaManager.sInstance.GetMonsterArea());
            builder.Append(",");
            builder.Append("\n");
            
            WriteStringMonster("collection", builder , NavEditAreaManager.sInstance.GetCollectionArea());
            builder.Append(",");
            builder.Append("\n");

            WriteStringMultiArea(builder, "jump_area", NavEditAreaManager.sInstance.GetJumpAreaPolygon());
            builder.Append(",");
            builder.Append("\n");

            WriteStringMultiArea(builder, "shadow_area", NavEditAreaManager.sInstance.GetShadowAreaPolygon()); 
            builder.Append("\n");
            builder.Append("}");
             
            String dirName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName); 
            WriteToFile(dirName + "/" + name + ".lua", builder.ToString());
        }



        private void WriteToLuaFile(string filePath)
        { 
            WriteToLuaFile(filePath, ChooseMap.curMapId.ToString());
        }

        private void WriteStringSafeArea(StringBuilder builder)
        {
            List<Polygon> list = NavEditAreaManager.sInstance.GetSafeAreaPolygon();
            string name = "safe_area";
            WriteStringMultiArea(builder, name, list);
        }

        private void WriteStringMultiArea(StringBuilder builder, string name, List<Polygon> list)
        {
            List<Triangle> lstTri = new List<Triangle>();
            builder.Append("\t" + name + " = {");
            builder.Append("\n");  
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append("\t\t");
                builder.Append("[" + i + "] = {");
                builder.Append("\n");
                int flag = 0;
                List<Vector2> allPoints = list[i].GetPoints();

                // 存储border
                builder.Append("\t\t\t");
                builder.Append("border = {");
                builder.Append("\n");
                foreach (var item in allPoints)
                {
                    if (flag != 0)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                    flag++;
                    builder.Append("\t\t\t\t");
                    builder.Append("{x = " + item.x + ", y = " + item.y * 2+ "}");
                }
                builder.Append("\n");
                builder.Append("\t\t\t},");
                builder.Append("\n");

                //save triangle
                List<Triangle> triList = NavEditAreaManager.sInstance.GeneTriangle(list[i]);
                builder.Append("\t\t\t");
                builder.Append("triangle = {");
                builder.Append("\n");
                for (int tri = 0; tri < triList.Count; tri++)
                {
                    builder.Append("\t\t\t\t");
                    WriteTriangle(builder, triList[tri]);
                    if (tri != triList.Count - 1)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                }
                builder.Append("\n");
                builder.Append("\t\t\t},");

                builder.Append("\n\t\t\tcenter = {");
                //Vector2 center = CalCenterByOutsideSquare(allPoints);
                //Vector2 center = CalCenterByTriangle(triList);
                Vector2 center = CalCenterByAvager(allPoints);
                builder.Append(" x = " + center.x + " , y = " + center.y);
                builder.Append(" }");

                builder.Append("\n");
                builder.Append("\t\t}");
                if (i != list.Count - 1)
                {
                    builder.Append(",");
                    builder.Append("\n");
                }
            }
            builder.Append("\n");
            builder.Append("\t}");
        }

        private void WriteStringArea(StringBuilder builder)
        {
            List<Triangle> list = NavEditAreaManager.sInstance.GetAreaTriangle();
            string name = "area";
            builder.Append("\t" + name + " = {");
            builder.Append("\n");
            int flag = 0;
            foreach (var item in list)
            {
                if (flag != 0)
                {
                    builder.Append(",");
                    builder.Append("\n");
                }
                flag++;
                builder.Append("\t\t"); 
                WriteTriangle(builder, item); 
            }
            builder.Append("\n\t}");
        } 

        private void WriteStringMonster(string name, StringBuilder builder , List<NavEditMonsterArea> list) 
        {
            name = "\t " + name + " = {\n";
            builder.Append(name);
            for (int i = 0; i < list.Count; i++)
            {
                NavEditMonsterArea item = list[i];
                builder.Append("\t\t");
                builder.Append("[" + i + "] = {");
                builder.Append("\n");
                builder.Append("\t\t\ttrianles = {");
                builder.Append("\n");
                int triFlag = 0;
                foreach (var tri in item.m_lstTriangle)
                {
                    if (triFlag != 0)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                    triFlag++;
                    builder.Append("\t\t\t\t");
                    WriteTriangle(builder, tri); 
                }
                builder.Append("\n");
                builder.Append("\t\t\t},");
                builder.Append("\n");

                Polygon polygon = list[i].polygon; 
                // 存储border
                builder.Append("\t\t\t");
                builder.Append("border = {");
                builder.Append("\n");
                int poiFlag = 0;
                if (polygon != null)
                {
                    List<Vector2> allPoints = polygon.GetPoints();
                    foreach (var poi in allPoints)
                    {
                        if (poiFlag != 0)
                        {
                            builder.Append(",");
                            builder.Append("\n");
                        }
                        poiFlag++;
                        builder.Append("\t\t\t\t");
                        builder.Append("{x = " + poi.x + ", y = " + poi.y * 2 + "}");
                    }
                } 
                builder.Append("\n");
                builder.Append("\t\t\t},");
                builder.Append("\n");



                builder.Append("\t\t\tpoints = {");
                builder.Append("\n");
                int flag = 0;
                foreach (var point in item.allPoints)
                {
                    if (flag != 0)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                    flag++;
                    builder.Append("\t\t\t\t");
                    Vector3 position = point.transform.position;
                    builder.Append("{ x = " + position.x + ", y = " + position.y * 2 + " }");
                }
                builder.Append("\n");
                builder.Append("\t\t\t}");
                builder.Append("\n");
                builder.Append("\t\t}");
                if (i != list.Count - 1)
                {
                    builder.Append(",");
                    builder.Append("\n");
                }
            }
            builder.Append("\n\t}");
        }


        private void WriteStringSquare(StringBuilder builder, SquareArea area)
        {
            string name = area.ToString().ToLower();
            builder.Append("\t" + name + " = {");
            builder.Append("\n");
            List<NavEditSquare> list = NavEditAreaManager.sInstance.GetSquare(area);
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append("\t\t");
                Vector3 pos = list[i].StartPoint.transform.position;
                Vector3 endPos = list[i].EndPoint.transform.position;
                builder.Append("[" + i + "] = {");
                builder.Append("start_point = { x = " + pos.x + ", y = " + pos.y * 2 + " }, ");
                builder.Append("end_point = { x = " + endPos.x + ", y = " + endPos.y * 2 + " },");
                builder.Append("center = { x = " + ((pos.x + endPos.x)/2) + ", y = " + ((pos.y*2 + endPos.y*2)/2) + "}");
                builder.Append("}");
                if (i != list.Count - 1)
                {
                    builder.Append(",");
                    builder.Append("\n");
                } 
            }
            builder.Append("\n");
            builder.Append("\t}");
        }

        private void StringWriteNpc(StringBuilder builder)
        {
            List<Vector3> list = NavEditAreaManager.sInstance.GetNpcPostionList();
            builder.Append("\tnpc = {");
            builder.Append("\n");
            int flag = 0;
            foreach (var item in list)
            {
                if (flag != 0)
                {
                    builder.Append(",");
                    builder.Append("\n");
                }
                flag++;
                builder.Append("{ x = " + item.x + ", y = " + item.y * 2 + " }"); 
            }
            builder.Append("\n");
            builder.Append("\t}");
        }




        private void WriteTriangle(StringBuilder builder, Triangle navTri)
        {
            if (navTri != null)
            {
                //保存网格ID
                builder.Append("{nav_id = ");
                builder.Append(navTri.GetID());
                builder.Append(",");

                //保存网格的三角形顶点
                builder.Append("tris = {");
                for (int i = 0; i < 3; i++)
                {
                    builder.Append("{x = " + navTri.GetPoint(i).x + ", ");
                    builder.Append("y = " + navTri.GetPoint(i).y * 2 + "}");
                    if (i != 2)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("}, ");

                // 保存所有邻居边
                builder.Append("neibor = {");
                for (int i = 0; i < 3; i++)
                {
                    builder.Append(navTri.GetNeighbor(i));
                    if (i != 2)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("}, ");

                // 保存每条边中点距离
                builder.Append("distance = {");
                for (int i = 0; i < 3; i++)
                {
                    builder.Append(navTri.GetWallDis(i));
                    if (i != 2)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("}, ");

                // 保存中心点位置
                builder.Append("center = {");
                builder.Append("x = " + navTri.GetCenter().x + ", ");
                builder.Append("y = " + navTri.GetCenter().y * 2 + "}, ");
                //保存区域id
                builder.Append("group_id = " + navTri.GetGroupID());
                builder.Append("}");
            }
        }

        private void WriteToErlangFile(string filePath, string mapId)
        {
            string includeName = WriteToErlangIncludeFile(filePath);
            string name = "data_mask_" + mapId;
           

            StringBuilder builder = new StringBuilder();

            builder.Append("-module(" + name + ").");
            builder.Append("\n");
            builder.Append("-include(\"" + includeName + "\").");
            builder.Append("\n");
            builder.Append("-export([get_rec/0, get_small_rec/0, get_area/0, get_area/1, get_safe_area/0, get_transfer/0, get_birth/0, get_relive/0, get_monster/0, get_collection/0, get_jump_area/0, get_shadow_area/0]).");
            builder.Append("\n");


            Config.MapResourceInfoTbl tpl = UnityEditor.AssetDatabase.LoadAssetAtPath<Config.MapResourceInfoTbl>("Assets/Resources/tpl/Config.MapResourceInfoTbl.asset");
            // Config.MapResourceInfoTbl tpl = go.GetComponent<Config.MapResourceInfoTbl>();
            Config.MapResourceInfo info = tpl.data.Find(it => it.maskId == int.Parse(mapId));
            builder.Append("\n");
            builder.Append("get_rec()->");
            builder.Append("\n"); 
            builder.Append("\t{" + info.width + "," + info.height * 2 + "}.");
            builder.Append("\n");

            builder.Append("get_small_rec()->");
            builder.Append("\n");
            builder.Append("\t{" + info.smallWidth + "," + info.smallHeight + "}.");
            builder.Append("\n");

            builder.Append("\n");
            WriteErlangArea(builder);
            builder.Append("\n");
            WriteErlangMultiArea(builder, "safe_area", NavEditAreaManager.sInstance.GetSafeAreaPolygon());
            builder.Append("\n");
            WriteErlangSquare(builder, SquareArea.Transfer);
            builder.Append("\n");
            WriteErlangSquare(builder, SquareArea.Birth);
            builder.Append("\n");
            WriteErlangSquare(builder, SquareArea.Relive);
            builder.Append("\n");

            WriteErlangMonster(builder, "monster", NavEditAreaManager.sInstance.GetMonsterArea());
            builder.Append("\n");

            WriteErlangMonster(builder, "collection", NavEditAreaManager.sInstance.GetCollectionArea());
            builder.Append("\n");

            WriteErlangMultiArea(builder, "jump_area", NavEditAreaManager.sInstance.GetJumpAreaPolygon());
            builder.Append("\n");

            WriteErlangMultiArea(builder, "shadow_area", NavEditAreaManager.sInstance.GetShadowAreaPolygon());

            string dirName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName); 
            WriteToFile(dirName + "/" + name + ".erl", builder.ToString());
        }

        private void WriteToErlangFile(string filePath)
        {
            WriteToErlangFile(filePath, ChooseMap.curMapId.ToString());
        }

        private string WriteToErlangIncludeFile(string filePath)
        {
            UTF8Encoding utf8 = new UTF8Encoding();

            string name = "map_mask.hrl";
            
            //StringBuilder builder = new StreamWriter(fs, Encoding.UTF8); 

            //"-record(cfg_boss_night_point,{type,point,left,right})."
            StringBuilder builder = new StringBuilder(); 
            builder.Append("-record(nav_triangle, {nav_id, tris, neibor, distance, center, group_id, collider}).");
            builder.Append("\n");
            builder.Append("-record(area, {area_id, border, tris, center}).");
            builder.Append("\n");
            builder.Append("-record(nav_area, {border, tris}).");
            builder.Append("\n");
            builder.Append("-record(monster_area, {area_id, border, tris, points}).");

            String dirName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            WriteToFile(dirName + "/" + name, builder.ToString());
            return name;
        }


        private void WriteErlangMultiArea(StringBuilder builder, string name, List<Polygon> list)
        {
            builder.Append("get_" + name + "()->");
            builder.Append("\n");

            builder.Append("\t["); 
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append("\n");
                builder.Append("\t\t#area{\n\t\t\tarea_id = " + i + ", ");

                builder.Append("\n");
                builder.Append("\t\t\tborder = [");
                int flag = 0;
                List<Vector2> allPoints = list[i].GetPoints();
                foreach (var position in allPoints)
                {
                    if (flag != 0)
                    {
                        builder.Append(",");
                    }
                    flag++;
                    builder.Append("{" + Mathf.CeilToInt(position.x) + ", " + Mathf.CeilToInt(position.y) * 2 + "}");
                }
                builder.Append("], ");

                builder.Append("\n");
                builder.Append("\t\t\ttris = [");
                List<Triangle> triList = NavEditAreaManager.sInstance.GeneTriangle(list[i]);
                builder.Append("\n");
                for (int tri = 0; tri < triList.Count; tri++)
                {
                    builder.Append("\t\t\t\t");
                    WriteErlangTriangle(builder, triList[tri]);
                    if (tri != triList.Count - 1)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                }
                builder.Append("\n");
                builder.Append("\t\t\t],");

                builder.Append("\n\t\t\tcenter = [{");
                Vector2 center = CalCenterByOutsideSquare(allPoints);
                builder.Append(center.x + ", " + center.y);
                builder.Append("}]");

                builder.Append("\n");
                if (i != list.Count - 1)
                {
                    builder.Append("\t\t},");
                    builder.Append("\n");
                } else
                {
                    builder.Append("\t\t}"); 
                }
            }
            builder.Append("\n");
            builder.Append("\t].");
        }

        private void WriteErlangPolygon(StringBuilder builder, Polygon poly)
        {
            int polyFlag = 0;
            List<Vector2> allPolyPoints = poly.GetPoints();
            builder.Append("[");
            foreach (var tri in allPolyPoints)
            {
                if (polyFlag != 0)
                {
                    builder.Append(",");
                }
                polyFlag++;
                builder.Append("{" + Mathf.CeilToInt(tri.x) + ", " + Mathf.CeilToInt(tri.y) * 2 + "}");
            }
            builder.Append("]");
        }

        private void WriteErlangArea(StringBuilder builder)
        {
            List<Triangle> list = NavEditAreaManager.sInstance.GetAreaTriangle();
            Dictionary<int, List<Polygon>> polygon = NavEditAreaManager.sInstance.GetAreaPolygon();
            string name = "get_area()->";
            builder.Append(name);
            builder.Append("\n");
            builder.Append("\t#nav_area{");
            builder.Append("\n");
            builder.Append("\t\tborder=[");
            builder.Append("\n");
            int pFlag = 0;
            foreach (var item in polygon)
            {
                if (pFlag != 0)
                {
                    builder.AppendLine(","); 
                }
                pFlag++;
                List<Polygon> polys = item.Value;
                int count = polys.Count;
                if (count == 0)
                {
                    builder.Append("\t\t\t{[], []}");
                } else if (count == 1)
                {
                    builder.Append("\t\t\t{");
                    WriteErlangPolygon(builder, polys[0]);
                    builder.Append(", []}");
                } else
                {
                    builder.Append("\t\t\t{");
                    for (int i = 0; i < count; i++)
                    {
                        Polygon gon = polys[i];
                        if (i == 0)
                        {
                            WriteErlangPolygon(builder, gon);
                            builder.Append(", ");
                        }
                        else
                        {
                            if (i == 1)
                            {
                                builder.Append("["); 
                            }
                            WriteErlangPolygon(builder, gon);
                            if (i == count - 1)
                            {
                                builder.Append("]");
                            } else
                            {
                                builder.Append(", ");
                            }
                        } 
                    }
                    builder.Append("}");
                } 
            }
            builder.Append("\n");
            builder.Append("\t\t],");
            builder.Append("\n");

            int flag = 0;
            builder.Append("\t\ttris=["); 
            builder.Append("\n");  
            foreach (var item in list)
            {
                if (flag != 0)
                {
                    builder.Append(",");
                    builder.Append("\n");
                }
                flag++;
                builder.Append("\t\t\t");
                WriteErlangTriangle(builder, item);
            }
            builder.Append("\n");
            builder.Append("\t\t]");
            builder.Append("\n");
            builder.Append("\t}.");
            builder.Append("\n");

            WriteErlangNavArea(builder);

        }

        private void WriteErlangNavArea(StringBuilder builder)
        {
            Dictionary<int, List<Triangle>> dic = NavEditAreaManager.sInstance.GetGroupAreaTriangle();
            Dictionary<int, List<Polygon>> polygon = NavEditAreaManager.sInstance.GetAreaPolygon();
            
            for (int i = 0; i < dic.Count; i++)
            {
                builder.Append("get_area(" + i + ")->");
                builder.Append("\n");
                builder.Append("\t#nav_area{");
                builder.Append("\n");
                builder.Append("\t\tborder=[");
                builder.Append("\n");
                
                List<Polygon> polys = polygon[i];
                int count = polys.Count;
                if (count == 0)
                {
                    builder.Append("\t\t\t{[], []}");
                }
                else if (count == 1)
                {
                    builder.Append("\t\t\t{");
                    WriteErlangPolygon(builder, polys[0]);
                    builder.Append(", []}");
                }
                else
                {
                    builder.Append("\t\t\t{");
                    for (int j = 0; j < count; j++)
                    {
                        Polygon gon = polys[j];
                        if (j == 0)
                        {
                            WriteErlangPolygon(builder, gon);
                            builder.Append(", ");
                        }
                        else
                        {
                            if (j == 1)
                            {
                                builder.Append("[");
                            }
                            WriteErlangPolygon(builder, gon);
                            if (j == count - 1)
                            {
                                builder.Append("]");
                            }
                            else
                            {
                                builder.Append(", ");
                            }
                        }
                    }
                    builder.Append("}");
                }

                builder.Append("\n");
                builder.Append("\t\t],");

                builder.Append("\n");
                builder.Append("\ttris=[");
                builder.Append("\n");
                int flag = 0;
                flag = 0;
                foreach (var item in dic[i])
                {
                    if (flag != 0)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                    flag++;
                    builder.Append("\t\t");
                    WriteErlangTriangle(builder, item);
                }
                builder.Append("\n");
                builder.Append("\t\t]");
                builder.Append("\n");
                if (i != dic.Count - 1)
                {
                    builder.Append("\t};");
                    builder.Append("\n");
                }
                else
                {
                    builder.Append("\t}.");
                } 
            }
        }


        private void WriteErlangMonster(StringBuilder builder, string name, List<NavEditMonsterArea> list)
        { 
            builder.Append("get_" + name + "()->\n");
            builder.Append("\t[");
            builder.Append("\n");
            for (int i = 0; i < list.Count; i++)
            {
                NavEditMonsterArea item = list[i];
                builder.Append("\t\t");
                builder.Append("#monster_area{");
                builder.Append("\n");
                builder.Append("\t\t\tarea_id = " + i + ",");

                Polygon poly = list[i].polygon;
                

                builder.Append("\n");
                builder.Append("\t\t\tborder = [");
                if (poly != null)
                {
                    int polyFlag = 0;
                    List<Vector2> allPolyPoints = poly.GetPoints();
                    foreach (var tri in allPolyPoints)
                    {
                        if (polyFlag != 0)
                        {
                            builder.Append(",");
                        }
                        polyFlag++;
                        builder.Append("{" + Mathf.CeilToInt(tri.x) +  ", " + Mathf.CeilToInt(tri.y) * 2 +  "}");
                    }
                } 
                builder.Append("],");
                builder.Append("\n");



                builder.Append("\n");
                builder.Append("\t\t\ttris = [");
                builder.Append("\n");
                int triFlag = 0;
                foreach (var tri in item.m_lstTriangle)
                {
                    if (triFlag != 0)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                    triFlag++;
                    builder.Append("\t\t\t\t");
                    WriteErlangTriangle(builder, tri); 
                }
                builder.Append("\n");
                builder.Append("\t\t\t],");
                builder.Append("\n");


                builder.Append("\t\t\tpoints = [");
                builder.Append("\n");
                int flag = 0;
                foreach (var point in item.allPoints)
                {
                    if (flag != 0)
                    {
                        builder.Append(",");
                        builder.Append("\n");
                    }
                    flag++;
                    builder.Append("\t\t\t\t");
                    Vector3 position = point.transform.position;
                    builder.Append("{" + Mathf.CeilToInt(position.x) + ", " + Mathf.CeilToInt(position.y) * 2 + "}");
                }
                builder.Append("\n");
                builder.Append("\t\t\t]");
                builder.Append("\n");
                builder.Append("\t\t}");
                if (i != list.Count - 1)
                {
                    builder.Append(",");
                    builder.Append("\n");
                }
            }
            builder.Append("\n");
            builder.Append("\t].");
        }

        private void WriteErlangSquare(StringBuilder builder, SquareArea area)
        {
            string name = area.ToString().ToLower();
            builder.Append("get_" + name + "()->");
            builder.Append("\n");
            builder.Append("\t[");
            List<NavEditSquare> list = NavEditAreaManager.sInstance.GetSquare(area);
            for (int i = 0; i < list.Count; i++)
            { 
                Vector3 pos = list[i].StartPoint.transform.position;
                Vector3 endPos = list[i].EndPoint.transform.position;
                builder.Append("{");
                builder.Append("{" + Mathf.CeilToInt(pos.x) + ", " + Mathf.CeilToInt(pos.y) * 2 + "}, ");
                builder.Append("{" + Mathf.CeilToInt(endPos.x) + ", " + Mathf.CeilToInt(endPos.y) * 2 + "}, ");
                builder.Append("{" + Mathf.CeilToInt((pos.x + endPos.x)/2) + ", " + Mathf.CeilToInt((pos.y + endPos.y)/2) * 2 + "}");
                builder.Append("}");
                if (i != list.Count - 1)
                {
                    builder.Append(",");
                    builder.Append("\n");
                }
            }
            builder.Append("].");
        }

        private void WriteErlangNpc(StringBuilder builder)
        {
            List<Vector3> list = NavEditAreaManager.sInstance.GetNpcPostionList();
            builder.Append("get_npc()->");
            builder.Append("\n");
            builder.Append("\t[");
            builder.Append("\n");
            int flag = 0;
            foreach (var item in list)
            {
                if (flag != 0)
                {
                    builder.Append(",");
                    builder.Append("\n");
                }
                flag++;
                builder.Append("{" + Mathf.CeilToInt(item.x) + ", " + Mathf.CeilToInt(item.y) + " }");
            }
            builder.Append("\n");
            builder.Append("\t].");
        }
         


        private void WriteErlangTriangle(StringBuilder builder, Triangle navTri)
        {
            if (navTri != null)
            {
                //保存网格ID
                builder.Append("#nav_triangle{nav_id = ");
                builder.Append(navTri.GetID());
                builder.Append(",");

                List<Vector2> vecPoints = new List<Vector2>();

                //保存网格的三角形顶点
                builder.Append("tris = [");
                for (int i = 0; i < 3; i++)
                {
                    vecPoints.Add(new Vector2(Mathf.CeilToInt(navTri.GetPoint(i).x), Mathf.CeilToInt(navTri.GetPoint(i).y) * 2));
                    builder.Append("{" + Mathf.CeilToInt(navTri.GetPoint(i).x) + ", " + Mathf.CeilToInt(navTri.GetPoint(i).y) * 2 + "}");
                    if (i != 2)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("], ");

                // 保存所有邻居边
                builder.Append("neibor = [");
                for (int i = 0; i < 3; i++)
                {
                    builder.Append(navTri.GetNeighbor(i));
                    if (i != 2)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("], ");


                double[] walldis = navTri.CalcWallDistance(vecPoints.ToArray());

                // 保存每条边中点距离
                builder.Append("distance = [");
                for (int i = 0; i < walldis.Length; i++)
                {
                    builder.Append(Math.Round(walldis[i], 4) );
                    if (i != 2)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("], ");


                Rect rect = navTri.CalcCollider(vecPoints.ToArray());
                // 保存包围盒
                builder.Append("collider = {{" + rect.xMin + "," + rect.yMin + "}, {" + rect.xMax + ", " + rect.yMax + "}}, ");  

                // 保存中心点位置
                builder.Append("center = {" + navTri.GetCenter().x + ", " + navTri.GetCenter().y * 2 + "}, "); 
                //保存区域id
                builder.Append("group_id = " + navTri.GetGroupID());
                builder.Append("}");
            }
        }


        /// <summary>
        /// 从文件中读取导航网格信息
        /// </summary>
        /// <param name="filePath">文件全路径</param>
        /// <param name="navTriangles">读取的导航网格</param>
        /// <returns></returns>
        public NavResCode LoadNavMeshFromResource(string filePath, out List<Triangle> navTriangles, GetResources GetRes)
        {
            navTriangles = new List<Triangle>();
            BinaryReader binReader = null;
            MemoryStream stream = null;

            if (GetRes == null)
            {
                if (!File.Exists(filePath))
                    return NavResCode.FileNotExist;

                // open file
                FileStream fs = File.Open(filePath, FileMode.Open);
                binReader = new BinaryReader(fs);
            }
            else
            {

                UnityEngine.Object FileObject = GetRes(filePath);
                if (FileObject == null)
                {
                    return NavResCode.FileNotExist;
                }

                TextAsset asset = (TextAsset)FileObject;

                stream = new MemoryStream(asset.bytes);
                binReader = new BinaryReader(stream);
            }
            

            NavResCode res = NavResCode.Failed;
            try
            {
                res = LoadNavMeshFromFile(ref navTriangles, binReader);
            }
            catch
            {

            }
            finally
            {
                binReader.Close();
                if( GetRes != null )
                    stream.Close();
            }


            return res;
        }

        /// <summary>
        /// 从文件中读取导航网格信息(这个函数使用C#的文件加载)
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="navTriangles"></param>
        /// <returns></returns>
        public NavResCode LoadNavMeshFromFile(string filePath, out List<Triangle> navTriangles)
        {
            //navTriangles = new List<Triangle>();
            //// check file exist
            //if (!File.Exists(filePath))
            //    return NavResCode.FileNotExist;

            //// open file
            //FileStream fs = File.Open(filePath, FileMode.Open);
            //BinaryReader binReader = new BinaryReader(fs);

            //NavResCode res = NavResCode.Failed;
            //try
            //{
            //    res = LoadNavMeshFromFile(ref navTriangles, binReader);
            //}
            //catch
            //{
            //}
            //finally
            //{
            //    binReader.Close();
            //    fs.Close();
            //}


            //return res;
            return LoadNavMeshFromResource(filePath, out navTriangles, null);
        }

        /// <summary>
        /// 根据传进来BinaryReader读取数据
        /// </summary>
        /// <param name="navTriangles"></param>
        /// <param name="binReader"></param>
        /// <returns></returns>
        public NavResCode LoadNavMeshFromFile(ref List<Triangle> navTriangles, BinaryReader binReader)
        {
            try
            {
                // 读取版本号
                string fileVersion = new string(binReader.ReadChars(NAVMESH_VERSION.Length));
                if (fileVersion != NAVMESH_VERSION)
                    return NavResCode.VersionNotMatch;
                // 读取导航三角形数量
                int navCount = binReader.ReadInt32();
                Triangle currTri;
                for (int i = 0; i < navCount; i++)
                {
                    currTri = new Triangle();
                    currTri.Read(binReader);
                    navTriangles.Add(currTri);
                }
            }
            catch
            {
                //Debug.LogError(e.Message);
                return NavResCode.Failed;
            }
            finally
            {
            }

            return NavResCode.Success;
        }

        /// <summary>
        /// 创建导航网格
        /// </summary>
        /// <param name="polyAll">所有阻挡区域</param>
        /// <param name="triAll">输出的导航网格</param>
        /// <returns></returns>
        public NavResCode CreateNavMesh(List<Polygon> polyAll , ref int id , int groupid , ref List<Triangle> triAll)
        { 
            triAll.Clear();  
            List<Line2D> allLines = new List<Line2D>();  //线段堆栈

            //Step1 保存顶点和边 
            NavResCode initRes = InitData(polyAll);
            if (initRes != NavResCode.Success)
                return initRes;

            int lastNeighborId = -1;
            Triangle lastTri = null;

            //Step2.遍历边界边作为起点
            {
				Line2D sEdge = startEdge;
                allLines.Add(sEdge);
                Line2D edge = null;

                do
                {
                    //Step3.选出计算出边的DT点，构成约束Delaunay三角形
                    edge = allLines[allLines.Count - 1];
                    allLines.Remove(edge);

                    Vector2 dtPoint;
                    bool isFindDt = FindDT(edge, out dtPoint);
                    if (!isFindDt)
                        continue;
                    Line2D lAD = new Line2D(edge.GetStartPoint(), dtPoint);
                    Line2D lDB = new Line2D(dtPoint, edge.GetEndPoint());

                    //创建三角形
					Triangle delaunayTri = new Triangle(edge.GetStartPoint(), edge.GetEndPoint(), dtPoint, id++ , groupid);
                    // 保存邻居节点
                    //                     if (lastNeighborId != -1)
                    //                     {
                    //                         delaunayTri.SetNeighbor(lastNeighborId);
                    //                         if(lastTri != null)
                    //                             lastTri.SetNeighbor(delaunayTri.ID);
                    //                     }
                    //save result triangle
                    triAll.Add(delaunayTri);

                    // 保存上一次的id和三角形
                    lastNeighborId = delaunayTri.GetID();
                    lastTri = delaunayTri;

                    int lineIndex;
                    //Step4.检测刚创建的的线段ad,db；如果如果它们不是约束边
                    //并且在线段堆栈中，则将其删除，如果不在其中，那么将其放入
                    if (!Line2D.CheckLineIn(allEdges, lAD, out lineIndex))
                    {
                        if (!Line2D.CheckLineIn(allLines, lAD, out lineIndex))
                            allLines.Add(lAD);
                        else
                            allLines.RemoveAt(lineIndex);
                    }

                    if (!Line2D.CheckLineIn(allEdges, lDB, out lineIndex))
                    {
                        if (!Line2D.CheckLineIn(allLines, lDB, out lineIndex))
                            allLines.Add(lDB);
                        else
                            allLines.RemoveAt(lineIndex);
                    }

                    //Step5.如果堆栈不为空，则转到第Step3.否则结束循环 
                } while (allLines.Count > 0);
            }

            // 计算邻接边和每边中点距离
            for (int i = 0; i < triAll.Count; i++)
            {
                Triangle tri = triAll[i];
                //// 计算每个三角形每边中点距离
                //tri.calcWallDistance();

                // 计算邻居边
                for (int j = 0; j < triAll.Count; j++)
                {
                    Triangle triNext = triAll[j];
                    if (tri.GetID() == triNext.GetID())
                        continue;

                    int result = tri.isNeighbor(triNext);
                    if (result != -1)
                    {
                        tri.SetNeighbor(result , triNext.GetID() );
                    }
                }
            }

            return NavResCode.Success;
        }

        bool needSplitBig = false;
        // 判断是否需要拆分大三角形
        public bool NeedSplitBig
        {
            get { return needSplitBig; }
            set { needSplitBig = value; }
        }
        int needSplitSize = 50;
        // 拆分的尺寸
        public int NeedSplitSize
        {
            get { return needSplitSize; }
            set { needSplitSize = value; }
        }
        //         void SplitBigTriangle(ref List<Triangle> triangles)
        //         {
        //             // 将面积过大的三角形拆分成三个小三角形
        //             for (int i = 0; i < triangles.Count; i++)
        //             {
        //                 if (triangles[i].Area() > NeedSplitSize)
        //                 {
        //                 }
        //             }
        //         }

        /// <summary>
        /// 判断点是否是线段的可见点，组成的三角形没有和其他边相交
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsPointVisibleOfLine(Line2D line, Vector2 point)
        {
            if (line == null)
                return false;

            Vector2 sPnt = line.GetStartPoint();
            Vector2 ePnt = line.GetEndPoint();

            // 是否是线段端点
            if (point == sPnt || point == ePnt)
                return false;
            //点不在线段的右侧（多边形顶点顺序为顺时针）
            if (line.ClassifyPoint(point) != PointSide.RIGHT_SIDE)
                return false;

            if (!IsVisibleIn2Point(sPnt, point))
                return false;

            if (!IsVisibleIn2Point(ePnt, point))
                return false;

            return true;
        }

        /// <summary>
        /// 判断这条线段是否没有和其他的边相交
        /// </summary>
        /// <param name="sPnt"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsVisibleIn2Point(Vector2 sPnt, Vector2 ePnt)
        {
            Line2D line = new Line2D(sPnt, ePnt);
            Vector2 interPos;

            foreach (Line2D edge in allEdges)
            {
                if (edge.Intersection(line, out interPos) == LineCrossState.CROSS)
                {
                    if (!NMath.IsEqualZero(sPnt - interPos)
                        && !NMath.IsEqualZero(ePnt - interPos))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 找到指定边的约束边DT
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private bool FindDT(Line2D line, out Vector2 dtPoint)
        {
            dtPoint = new Vector2();
            if (line == null)
                return false;

            Vector2 ptA = line.GetStartPoint();
            Vector2 ptB = line.GetEndPoint();

            List<Vector2> visiblePnts = new List<Vector2>();
            foreach (Vector2 point in allPoints)
            {
                if (IsPointVisibleOfLine(line, point))
                    visiblePnts.Add(point);
            }

            if (visiblePnts.Count == 0)
                return false;

            bool bContinue = false;
            dtPoint = visiblePnts[0];

            do
            {
                bContinue = false;
                //Step1.构造三角形的外接圆，以及外接圆的包围盒
                Circle circle = NMath.CreateCircle(ptA, ptB, dtPoint);
                Rect boundBox = NMath.GetCircleBoundBox(circle);

                //Step2. 依次访问网格包围盒内的每个网格单元：
                //若某个网格单元中存在可见点 p, 并且 ∠p1pp2 > ∠p1p3p2，则令 p3=p，转Step1；
                //否则，转Step3.
                float angOld = (float)Math.Abs(NMath.LineRadian(ptA, dtPoint, ptB));
                foreach (Vector2 pnt in visiblePnts)
                {
                    if (pnt == ptA || pnt == ptB || pnt == dtPoint)
                        continue;
                    if (!boundBox.Contains(pnt))
                        continue;

                    float angNew = (float)Math.Abs(NMath.LineRadian(ptA, pnt, ptB));
                    if (angNew > angOld)
                    {
                        dtPoint = pnt;
                        bContinue = true;
                        break;
                    }
                }

                //false 转Step3
            } while (bContinue);

            //Step3. 若当前网格包围盒内所有网格单元都已被处理完，
            // 也即C（p1，p2，p3）内无可见点，则 p3 为的 p1p2 的 DT 点
            return true;
        }

        /// <summary>
        /// 初始化创建导航网格需要的数据
        /// </summary>
        /// <param name="polyAll">所有阻挡区域</param>
        /// <returns></returns>
        private NavResCode InitData(List<Polygon> polyAll)
        {
            allEdges = new List<Line2D>();
            allPoints = new List<Vector2>();

            PolyResCode resCode = NavUtil.UnionAllPolygon(ref polyAll);
            if (resCode != PolyResCode.Success)
                return NavResCode.Failed;
            // 保存所有点和边
            foreach (Polygon poly in polyAll)
            {
                if (poly.GetPoints().Count < 3)
                    continue;
                AddPoint(poly.GetPoints());
                AddEdge(poly.GetPoints());
            }

			if(polyAll != null && polyAll.Count > 0 )
            {
                List<Vector2> pointList = polyAll[0].GetPoints();
                startEdge = new Line2D(pointList[0], pointList[1]);
            }
				
            return NavResCode.Success;
        }

        /// <summary>
        /// 保存用到的顶点
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private NavResCode AddPoint(List<Vector2> points)
        {
            foreach (Vector2 point in points)
            {
                allPoints.Add(point);
            }

            return NavResCode.Success;
        }

        /// <summary>
        /// 保存所有边
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private NavResCode AddEdge(List<Vector2> points)
        {
            Vector2 pBegin = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 pEnd = points[i];
                Line2D line = new Line2D(pBegin, pEnd);
                allEdges.Add(line);
                pBegin = pEnd;
            }
            Line2D lineEnd = new Line2D(pBegin, points[0]);
            allEdges.Add(lineEnd);

            return NavResCode.Success;
        }


        public void WriteClientCommonFile(DirectoryInfo info, string filePath)
        { 
           
            StringBuilder builder = new StringBuilder();
            builder.Append("DataMask = DataMask or {}");
            builder.Append("\n"); 
            FileInfo[] files = info.GetFiles();
            List<int> list = new List<int>();
            foreach (var item in files)
            {
                string name = item.Name;
                string mapId = name.Split('.')[0];
                list.Add(int.Parse(mapId));
            }

            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetRec", "rec");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetSmallRec", "small_rec"); 
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetArea", "area");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetSafeArea", "safe_area");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetTransfer", "transfer");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetBirth", "birth");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetRelive", "relive");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetMonster", "monster");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetCollection", "collection");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetJumpArea", "jump_area");
            builder.Append("\n");
            ClientCommonFileFunction(builder, list, "GetShadowArea", "shadow_area");
            builder.Append("\n");

            String dirName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            WriteToFile(dirName + "/" + "DataMask" + ".lua", builder.ToString());
        }



        private void WriteToFile(string path, string content)
        { 
            FileStream fs = File.Create(path);
            byte[] bytes = UTF8Encoding.Default.GetBytes(content);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
        }
        

        private void ClientCommonFileFunction(StringBuilder builder, List<int> list, string funName, string fieldName)
        {
            builder.Append("function DataMask." + funName + "(maskId)");
            builder.Append("\n");
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append("\t");
                if (i != list.Count - 1)
                {
                    if (i == 0)
                    {
                        builder.Append("if maskId == " + list[i] + " then ");
                    }
                    else
                    {
                        builder.Append("elseif maskId == " + list[i] + " then ");
                    }
                    builder.Append("\n");
                    builder.Append("\t\t");
                    builder.Append("return DataMask" + list[i] + "." + fieldName);
                    builder.Append("\n");
                }
                else
                {
                    builder.Append("else");
                    builder.Append("\n");
                    builder.Append("\t\t");
                    builder.Append("LogError(\"not find \" .. maskId .. \" " + fieldName + " data\", true)");
                    builder.Append("\n");
                    builder.Append("\t\t");
                    builder.Append("return nil");
                    builder.Append("\n");
                    builder.Append("\t");
                    builder.Append("end"); 
                }
            }
            builder.Append("\n");
            builder.Append("end");
            builder.Append("\n");
        }

        /// <summary>
        /// 根据外接矩形来计算多边形的中心
        /// </summary>
        /// <param name="allPoints">多边形的点集</param>
        /// <returns>计算结果</returns>
        private Vector2 CalCenterByOutsideSquare(List<Vector2> allPoints)
        {
            Vector2 center = Vector2.zero;
            float minX = float.MaxValue , minY = float.MaxValue , maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < allPoints.Count; i++)
            {
                if (allPoints[i].x < minX) minX = allPoints[i].x;
                if (allPoints[i].y < minY) minY = allPoints[i].y;
                if (allPoints[i].x > maxX) maxX = allPoints[i].x;
                if (allPoints[i].y > maxY) maxY = allPoints[i].y;
            }

            center.x = (minX + maxX) / 2f;
            center.y = (minY * 2 + maxY * 2) / 2f;

            return center;
        }

        /// <summary>
        /// 根据多边形所有顶点的平均值求几何中心
        /// </summary>
        /// <param name="allPoints">多边形的点集</param>
        /// <returns>计算结果</returns>
        private Vector2 CalCenterByAvager(List<Vector2> allPoints)
        {
            Vector2 center = Vector2.zero;

            for (int i = 0; i < allPoints.Count; ++i)
            {
                center = center + allPoints[i];
            }
            center.y *= 2f;
            return center/allPoints.Count;
        }

        /// <summary>
        /// 根据多边形内的三角形面积计算中心
        /// </summary>
        /// <param name="allTriangles">多边形内的三角形</param>
        /// <returns>计算结果</returns>
        private Vector2 CalCenterByTriangle(List<Triangle> allTriangles)
        {
            Vector2 center = Vector2.zero;
            float cs =0 ,s = 0;
            for (int i = 0; i < allTriangles.Count; i++)
            {
                Vector2 point1 = allTriangles[i].GetPoint(0);
                Vector2 point2 = allTriangles[i].GetPoint(1);
                Vector2 point3 = allTriangles[i].GetPoint(2);

                cs = (point1.x * point2.y + point2.x * point3.y + point3.x * point1.y - point1.x * point3.y - point2.x * point1.y - point3.x * point2.y ) / 2f;

                if ( s <= 0.0f)
                {
                    center = allTriangles[i].GetCenter();
                    s += cs;
                    continue;
                }

                float k = cs / s;
                center.x = (center.x + k * allTriangles[i].GetCenter().x) / (1 + k);
                center.y = (center.y + k * allTriangles[i].GetCenter().y) / (1 + k);
                s += cs;
            }
            center.y *= 2;
            return center;
        }
    }
}
