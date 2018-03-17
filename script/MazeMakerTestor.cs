using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeMakerTestor : MonoBehaviour {
    public class RoomRect
    {
        public int posRow = -1;
        public int posCol = -1;
        public int width = 4;
        public int height = 4;

        public RoomRect( int row , int col, int wid, int hei )
        {
            posRow = row;
            posCol = col;
            width = wid;
            height = hei;
        }
    }

    public enum TileType
    {
        Barrier = 0,
        Room,
        Maze,
        Door
    }

    public GameObject m_roomTilePrefab;
    public GameObject m_maptilePrefab;
    public GameObject m_doorTilePrefab;
    private GameObject m_tileParent;
    public int m_RoomTries = 100;
    public int m_roadTries = 100;
    public int m_row;
    public int m_col;
    public TileType[,] m_map;
    List<RoomRect> m_rooms = new List<RoomRect>();
	// Use this for initialization
	void Start () {
        CreatMaze();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreatMaze();
        }
	}

    public void CreatMaze()
    {
        m_map = null;
        m_map = new TileType[m_row, m_col];

        if (m_tileParent != null)
        {
            DestroyImmediate(m_tileParent);
        }

        m_tileParent = new GameObject("tilegroup");

        AddRoom();
        GrowMaze();
        ShowTileArea();
    }

    void ShowTileArea()
    {
        Vector3 startPoint = Vector3.zero;
        for (int i = 0; i<m_row ; ++i)
        {
            for (int j = 0; j < m_col; ++j)
            {
                if (m_map[i, j] == TileType.Room)
                {
                   GameObject go = Instantiate(m_roomTilePrefab, m_tileParent.transform);
                    go.transform.position = new Vector3((float)j, 0, (float)i);
                    go.name = i.ToString() + '-' + j.ToString();
                }

                if (m_map[i, j] == TileType.Maze)
                {
                    GameObject go = Instantiate(m_maptilePrefab, m_tileParent.transform);
                    go.transform.position = new Vector3((float)j, 0, (float)i);
                    go.name = i.ToString() + '-' + j.ToString();
                }

                if (m_map[i, j] == TileType.Door)
                {
                    GameObject go = Instantiate(m_doorTilePrefab, m_tileParent.transform);
                    go.transform.position = new Vector3((float)j, 0, (float)i);
                    go.name = i.ToString() + '-' + j.ToString();
                }
            }
        }
    }

    public void AddRoom()
    {
        for (int i = 0; i < m_RoomTries; i++)
        {
            int width = Random.Range(1, 3)*2 + 1;
            int height = Random.Range(1, 3)*2 + 1;
            int startRow = Random.Range(0, (m_row - height) / 2) * 2 + 1;
            int startCol = Random.Range(0, (m_col - width) / 2) * 2 + 1;

            if (!IsOverflow(startRow, startCol, startRow + height-1, startCol + width-1))
            {
                RoomRect room = new RoomRect( startRow , startCol , width , height );
                FillTile(room);
                m_rooms.Add(room);
            }
        }
    }

    private bool IsOverflow(int startRow, int startCol, int endRow, int endCol)
    {
        bool overflow = false;

        if (m_map[startRow, startCol]>0 || m_map[endRow,endCol] >0 || m_map[endRow, startCol] >0 || m_map[startRow,endCol] >0) 
            overflow = true;
        
        return overflow;
    }

    private void FillTile(RoomRect room)
    {
        if (m_map == null)
        {
            Debug.Log("地图数据不存在");
            return;
        }

        for (int i = 0; i < room.height; i++)
        {
            for (int j = 0; j < room.width; j++)
            {
                m_map[room.posRow + i, room.posCol + j]  = TileType.Room;
            }
        }
    }

    private void GrowMaze()
    {
        for( int i = 0; i<m_roadTries ;++i )
        {
            int posRow = Random.Range(0, (m_row - 1) / 2) * 2;
            int posCol = Random.Range(0, (m_col - 1) / 2) * 2;

            if (m_map[posRow, posCol] != TileType.Barrier) continue;

            int startFill, endFill;
            if (HorizonlConnect(posRow, posCol , out startFill ,out endFill))
            {
                FillTileHorizontal(startFill , endFill , posRow);
            }

            if (VerticalConnect(posRow, posCol ,out startFill ,out endFill))
            {
                FillTileVertical(startFill, endFill, posCol);
            }
        }
    }

    private void FillTileHorizontal( int startCol , int endCol ,int row )
    {
        if (m_map[row, startCol] == TileType.Room)
        {
            m_map[row, startCol] = TileType.Door;
        }

        for (int i = startCol + 1 ; i < endCol; ++i)
        {
            m_map[row, i] = TileType.Maze;
        }

        if (m_map[row, endCol] == TileType.Room)
        {
            m_map[row, endCol] = TileType.Door;
        }
    }

    private void FillTileVertical(int startRow, int endRow, int col)
    {
        if (m_map[startRow, col] == TileType.Room)
        {
            m_map[startRow, col] = TileType.Door;
        }

        for (int i = startRow + 1; i <endRow; ++i)
        {
            m_map[i, col] = TileType.Maze;
        }

        if (m_map[endRow, col] == TileType.Room)
        {
            m_map[endRow, col] = TileType.Door;
        }
    } 

    private bool HorizonlConnect( int centerRow , int centerCol , out int startCol , out int endCol)
    {
        bool found = false;

        startCol = 0;
        endCol = 0;

        int connet = 0;
        int curCol = centerCol-1;
        while (curCol >= 0)
        {
            if (m_map[centerRow, curCol] != TileType.Barrier)
            {
                connet++;
                startCol = curCol;
                break;
            }

            curCol--;
        }

        curCol = centerCol + 1;
        while (curCol < m_col)
        {
            if (m_map[centerRow, curCol] != TileType.Barrier)
            {
                connet++;
                endCol = curCol;
                break;
            }
            curCol++;
        }

        if (connet == 2)
            found = true;

        return found;
    }

    private bool VerticalConnect(int centerRow, int centerCol , out int startRow, out int endRow)
    {
        bool found = false;
        startRow = 0;
        endRow = 0;

        int connet = 0;
        int curRow = centerRow - 1;
        while (curRow >= 0)
        {
            if (m_map[curRow, centerCol]!= TileType.Barrier)
            {
                connet++;
                startRow = curRow;
                break;
            }

            curRow--;
        }

        curRow = centerRow + 1;
        while (curRow < m_row)
        {
            if (m_map[curRow, centerCol] != TileType.Barrier)
            {
                connet++;
                endRow = curRow;
                break;
            }
            curRow++;
        }

        if (connet == 2)
            found = true;


        return found;
    }
}
