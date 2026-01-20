using UnityEngine;

public enum BattleBuffType { None, 방어, 광속, 고양, 철벽, 집중, 분노, 재생 }
public enum BattleDeBuffType {None, 경직, Lethargy, 광폭, 기절, 파열, 실명, 출혈, 상실}

[CreateAssetMenu(fileName = "test Skill", menuName = "Game Data/TestSkillSO")]
public class skillso : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string _name; public string 이름 => _name;
    [SerializeField] private int _ID; public int ID => _ID;
    [SerializeField] private MartialArtType _artType; public MartialArtType 무술타입 => _artType;
    [SerializeField] private BattleBuffType _buffType; public BattleBuffType 버프 => _buffType;
    [SerializeField] private BattleDeBuffType _deBuffType; public BattleDeBuffType 디버프 => _deBuffType;
    [SerializeField] private int _attack; public int 위력 => _attack;
    
    [SerializeField] private int _usePoint; public int 소모_행동력 => _usePoint;
    [SerializeField] private int _nextMove; public int 다음_이동력 => _nextMove;
    [SerializeField] private int _nextPoint; public int 다음_행동력 => _nextPoint;
    [SerializeField] System.Collections.Generic.List<Vector2> _skillRange;
    public System.Collections.Generic.List<Vector2> 스킬범위 => _skillRange;

    [Header("부가 정보")]
    [SerializeField] private BattleDeBuffType _bonusDebuff; public BattleDeBuffType 디버프_검사 => _bonusDebuff;
    [SerializeField] private float _bonus; public float 보너스배수 => _bonus;

    [Header("설명"), TextArea]
    public string description;
    public Sprite icon;

    public float 스킬보너스(GameObject 피해자)
    {
        if (BattleTagSystem.Instance.태그확인(피해자, 디버프_검사.ToString(), true) == true) return _bonus;
        return 0f;
    }
}