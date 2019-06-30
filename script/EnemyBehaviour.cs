using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DreamDungon;

public class EnemyBehaviour : RoleBehaviour {
    NavMeshAgent m_navAgent;
    void OnEnable()
    {
        m_forward_axis = transform.forward;
        m_right_axis = transform.right;
        _anim_ctrl = GetComponent<Animator>();
        m_navAgent = GetComponent<NavMeshAgent>();
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

    void LateUpdate()
    {
        if ( m_attribute._obsessed && _status == null)
        {
            //Debug.Log("====here is fix null status ======");
            AnimatorStateInfo anim_state_info = _anim_ctrl.GetCurrentAnimatorStateInfo(0);
            if (anim_state_info.IsName("Idle"))
            {
                ResetBehaviourStatus();
            }
        }
    }

    public override bool IsAnimationEnd(string anim_name)
    {
        AnimatorStateInfo anim_state_info = _anim_ctrl.GetCurrentAnimatorStateInfo(0);
        if (anim_state_info.IsName(anim_name))
        {
            return anim_state_info.normalizedTime >= 1f;
        }
        return false;
    }

    public override bool IsAnimationName(string anim_name)
    {
        AnimatorStateInfo anim_state_info = _anim_ctrl.GetCurrentAnimatorStateInfo(0);
        return anim_state_info.IsName(anim_name);
    }

    public override void QuickAttack()
    {
		StopPathFinding ();
        _anim_ctrl.SetTrigger("quick");
        m_attribute._status = RoleStatus.Status_QuickAttack;

        DataManager.Instance.CalculateFightEffect(gameObject, 0);
    }

    public override void HeavyAttack()
    {
		StopPathFinding ();
        _anim_ctrl.SetTrigger("heavy");
        m_attribute._status = RoleStatus.Status_HeavyAttack;

        DataManager.Instance.CalculateFightEffect(gameObject, 1);
    }

    public override void PathFindingLogic()
    {
        _anim_ctrl.SetTrigger("run");
        m_attribute._status = RoleStatus.Status_Run;
		m_navAgent.SetDestination (DataManager.Instance.GetCurPlayerPosition ());
    }

    public override void BackToIdle()
    {
        if (m_attribute._obsessed)
        {
            transform.forward = m_forward_axis;
        }

		if(!IsAnimationName(AnimatorConst.Animator_Idle)) _anim_ctrl.SetTrigger("idle");
        m_attribute._status = RoleStatus.Status_Idle;
    }

    public void UnderAttack(FightRequest FR)
    {
        Debug.Log(string.Format("{0}attack {1} make {2} damage", FR._attacker.name, this.name, FR.attack));
    }

    public override void AcceptMessage(RoleCmdRequest request)
    {        
        if (_status == null)
        {
            //Debug.Log("-------->> _status is null <<-------");
            return;
        }

        //Debug.Log("_status ======>>" + _status.ToString());
        _status = _status.InputHandle(this, request);
    }

    public override void ResetBehaviourStatus()
    {
        m_right_axis = transform.right;
        m_forward_axis = transform.forward;

        _status = new MonsterIdle();
        m_attribute._status = RoleStatus.Status_Idle;                      
    }

    public override void RotateMoveAxis(float rad)
    {
        m_forward_axis = Quaternion.AngleAxis(rad, Vector3.up) * m_forward_axis;
        m_right_axis = Quaternion.AngleAxis(rad, Vector3.up) * m_right_axis;
    }

    public override void CalTurnRadAndStep(float horizontal, float vertical)
    {
        if (horizontal < 0f)
        {
            RotateMoveAxis(-m_turn_rad);
        }
        else if (horizontal > 0f)
        {
            RotateMoveAxis(m_turn_rad);
        }

        if (m_attribute._status == RoleStatus.Status_Run)
        {
            Vector3 step = m_right_axis * horizontal + m_forward_axis * vertical;
            transform.forward = step.normalized;
            m_navAgent.Move(step.normalized * m_attribute._step_rate);
        }
    }

    public override void Realase()
    {
        DataManager.Instance.SoulReturnBody();
    }

	private void StopPathFinding()
	{
		if ( !m_attribute._obsessed && m_attribute._status == RoleStatus.Status_Run) {		
			m_navAgent.SetDestination (transform.position);
		}
	}
}
