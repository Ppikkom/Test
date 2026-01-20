using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using BattleCore; // 리팩터된 컨텍스트

public abstract class AIBase : MonoBehaviour
{
    protected BattleContext ctx;
    protected INode rootNode;
    protected GameObject target;
    protected BaseCombatant stat;

    [Tooltip("이 유닛이 사용할 수 있는 스킬 이름들 (SkillManager 키)")]
    public string[] skillNames;

    protected readonly List<string> usableSkills = new();

    protected virtual void Awake()
    {
        ctx  = FindAnyObjectByType<BattleBootstrapper>()?.GetCtx();
        stat = GetComponent<BaseCombatant>();
        InitBehaviourTreeAsync().Forget();
    }

    protected virtual void Update() { }

    protected async UniTaskVoid InitBehaviourTreeAsync()
    {
        // 씬 초기화 타이밍 보정
        await UniTask.NextFrame();
        rootNode = BuildBehaviorTree();
    }

    protected abstract INode BuildBehaviorTree();

    public INode GetNode() => rootNode;
    public void ReloadRootNode() => InitBehaviourTreeAsync().Forget();

    // 팀/타겟 및 스킬 탐색 훅
    public abstract void FindNearestTarget();

    protected void RefreshUsableSkills()
    {
        usableSkills.Clear();

        if (skillNames == null || ctx == null || ctx.Attack == null) return;

        foreach (var name in skillNames)
        {
            // ⬇️ 리팩터 후 AttackSystem 경유
            if (ctx.Attack.CanCheckUseSkill(gameObject, name))
                usableSkills.Add(name);
        }
    }

    // 행동 말풍선 출력 헬퍼
    protected void ShowActionText(string msg)
    {
        if (ctx == null || ctx.Selection == null) return;
        var bubble = ctx.Selection.behavioirText;
        if (bubble == null) return;

        if (!bubble.enabled) bubble.enabled = true;

        var pivot = (ctx.curUnit != null) ? ctx.curUnit.transform.position : transform.position;
        bubble.rectTransform.anchoredPosition3D = pivot + Vector3.up * 1.5f;
        bubble.text = msg;
    }
}
