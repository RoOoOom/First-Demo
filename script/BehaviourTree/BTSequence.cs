using System.Collections;
using System.Collections.Generic;
using BehaviourTree;
using UnityEngine;

public class BTSequence : BTComposite {
    public override BTNodeStatus Tick()
    {
        if (_children == null || _children.Count == 0)
            return BTNodeStatus.Failed;

        var _active_node = _children[_process_index];
        BTNodeStatus result = _active_node.Tick();

        if (result == BTNodeStatus.Compeleted)
        {
            _process_index++;
        }
        else if (result == BTNodeStatus.Failed)
        {
            _process_index = 0;
            return BTNodeStatus.Failed;
        }

        if (_process_index >= _children.Count)
        {
            _process_index = 0;
            return BTNodeStatus.Compeleted;
        }

        return BTNodeStatus.Running;
    }
}
