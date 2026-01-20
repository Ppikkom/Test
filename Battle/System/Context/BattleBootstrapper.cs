using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using BattleCore;

public class BattleBootstrapper : MonoBehaviour
{
    [Header("타일 / 맵 관련")]
    public GameObject 발판_프리팹;
    public Vector3 생성_위치;
    public Vector2 타일간격 = new(3, -3);
    int[,] 필드 = { { 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1 } };
    public TextAsset 초기위치;

    [Header("캐릭터 관련")]
    public GameObject[] 플레이어블;
    public GameObject[] 적;

    BattleStateMachine _fsm;
    private BattleContext _ctx;
    public BattleContext BattleContext => _ctx;
    
    private CancellationTokenSource _battleCts;

    public GameObject 화살표;
    public TextMeshProUGUI 행동Text;

    [Header("스킬 관련")]
    public skills 스킬;


    void Awake()
    {
        _ctx = BuildContext();
        _fsm = new BattleStateMachine();
        BuildStates();               
    }

    private void Start()
    {
        StartAsync().Forget();
    }

    private async UniTaskVoid StartAsync()
    {
        _battleCts = new CancellationTokenSource();
        await _fsm.InitAsync(_ctx.States.BattleStart, _battleCts.Token);
    }

    private void Update() => _fsm.Tick();

    private void OnDestroy()
    {
        _battleCts?.Cancel();
        _battleCts?.Dispose();
        _fsm?.Dispose();
    }

    // ⬇️ 반환 타입과 내부 할당 모두 BattleCore 구조에 맞게 수정
    private BattleContext BuildContext()
    {
        var ctx = new BattleContext();

        // 공용 FieldRoot 하나 만들어서 Grid/Highlighter에 같이 연결
        var fieldRoot = new GameObject("Fields");

        // === Grid / 맵 관련 ===
        ctx.Grid.TilePrefab     = 발판_프리팹;
        ctx.Grid.SpawnOrigin    = 생성_위치;
        ctx.Grid.TileSpacing    = 타일간격;
        ctx.Grid.FieldMask      = 필드;
        ctx.Grid.FieldRoot      = fieldRoot;
        ctx.Grid.InitPositionCsv= 초기위치;
        ctx.Grid.MapID = BattleFlowManager.Instance._pendingRequest.Value.fieldId;

        // === Highlighter ===
        ctx.Highlighter.FieldRoot = fieldRoot;

        // === Selection / UI ===
        ctx.Selection.arrowTest     = 화살표;
        ctx.Selection.behavioirText = 행동Text;
        ctx.Selection.MainCam       = Camera.main;
        ctx.InitTrigger(); // 트리거 초기화(원래 메서드명 유지)

        // === 유닛 관련 ===
        var allUnits = 플레이어블.Concat(적).ToList();
        ctx.Roster.UnitDB   = allUnits.ToDictionary(go => go.name, go => go);

        // 시작 시 이벤트 정보 초기화
        ctx.InitSetEvent();

        // 스킬 초기화
        ctx.InitAttackSkill(스킬);

        return ctx;
    }

    private void BuildStates()
    {
        var bStart  = new BattleStartState(_fsm, _ctx);
        var bEnd    = new BattleEndState(_fsm, _ctx);
        var tEnd = new TurnEnd(_fsm, _ctx);
        var tAction = new TurnAction(_fsm, _ctx);
        var pStart = new PlayerStart(_fsm, _ctx);
        var nStart = new NpcStart(_fsm, _ctx);

        _ctx.States = new StateRef
        {
            BattleStart = bStart,
            BattleEnd = bEnd,
            TurnEnd = tEnd,
            TurnAction = tAction,
            NpcStart = nStart,
            PlayerStart = pStart
        };
    }
}

public class StateRef
{
    public IState BattleStart, BattleEnd, TurnEnd, TurnAction, PlayerStart, NpcStart;
}