using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class BTNode{
    public RoleBehaviour _DataBase;

    public virtual bool Init(RoleBehaviour roleBehaviour)
    {
        if (roleBehaviour == null)
        {
            Debug.LogError("role behaviour compenont is empty");
            return false;
        }

        _DataBase = roleBehaviour;
        return true;
    }
    public virtual BTNodeStatus Tick()
    {
        return BTNodeStatus.Compeleted;
    }

    public virtual void Clear()
    {
        _DataBase = null;
    }
}
