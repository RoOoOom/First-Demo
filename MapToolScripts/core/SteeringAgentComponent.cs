#region Copyright
// ******************************************************************************************
//
// 							SimplePath, Copyright © 2011, Alex Kring
//
// ******************************************************************************************
#endregion

using UnityEngine;
using System.Collections; 
using System.Collections.Generic;

public class SteeringAgentComponent : MonoBehaviour
{
    #region Unity Editor Fields
    public float m_arrivalDistance = 0;
    public float m_maxSpeed = 3.0f;
    public float m_lookAheadDistance = 0.50f;
    public float m_slowingDistance = 1.0f;
    public float m_accelerationRate = 25.0f;
    public float m_gravitationalAccelerationRate = 0.0f;
    public Color m_debugPathColor = Color.yellow;
    public Color m_debugGoalColor = Color.red;
    public bool m_debugShowPath = true;
    public bool m_debugShowVelocity = false;
    public float speed = 0.02f;
    #endregion

    #region Fields 
    Vector3 newVelocity;
    Vector3 seekPos;
    Vector3 currentFloorPosition;

    private bool planChanged = false;
    List<Vector2> plan = null;
    int curPathNodeIndex = 1; //从1开始，0记录的是当前位置

    private bool _moveCommand = false;
    private float _totalTime = 0f;

    private long frame = 0;
    #endregion

    private GameObject mainCamera;


    #region MonoBehaviour Functions
    void Awake()
    {
        mainCamera = GameObject.Find("Main Camera");
    }

    void Update()
    {
        if (_moveCommand)
        {
            if (_totalTime > 0)
            {
                float time = Time.deltaTime;
                float div = _totalTime / time;
                Vector3 tPos = transform.position + newVelocity * (div > 1 ? 1 : div);
                Game.NavMesh.NavTriangle triangle = Game.NavMesh.Seeker.instance.GetTriangleByPos(tPos);
                if (null != triangle)
                {
                    transform.position = tPos;
                    mainCamera.transform.position = tPos;
                }
                _totalTime -= time;
            }
            else
            {
                _moveCommand = false;
            }

            return;
        }

        ++frame; 

        if (null == plan || plan.Count <= 1) return;

        if (planChanged)
        {
            planChanged = false;
            curPathNodeIndex = 1;
            UpdateVelociity();
        }

        if (curPathNodeIndex < plan.Count)
        { 

            float distance = Vector3.Distance(seekPos, this.transform.position);

            if (distance < speed)
            {
                if (++curPathNodeIndex == plan.Count)
                {
                    StopSteering();
                }
                else
                {
                    UpdateVelociity();
                }
            }
            else
            {
                transform.position += newVelocity;
                mainCamera.transform.position += newVelocity;
            }
        }
    }

    void UpdateVelociity()
    {
        seekPos = new Vector3(plan[curPathNodeIndex].x, 0, plan[curPathNodeIndex].y);
        currentFloorPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 dir = seekPos - currentFloorPosition;
        newVelocity = dir.normalized * speed; 
    }

    void OnDrawGizmos()
    {
        if (m_debugShowPath)
        {
            if (null != plan && plan.Count > 0)
            {
                // Draw the active path, if it is solved.
                Gizmos.color = m_debugPathColor;
                for (int i = 1; i < plan.Count; i++)
                {
                    Vector3 start = plan[i - 1];
                    Vector3 end = plan[i];
                    Gizmos.DrawLine(start, end);
                    Gizmos.DrawWireSphere(end, m_arrivalDistance);
                    //Debug.LogError("pos: " + end);
                }

                // Draw the goal pos, if there is a path request.
                Gizmos.color = m_debugGoalColor;
                Vector3 goal = plan[plan.Count - 1];
                Gizmos.DrawWireSphere(goal, m_arrivalDistance);
            }
        }
    }
    #endregion

    public void StopSteering()
    {
        if (null != plan && plan.Count > 0)
        { 
            plan = null; 
        }
    }

    /// <summary>
    /// 寻路路径移动
    /// </summary>
    /// <param name="newPlan"></param>
    public void SetNewPlan(List<Vector2> newPlan)
    {
        planChanged = true;
        plan = newPlan;
    }


    /// <summary>
    /// 沿当前方向移动多久
    /// </summary>
    /// <param name="duration"></param>
    public void MoveAhead(float duration, float speed)
    {
        plan = null;
        float angle = gameObject.transform.localEulerAngles.y * Mathf.PI / 180f;
        float x = Mathf.Sin(angle);
        float y = Mathf.Cos(angle);
        Vector3 dir = new Vector3(x, y, 0f);
        newVelocity = dir * speed;
        _totalTime = duration;
        _moveCommand = true;
    }
}
