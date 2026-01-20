using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 유닛의 공통 스탯과 계산 로직.
/// - 외부 코드와의 호환을 위해 기존 공개 필드/메서드 시그니처 유지
/// - 안전 가드/초기화 일관성/할당 최소화를 중심으로 정리
/// </summary>
public abstract class BaseCombatant : MonoBehaviour
{
    [Header("Identity / Discipline")]
    public string characterName;
    public MartialArtType mainArt;

    [Header("Initial Stats (ScriptableObject)")]
    [SerializeField] private CharacterStatsSo objInitialStats;

    [Header("Runtime Stats (Containers)")]
    [HideInInspector] public BasicStats basicStats = new BasicStats();
    [HideInInspector] public PhysicalStats physicalStats = new PhysicalStats();
    [HideInInspector] public CombatStats combatStats = new CombatStats();

    // 초기 AP/MP 매핑 (없으면 기본값 1,1)
    private static readonly Dictionary<MartialArtType, (int ap, int mp)> s_InitPoints = new()
    {
        { MartialArtType.무에타이, (1,5) }, { MartialArtType.레슬링, (1,1) }, { MartialArtType.복싱, (2,3) },
        { MartialArtType.주짓수,  (1,3) }, { MartialArtType.쿵푸,   (2,2) }, { MartialArtType.크라브마가,(1,1) },
        { MartialArtType.태권도,  (1,1) },
    };

    // 공격/방어 근거 근육(할당 최소화를 위해 배열 캐시)
    private static readonly Dictionary<MartialArtType, MuscleType[]> s_AttackMuscles = new()
    {
        { MartialArtType.복싱,     new[]{ MuscleType.어깨, MuscleType.이두근, MuscleType.가슴 } },
        { MartialArtType.무에타이, new[]{ MuscleType.대퇴사두근, MuscleType.삼두근, MuscleType.가슴 } },
        { MartialArtType.레슬링,   new[]{ MuscleType.대퇴사두근 } },
        { MartialArtType.주짓수,   new[]{ MuscleType.이두근 } },
        { MartialArtType.쿵푸,     new[]{ MuscleType.삼두근, MuscleType.이두근 } },
        { MartialArtType.크라브마가,new[]{ MuscleType.삼두근, MuscleType.가슴 } },
        { MartialArtType.태권도,   new[]{ MuscleType.대퇴사두근, MuscleType.가슴 } },
    };

    private static readonly Dictionary<MartialArtType, MuscleType[]> s_DefenseMuscles = new()
    {
        { MartialArtType.복싱,     new[]{ MuscleType.전완근 } },
        { MartialArtType.주짓수,   new[]{ MuscleType.전완근, MuscleType.햄스트링, MuscleType.비복근 } },
        { MartialArtType.레슬링,   new[]{ MuscleType.비복근, MuscleType.햄스트링, MuscleType.등 } },
        { MartialArtType.무에타이, new[]{ MuscleType.햄스트링 } },
        { MartialArtType.쿵푸,     new[]{ MuscleType.전완근, MuscleType.등 } },
        { MartialArtType.크라브마가,new[]{ MuscleType.비복근, MuscleType.복부 } },
        { MartialArtType.태권도,   new[]{ MuscleType.햄스트링, MuscleType.등 } },
    };

    // 공개 전투 스탯 (호환 유지)
    public int 최대체력, 현재체력;
    public int 공격력, 방어력, 민첩도, 계산_민첩도;
    public float 회피율, 명중률, 치명타율;

    public int 현재_행동력, 다음_행동력, 추가_행동력;
    public int 현재_이동력, 다음_이동력, 추가_이동력;
    public int 투지게이지;
    public bool 턴종료;
    
    protected virtual void Awake() { }

