using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BattleCore
{
    /// <summary>
    /// Player input state, triggers and indices, cancel stack, and UI ties.
    /// </summary>
    public class SelectionState
    {
        public GameObject SelectedUnit;
        public List<GameObject> ActionableTiles = new();

        public bool moveTrigger, attackTrigger, ultimateTrigger, turnEndTrigger, rotateTrigger;
        public bool moveTrigger2, attackTrigger2; // confirmation triggers
        private bool _moveTrigger, _actionTrigger;

        public Stack<Action> cancelStack = new();

        public int moveIndex = 0, maxMoveIndex, attackIndex = 0, maxAttackIndex;
        public GameObject arrowTest; // pointer
        public string selectSkill;

        public Camera MainCam;
        public TextMeshProUGUI behavioirText;

        public void InitTriggers()
        {
            moveTrigger = attackTrigger = ultimateTrigger = true;
            turnEndTrigger = false;
            moveTrigger2 = attackTrigger2 = false;
            _moveTrigger = _actionTrigger = false;
            moveIndex = attackIndex = 0;
            maxMoveIndex = maxAttackIndex = 0;
            rotateTrigger = false;
        }

        public void EnterMoveSelection(List<GameObject> actionable, Action cancel)
        {
            if (_moveTrigger == false)
            {
                moveIndex = 0;
                cancelStack.Push(cancel);
                _moveTrigger = true;
                if (arrowTest) arrowTest.SetActive(true);
                maxMoveIndex = actionable.Count;
            }
        }

        public void LeaveMoveSelection()
        {
            if (arrowTest) arrowTest.SetActive(false);
            _moveTrigger = false;
            moveTrigger2 = false;
            moveTrigger = true;
            rotateTrigger = false;
            moveIndex = 0;
            maxMoveIndex = 0;
        }

        public void EnterAttackSelection(Action cancel)
        {
            if (_actionTrigger == false)
            {
                attackIndex = 0;
                cancelStack.Push(cancel);
                _actionTrigger = true;
            }
        }

        public void LeaveAttackSelection()
        {
            _actionTrigger = false;
            attackTrigger2 = false;
            attackTrigger = true;
            attackIndex = 0;
        }

        public void 이동활성화(BaseCombatant bc)
        {
            if (bc == null) return;
            if (moveTrigger && bc.현재_이동력 > 0)
                moveTrigger = false; // enter selection
            else if (!moveTrigger)
                moveTrigger2 = true; // confirm
        }

        public void 공격활성화(BaseCombatant bc, int skillCost)
        {
            if (bc == null) return;
            if (attackTrigger && bc.현재_행동력 > 0)
                attackTrigger = false; // enter selection
            else if (!attackTrigger && !attackTrigger2 && bc.현재_행동력 >= skillCost)
            {
                attackTrigger2 = true; // confirm
            }
        }

        public void 방어활성화(BaseCombatant bc)
        {
            if (bc == null || bc.현재_행동력 <= 0) return;
            BattleEffectSystem.Instance.효과등록(bc.gameObject, "철벽");
            bc.행동력_변경(-1);
        }

        public void 턴종료활성화() => turnEndTrigger = true;

        public void 이동회전활성화(BaseCombatant bc)
        {
            if (bc == null) return;
            Debug.Log("2");
            if (!moveTrigger && bc.현재_이동력 > 0) rotateTrigger = true;
            Debug.Log($"3 / {rotateTrigger}");
            // 걍 Trigger 만들어서 true false로 바꾸고 PlayerStart에서 조절?
        }
    }
}