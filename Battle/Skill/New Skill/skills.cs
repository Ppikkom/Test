using UnityEngine;
using System.Collections.Generic;


public class skills : MonoBehaviour
{
    [SerializeField] private List<skillso> _skills = new List<skillso>();

    public skillso GetSkillByName(string str) => _skills.Find(skill => skill.이름 == str);
    public skillso GetSkillByID(int id) => _skills.Find(skill => skill.ID == id);
    public List<Vector2> GetSkillRangeByName(string str) => _skills.Find(skill => skill.이름 == str).스킬범위;
    public int GetCostByName(string str) => str == null ? int.MaxValue : _skills.Find(skill => skill.이름 == str).소모_행동력;
    public int GetNextMovByName(string str) => _skills.Find(skill => skill.이름 == str).다음_이동력;
    public int GetNextActByName(string str) => _skills.Find(skill => skill.이름 == str).다음_행동력;
    public string GetDescByName(string str) => _skills.Find(skill => skill.이름 == str).description;

    public void 스킬사용(GameObject 가해자, string n, GameObject 피해자, Vector2 위치)
    {
        BaseCombatant 가해자_BC = 가해자.GetComponent<BaseCombatant>();
        BaseCombatant 피해자_BC = 피해자.GetComponent<BaseCombatant>();
        skillso 스킬 = GetSkillByName(n);

        가해자_BC.행동력_변경(-스킬.소모_행동력);
        
        if (스킬 == null) { Debug.LogError("스킬이 등록되지 않았습니다."); return; }
        
        int cAtk = 공격력계산(피해자, 가해자_BC.공격력, 위치, 스킬); // calculatedAtk
        int cDef = 방어력계산(피해자, 피해자_BC.방어력, 위치);
        float cAccuracy = 명중률계산(가해자, 가해자_BC.명중률);
        float cAgillty = 회피율계산(피해자, 피해자_BC.회피율);
        float cCritical = 치명타율계산(가해자, 가해자_BC.치명타율);
        
        Debug.Log($"{n} 스킬 사용함요");
        
        // 명중 및 회피
        if (Random.Range(0f, 100f) > cAccuracy) { Debug.Log($"{피해자.name} 명중 실패."); return; }
        if (Random.Range(0f, 100f) <= cAgillty) { Debug.Log($"{피해자.name} 회피 성공."); return; }

        // 데미지 계산
        var result = Mathf.Clamp(cAtk - cDef, 1, int.MaxValue);
        if (Random.Range(0f, 100f) <= cCritical) { Debug.Log($"{가해자.name} 치명타 공격."); result = (int)(result * 1.5f); }
        Debug.Log($"총 {result}의 피해를 입힘.");
        피해자_BC.체력변경(-result);
        
        // 피격 오브젝트 회전 처리
        if (Mathf.Abs(가해자.transform.position.x - 피해자.transform.position.x) > 0.01f && Mathf.Abs(가해자.transform.position.y - 피해자.transform.position.y) < 0.01f) // 오차
        {
            Vector3 toAttacker = 가해자.transform.position - 피해자.transform.position;
            toAttacker.y = 0; float yawOffset = -90f;
            피해자.transform.rotation = Quaternion.LookRotation(toAttacker.normalized, Vector3.up) * Quaternion.Euler(0f, yawOffset, 0f);
        }
        
        // 버프 혹은 디버프 부여
        if (스킬.버프 != BattleBuffType.None)
        {
            //TagManager.Instance.태그적용(가해자, 스킬.버프.ToString());
            EffectManager.Instance.효과등록(가해자, 스킬.버프.ToString());
        }
        if (스킬.디버프 != BattleDeBuffType.None)
        {

            //TagManager.Instance.태그적용(피해자, 스킬.디버프.ToString());
            EffectManager.Instance.효과등록(피해자, 스킬.디버프.ToString());
        }

        if (피해자.GetComponent<BaseCombatant>().현재체력 <= 0) 피해자.SetActive(false);
    }
    
    private bool HasSkill(string n)
    {
        if (string.IsNullOrEmpty(n)) return false;
        foreach (var v in _skills) if (v.이름 == n) return true;
        return false;
    }
    
    // 스탯 계산
    public int 공격력계산(BaseCombatant offender, BaseCombatant victim, Vector2 pos, string str) =>
        공격력계산(victim.gameObject, offender.공격력, pos, GetSkillByName(str));
    
    private int 공격력계산(GameObject victim, int value, Vector2 pos, skillso 스킬) // GameObject offender,
    {
        float magnification = 0f;
        
        if (pos.x > 0) magnification += 1f;            // 정면
        else if (pos.y == 0 && pos.x < 0) magnification += 1.5f; // 후면
        else magnification += 1.2f; // 측면
        
        if (BattleTagSystem.Instance.태그확인(gameObject, "Buff.InstinctHeightened")) magnification += 0.1f;
        else if (BattleTagSystem.Instance.태그확인(gameObject, "Buff.WildOpen")) magnification += 0.2f;
        else if (BattleTagSystem.Instance.태그확인(gameObject, "Debuff.Fear")) magnification -= 0.2f;
        
        magnification += 스킬.스킬보너스(victim);
        
        return (int)(value * magnification);
    }

    private int 방어력계산(GameObject victim ,int value, Vector2 pos)
    {
        float magnification = 1f;
        if (BattleTagSystem.Instance.태그확인(victim, "Buff.Defence")) magnification += 0.2f;
        if (BattleTagSystem.Instance.태그확인(victim, "Buff.IronDefence")) magnification += 0.3f;
        if (BattleTagSystem.Instance.태그확인(victim, "Debuff.Burst")) magnification -= 0.1f;
        
        if (pos.x > 1) magnification += 0.2f; // 마주보고 있을 때. pos.x > 0 인지 1인지 확인 필요.

        return (int)(value * magnification);
    }
    
    private float 명중률계산(GameObject obj ,float value)
    {
        float magnification = 1f;
        if (BattleTagSystem.Instance.태그확인(obj, "Debuff.Berserk"))  magnification -= 0.5f;
        if (BattleTagSystem.Instance.태그확인(obj, "Debuff.Blindness"))  magnification -= 0.3f;

        return (value * magnification);
    }

    private float 회피율계산(GameObject obj, float value)
    {
        float magnification = 1f;
        if (BattleTagSystem.Instance.태그확인(obj, "Buff.IronDefense")) magnification += 0.1f;
        
        return (value * magnification);
    }

    private float 치명타율계산(GameObject obj, float value)
    {
        float magnification = 1f;
        if (BattleTagSystem.Instance.태그확인(obj, "Buff.Focus")) magnification += 0.3f;
        
        return (value * magnification);
    }
}