    // --- 전투 시작 시 초기화 ---
    public virtual void BattleStart()
    {
        if (objInitialStats == null)
        {
            Debug.LogWarning($"{name}: CharacterStatsSo 가 지정되지 않았습니다.");
            objInitialStats = ScriptableObject.CreateInstance<CharacterStatsSo>();
        }

        characterName = string.IsNullOrEmpty(objInitialStats.characterName) ? characterName : objInitialStats.characterName;

        // 기본 능력치 초기화
        basicStats.최대체력      = objInitialStats.initialMaxHealth;
        basicStats.현재체력          = objInitialStats.initialHealth;
        basicStats.최대기력      = objInitialStats.initialMaxStamina;
        basicStats.현재기력          = objInitialStats.initialStamina;
        basicStats.현재멘탈          = objInitialStats.initialMental;
        basicStats.코어          = objInitialStats.initialCore;
        basicStats.직감          = objInitialStats.initialIntuition;
        basicStats.민첩          = objInitialStats.initialAgility;
        basicStats.지능          = objInitialStats.initialIntelligence;

        // 세부 능력치 컨테이너 채우기
        
        foreach (var kv in objInitialStats.initialMuscleStats)
        {
            physicalStats.muscleStats[kv.Key] = kv.Value;
        }
        foreach (var kv in objInitialStats.initialMartialArtStats)
        {
            combatStats.martialArtStats[kv.Key] = kv.Value;
        }
        
        
        최대체력   = Mathf.Max(1, basicStats.최대체력);
        현재체력   = Mathf.Clamp(basicStats.현재체력, 0, 최대체력);
        투지게이지 = Mathf.Max(0, basicStats.현재멘탈);

        공격력  = CalculateAttackPower();
        방어력  = CalculateDefensePower();
        민첩도  = basicStats.민첩;

        // 회피/치명 수식은 기존 로직 유지(0~100 범위로 클램프)
        회피율   = Mathf.Clamp(Mathf.Round((3.345f + 48.107f * (1f - Mathf.Pow((float)System.Math.E, -0.0035f * 민첩도))) * 100f) / 100f, 0f, 100f);
        치명타율 = Mathf.Clamp(Mathf.Round((3.345f + 48.107f * (1f - Mathf.Pow((float)System.Math.E, -0.0035f * basicStats.지능))) * 100f) / 100f, 0f, 100f);
        명중률   = 100f;

        InitPointChange(); // 초기 포인트 설정
    }

    // --- 턴 포인트 초기화/적용 ---
    private void InitPointChange()
    {
        턴종료 = false;
        var (ap, mp) = GetInitialPoints(mainArt);
        다음_행동력 = Mathf.Max(0, ap);
        다음_이동력 = Mathf.Max(0, mp);
        추가_행동력 = 0;
        추가_이동력 = 0;
    }

    public void PointChange()
    {
        턴종료 = false;
        현재_행동력 = Mathf.Max(0, 다음_행동력 + 추가_행동력);
        현재_이동력 = Mathf.Max(0, 다음_이동력 + 추가_이동력);
        다음_행동력 = 추가_행동력 = 0;
        다음_이동력 = 추가_이동력 = 0;
    }

    public virtual void 행동력_변경(int v)
    {
        int before = 현재_행동력;
        현재_행동력 = Mathf.Max(0, 현재_행동력 + v);
        if (현재_행동력 <= 0 && before > 0) 턴종료 = true; // 0 이하가 되면 턴 종료, Player는 사용안함
    }

    public void 이동력_변경(int v)
    {
        현재_이동력 = Mathf.Max(0, 현재_이동력 + v);
    }

    public bool 행동력_유무() => 현재_행동력 > 0;
    public bool 이동력_유무() => 현재_이동력 > 0;

    // --- 전투 계산 ---
    private float 투지버프(float v)
    {
        // Tag/Effect 시스템과의 결합은 유지 (널가드 포함)
        if (BattleTagSystem.Instance && BattleTagSystem.Instance.태그확인(gameObject, "Buff.InstinctHeightened")) v *= 1.1f;
        else if (BattleTagSystem.Instance && BattleTagSystem.Instance.태그확인(gameObject, "Buff.WildOpen")) v *= 1.2f;
        else if (BattleTagSystem.Instance && BattleTagSystem.Instance.태그확인(gameObject, "Debuff.Fear")) v *= 0.8f;
        return v;
    }

    public int 민첩도_계산() => Mathf.RoundToInt(투지버프(Mathf.Max(0, 민첩도)));

    public void 체력변경(int v) => 현재체력 = Mathf.Clamp(현재체력 + v, 0, Mathf.Max(1, 최대체력));

    // --- 유틸 ---
    private (int, int) GetInitialPoints(MartialArtType e)
        => s_InitPoints.TryGetValue(e, out var p) ? p : (1, 1);

    public int CalculateAttackPower()
    {
        if (!s_AttackMuscles.TryGetValue(mainArt, out var muscles) || muscles == null)
            return 0;
        int sum = 0;
        for (int i = 0; i < muscles.Length; i++)
            sum += physicalStats.GetStat(muscles[i]);
        return Mathf.Max(0, sum);
    }

    public int CalculateDefensePower()
    {
        if (!s_DefenseMuscles.TryGetValue(mainArt, out var muscles) || muscles == null)
            return 0;
        int sum = 0;
        for (int i = 0; i < muscles.Length; i++)
            sum += physicalStats.GetStat(muscles[i]);
        return Mathf.Max(0, sum);
    }
}