using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class BTCondition : BTNode {
    public delegate bool ExternalFunc();
    protected int _condition_type = 0; //0表示与，1表示或
    protected List<ExternalFunc> _func_list;

    public BTCondition(int condct_type = 0)
    {
        _condition_type = condct_type;
    }

    public virtual bool CheckCondition()
    {
        if (IsFuncArrayEmpty()) return true;
        bool result = true;
        foreach (ExternalFunc func in _func_list)
        {
            result = func();
            if (_condition_type == 0 && !result)
                return false;
            else if (_condition_type == 1 && result)
                return true;
        }

        return _condition_type==0; //如果能遍历完循环，要么type==0且全对，要么type==1且全错
    }

    public virtual void AddExternalFunc( ExternalFunc func )
    {
        if (_func_list == null)
        {
            _func_list = new List<ExternalFunc>();
        }

        if (_func_list.Contains(func))
            return;

        _func_list.Add(func);
    }

    public override BTNodeStatus Tick()
    {
        if (CheckCondition())
        {
            return BTNodeStatus.Compeleted;
        }
        else
        {
            return BTNodeStatus.Failed;
        }
    }

    public override void Clear()
    {
        base.Clear();

        _func_list.Clear();
    }

    protected bool IsFuncArrayEmpty()
    {
        if (_func_list == null || _func_list.Count == 0)
            return true;
        return false;
    }
}
