using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class BTComposite : BTNode {
    public  int _process_index = 0;
    //public  BTNodeStatus _record_status = BTNodeStatus.Running;
    //public BTNode _active_node;
    public List<BTNode> _children = new List<BTNode>();

    public override bool Init(RoleBehaviour roleBehaviour)
    {
        base.Init(roleBehaviour);

        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].Init(roleBehaviour);
        }

        return true;
    }

    public void AddChild(BTNode child_node)
    {
        if (_children == null)
        {
            _children = new List<BTNode>();
        }

        if (child_node == null)
        {
            Debug.LogWarning("try to add a empty child node to composite node");
            return;
        }
        _children.Add(child_node);
    }

    public override void Clear()
    {
        base.Clear();

        int count = _children.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                _children[i].Clear();
            }

            _children.Clear();
        }
    }
}
