using BattleCore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class bUIPresenter
{
    private readonly BattleContext _ctx;
    private readonly bUIView _view;

    public bUIPresenter(BattleContext c, bUIView v)
    {
        _ctx = c;
        _view = v;
    }

    public (bool notPlayer, int mp, int ap, int up, int maxup) GetButtonsCondition()
    {

        bool notPlayer = _ctx.NotPlayerTurn();
        int mp = _ctx.PC != null ? _ctx.PC.현재_이동력 : 0;
        int ap = _ctx.PC != null ? _ctx.PC.현재_행동력 : 0;
        int up = _ctx.PC != null ? _ctx.PC.투지게이지 : 0;
        int maxup = _ctx.PC != null ? 120 : 0;
        return (notPlayer, mp, ap, up, maxup);
    }

    public bool HasUnit() => _ctx.HasUnit();

    public void OnPressPrevious()
    {
        if (_ctx.Selection.cancelStack.Count <= 0) return;

        while (_ctx.Selection.cancelStack.Count > 0)
            _ctx.Selection.cancelStack.Pop()?.Invoke();
        _view.TogglePanels(false);
        _view.DeleteSkillBtns();
    }

    public void ShowSkillDescription(int skillIndex)
    {
        var list = DataManager.Instance.CurrentPlayerData.characterStats.combatStats.equippedSkills;
        if (skillIndex < 0 || skillIndex >= list.Count) return;

        string skillName = list[skillIndex];
        string desc = _ctx.Attack.skill.GetDescByName(skillName);
        _view.UpdateSkillText(skillName, desc);
        _ctx.ShowAttackTile(skillName);
    }

    public void CancelSkillDescription()
    {
        _ctx.RemoveAllTileHighlight();
        _ctx.Selection.ActionableTiles.Clear();
        _view.HideSkillTextObject();
    }

    //////////

    public void RefreshTeamPanelsIfNeeded()
    {
        if (_ctx.IsDiffCount())
        {
            BuildTeamStatusPanels();
            RebuildTurnOrderIcons();
        }
    }

    public void BuildTeamStatusPanels()
    {
        _view.ClearTeamPanels();

        int idx = 0;
        for (int i = 0; i < _ctx.Roster.Players.Count; i++)
        {
            var bc = _ctx.Roster.Players[i].GetComponent<BaseCombatant>();
            _view.CreateStatusIcons(idx, bc.characterName, bc.최대체력, bc.현재체력, 120, bc.투지게이지, bc.현재_이동력, bc.현재_행동력, true);
            _view.CreateBuffIcons(true);
            idx++;
        }

        for (int i = 0; i < _ctx.Roster.Enemies.Count; i++)
        {
            var bc = _ctx.Roster.Enemies[i].GetComponent<BaseCombatant>();
            _view.CreateStatusIcons(idx, bc.characterName, bc.최대체력, bc.현재체력, 120, bc.투지게이지, bc.현재_이동력, bc.현재_행동력, false);
            _view.CreateBuffIcons(false);
            idx++;
        }

        _ctx.UpdateEntityCount();
    }

    public void UpdateAllStatus()
    {
        var bc = _ctx.GetAllBC();
        for (int i = 0; i < bc.Count; i++)
        {
            _view.UpdateStatusData(i, bc[i].characterName, bc[i].최대체력, bc[i].현재체력, 120, bc[i].투지게이지, bc[i].현재_이동력, bc[i].현재_행동력);
        }
            
    }

    ////////// Rebuild가 필요한 경우 -> 죽었을 때, 캐릭터가 생길 때,
    /// 죽었을 때 -> 턴이 시작할 때, 공격을 맞았을 때
    /// 캐릭터가 생길 때 -> 배틀이 시작할 때, (턴이 끝나고) 이벤트가 발생할 때,
    

    public void UpdateTurnOrderIconsIfNeeded()
    {
        if (_ctx.Roster.UseUnit == null) return;

        if (_ctx.IsDiffTurn())
        {
            RebuildTurnOrderIcons();
        }
        else
        {
            CheckTurnOrderIcons();
        }
    }

    public void CheckTurnOrderIcons() => _view.UpdateTurnOrderIcon(_ctx.Roster.UseUnit, _ctx.index); 
    
    public void UpdateTurnOrderIcons(GameObject obj, int index, params Color[] colors)
    {
        if (index >= _ctx.Roster.UseUnit.Count) return;
        // _ctx.Roster.UseUnit 이거 넘기면 될 듯?
        //_view.UpdateTurnOrderIcon(_ctx.Roster.UseUnit);
        
        var unit = _ctx.Roster.UseUnit[index]; 
        var bc = unit.GetComponent<BaseCombatant>();
        var img = obj.GetComponent<UnityEngine.UI.Image>();

        if (!unit.activeSelf || bc.현재체력 <= 0)
            img.color = colors[0];
        else if (index == _ctx.index)
            img.color = colors[1];
        else
            img.color = colors[2];
    }

    public void RebuildTurnOrderIcons() // _view에 있어야 하나?
    {
        _view.ClearTurnOrderIcons();

        foreach (var v in _ctx.Roster.UseUnit)
        {
            var icon = _view.CreateTurnOrderIcons();
            var bc = v.GetComponent<BaseCombatant>();
            icon.GetComponentInChildren<TextMeshProUGUI>().text = bc.characterName;
        }

        _ctx.UpdateTurnCount();
        CheckTurnOrderIcons();
    }

    public void IncreaseTurnCount() => _view.UpdateTurnCount(_ctx.totalTurn);

    public void IsChangeUltiGauge()
    {
        if (_ctx.PC != null)
        {
            _view.UpdateUltiGauge(_ctx.PC.투지게이지);
            _view.ChangeUltiButton(!_ctx.CanPlayerUseUltimate());
        }
        else
            _view.ChangeUltiButton(false);

    }

    public void UpdateBuffIcons()
    {
        _view.UpdateBuffIcons(_ctx.Roster.UseUnit);
    }
    ////////
    
    
    
    public void OnMoveButton()
    {
        if (_view.GetButtonInteractable(0) == false) return;
        if (_ctx.NotPlayerTurn()) return;
        if (_view.IsOnMainPanel() == false) return;
        _ctx.이동활성화();
    }

    public void OnAttackButton()
    {
        // 그냥 버튼의 인터렉터블이 false면 작동하지않게 만들기?
        if (_view.GetButtonInteractable(1) == false) return;
        if (CanBehavior() == false) return;

        _ctx.공격활성화();
        _view.TogglePanels(true);
        _view.AddSkillBtns();
    }

    public void OnUltimateButton()
    {
        if (_view.GetButtonInteractable(2) == false) return;
        if (CanBehavior() == false) return;
        Debug.Log("Ultimate button clicked");
    }

    public void OnDefendButton()
    {
        if (_view.GetButtonInteractable(3) == false) return;
        if (CanBehavior() == false) return;
        _ctx.방어활성화();
    }

    public void OnEndTurnButton()
    {
        if (_view.GetButtonInteractable(4) == false) return;
        if (CanBehavior() == false) return;
        _ctx.턴종료활성화();

        while (_ctx.Selection.cancelStack.Count > 0)
            _ctx.Selection.cancelStack.Pop()?.Invoke();
    }

    public void OnPressSkillButton(int skillIndex)
    {
        if (_ctx.NotPlayerTurn()) return;
        if (_view.IsOnSkillPanel() == false) return;

        // 여기가 공격활성화 넣어야할듯?
        _ctx.Selection.attackIndex = skillIndex;
        _ctx.공격활성화();
        
        var list = DataManager.Instance.CurrentPlayerData.characterStats.combatStats.equippedSkills;
        if (skillIndex < 0 || skillIndex >= list.Count) return;

        _view.DeleteSkillBtns();

        //string skillName = list[_ctx.Selection.attackIndex];
        //_ctx.AttackTile(skillName);
        
        _view.TogglePanels(false);
    }

    public void OnWheelIndex(InputAction.CallbackContext context)
    {
        if (_ctx.NotPlayerTurn()) return;

        Vector2 v = context.ReadValue<Vector2>();

        int step = Mathf.RoundToInt(v.y);
        if (Mathf.Abs(step) != 1) return;
        if (!_ctx.Selection.moveTrigger && _ctx.PC != null && _ctx.PC.현재_이동력 > 0)
        {
            int n = Mathf.Max(1, _ctx.Selection.maxMoveIndex);
            _ctx.Selection.moveIndex = ((_ctx.Selection.moveIndex + step) % n + n) % n;
        }
    }

    public void OnWheelClick(InputAction.CallbackContext context)
    {
        if (_ctx.NotPlayerTurn()) return;
        Debug.Log("1");
        _ctx.이동회전활성화();
    }

    public void ChangeAttackIndex(int idx) => _ctx.ChangeAttackIndex(idx);

    private bool CanBehavior()
    {
        if (_ctx.NotPlayerTurn()) return false;
        if (_view.IsOnMainPanel() == false) return false;
        if (_ctx.IsMoveState() == true) return false; // 여기가 문제

        return true;
    }
}