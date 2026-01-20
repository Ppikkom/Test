using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using BattleCore;
public class TurnAction : StateBase
{
    public TurnAction(BattleStateMachine _fsm, BattleContext _ctx) : base(_fsm, _ctx) { }

    private string _str;
    private Animator _anim;
    private int _animHash = -1;
    private float _timer = 0;
    private Vector3 _startPos;

    public override async UniTask OnEnter(CancellationToken ct)
    {
        varInit();
        await base.OnEnter(ct);
    }


    public override async UniTaskVoid OnUpdate()
    {
        var st = _anim.GetCurrentAnimatorStateInfo(0);
        bool isAnimState = st.shortNameHash == _animHash && !_anim.IsInTransition(0);
        if (!isAnimState) return;
        
        if (_str == "Move")
        {
            _timer = Mathf.Min(_timer + Time.deltaTime, 1);
            ctx.curUnit.transform.position = Vector3.Lerp(_startPos, ctx.GridToWorld(ctx.MovePos), _timer);

            if (_timer >= 1f)
            {
                ctx.curUnit.transform.position = ctx.GridToWorld(ctx.MovePos);
                await TurnCheck();
                return;
            }
        }

        if (_str != "Move" && st.normalizedTime >= 1f)
        {
            if (ctx.NotPlayerTurn() == false) // Player
            {
                var skills = DataManager.Instance.CurrentPlayerData.characterStats.combatStats.equippedSkills;
                string skillName = skills[ctx.Selection.attackIndex]; // 이거보다 ctx.useSkill (string)로 받아놓는게 낳으려나.
                ctx.AttackTile(skillName);
            }
            else // Other
            {
                ctx.AttackTile(ctx.RBehaviour);
            }
            
            ctx.CleanupUseUnitList();
            await TurnCheck();
            return;
        }
    }


    public override UniTask OnExit()
    {
        ctx.OnUIChanged?.Invoke();
        ctx.OnBuffUIChanged?.Invoke();
        _anim.SetTrigger("ActionEnd");
        return base.OnExit();
    }

    void varInit()
    {
        _str = ctx.RBehaviour;
        Debug.Log($"{_str}");
        _anim = ctx.curUnit.GetComponentInChildren<Animator>();
        _animHash = Animator.StringToHash(_str);
        _anim.SetTrigger(_str);
        _timer = 0;
        _startPos = ctx.curUnit.transform.position;
    }

    private UniTask TurnCheck()
    {
        if (ctx.Roster.Players.Contains(ctx.curUnit))
            return fsm.ChangeState(ctx.States.PlayerStart);
        else
            return fsm.ChangeState(ctx.States.NpcStart);
    }
}
