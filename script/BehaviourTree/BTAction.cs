using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamDungon;

namespace BehaviourTree
{
    public class BTAction : BTNode
    {
        public BTNodeStatus _curStatus = BTNodeStatus.Running;
        public  BTCondition _condition_node;
        public BTAction(BTCondition condition)
        {
            _condition_node = condition;
        }
        public override BTNodeStatus Tick()
        {
            return _curStatus;
        }

        public bool IsConditionEmpty()
        {
            return _condition_node == null;
        }
    }

    public class AHeavyAttack : BTAction
    {
        public AHeavyAttack(BTCondition condition) : base(condition) { }

        public override BTNodeStatus Tick()
        {
            if (!IsConditionEmpty() && _condition_node.Tick() != BTNodeStatus.Compeleted)
            {
                return BTNodeStatus.Failed;
            }

			if (_DataBase.m_attribute._status != RoleStatus.Status_HeavyAttack)
			{                
				_DataBase.HeavyAttack();
				Debug.Log ("heavy attack running");
				return BTNodeStatus.Running;
			}

			if (!_DataBase.IsAnimationName(AnimatorConst.Animator_HeavyAttack) ||  _DataBase.IsAnimationEnd(AnimatorConst.Animator_HeavyAttack))
            {
				Debug.Log ("heavy attack compeleted");
                return BTNodeStatus.Compeleted;
            }           

            return BTNodeStatus.Running;
        }
    }

    public class AQuickttack : BTAction
    {
        public AQuickttack(BTCondition condition) : base(condition) { }

        public override BTNodeStatus Tick()
        {
            if (!IsConditionEmpty() && _condition_node.Tick() != BTNodeStatus.Compeleted)
            {
                return BTNodeStatus.Failed;
            }

			if (_DataBase.m_attribute._status != RoleStatus.Status_QuickAttack)
			{
				_DataBase.QuickAttack();
				return BTNodeStatus.Running;
			}           

			if (!_DataBase.IsAnimationName(AnimatorConst.Animator_QuickAttack) || _DataBase.IsAnimationEnd(AnimatorConst.Animator_QuickAttack))
            {				
                return BTNodeStatus.Compeleted;
            }           

            return BTNodeStatus.Running;
        }
    }

    public class ARun : BTAction
    {
        public ARun(BTCondition condition) : base(condition) { }

        public override BTNodeStatus Tick()
        {
            if (!IsConditionEmpty() && _condition_node.Tick() != BTNodeStatus.Compeleted)
            {
                return BTNodeStatus.Failed;
            }

			if (_DataBase.m_attribute._status == RoleStatus.Status_Idle) {
				_DataBase.PathFindingLogic ();
			} 
			/*
			else {
				if(!_DataBase.IsAnimationName(AnimatorConst.Animator_Run))
				{
					_DataBase.PathFindingLogic ();
				}
			}*/

            return BTNodeStatus.Compeleted;
        }
    }

    public class AIdle : BTAction
    {
        public AIdle(BTCondition condition) : base(condition) { }

        public override BTNodeStatus Tick()
        {
            if (!IsConditionEmpty() && _condition_node.Tick() != BTNodeStatus.Compeleted)
            {
                return BTNodeStatus.Failed;
            }

            if (_DataBase.m_attribute._status != RoleStatus.Status_Idle)
            {
                _DataBase.BackToIdle();
                //_DataBase.m_attribute._status = RoleStatus.Status_Idle;
            }

            return BTNodeStatus.Compeleted;
        }
    }
}
