using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class EnemyAI : MonoBehaviour {
    RoleAttribute _attr;
    BTSelector root;
	// Use this for initialization
	void Start () {
        _attr = GetComponent<RoleBehaviour>().m_attribute;
        ConstructBehaviourTree();        
	}
	
	// Update is called once per frame
	void Update () {
		if (root!=null && !_attr._obsessed)
        {
            root.Tick();
        }
	}

    void ConstructBehaviourTree()
    {
        root = new BTSelector();
        BTSequence btHeavyAtk = new BTSequence();
        BTSequence btQuickAtk = new BTSequence();
        BTSequence btRun = new BTSequence();

        AIdle actIdle = new AIdle(null);

        BTCondition inRange1 = new BTCondition();
        inRange1.AddExternalFunc(IsInQuickAtackRange);
        AQuickttack actQuickAtk = new AQuickttack(inRange1);

        BTCondition inRange2 = new BTCondition();
        inRange2.AddExternalFunc(IsInHeavyAtackRange);
        AHeavyAttack actHeavyAtk = new AHeavyAttack(inRange2);

        BTCondition inRange3 = new BTCondition();
        inRange3.AddExternalFunc(IsInHuntRange);
        ARun actRun = new ARun(inRange3);

        

        btQuickAtk.AddChild(actQuickAtk);
        btQuickAtk.AddChild(actIdle);
        btHeavyAtk.AddChild(actHeavyAtk);
		btHeavyAtk.AddChild(actIdle);
        btRun.AddChild(actRun);

        if (_attr._quickAtk_range < _attr._heavyAtk_range)
        {
            root.AddChild(btQuickAtk);
            root.AddChild(btHeavyAtk);
        }
        else
        {
            root.AddChild(btHeavyAtk);
            root.AddChild(btQuickAtk);
        }

        root.AddChild(btRun);
        root.AddChild(actIdle);

        root.Init(GetComponent<RoleBehaviour>());
    }

    public bool IsInQuickAtackRange()
    {
        float dis = DataManager.Instance.DistanceOfPlayer(transform.position);
        if (dis <= _attr._quickAtk_range)
            return true;
        else
            return false;        
    }

    public bool IsInHeavyAtackRange()
    {
        float dis = DataManager.Instance.DistanceOfPlayer(transform.position);
        if (dis <= _attr._heavyAtk_range)	
            return true;
        else
            return false;
    }

    public bool IsInHuntRange()
    {		
        float dis = DataManager.Instance.DistanceOfPlayer(transform.position);
        if (dis <= _attr._hunt_range)
            return true;
        else
            return false;
    }
}
