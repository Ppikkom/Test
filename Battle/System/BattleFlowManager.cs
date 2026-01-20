using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class BattleFlowManager : MonoBehaviour
{
    public static BattleFlowManager Instance { get; private set; }
    public BattleRequest? _pendingRequest; 
    private UniTaskCompletionSource<bool> _battleTcs;
    private string _lastScene;
    public bool isBattleEnd = false;
    public bool BattleResult = false;

    [SerializeField] private List<BattleStartSO> allBattle = new List<BattleStartSO>();
    //[SerializeField] private BattleStartSo[] allBattle;
    
    void Awake()
    {
        //allBattle = Resources.LoadAll<BattleStartSo>("Battle").ToList();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }

    // 나중에 이거 호출하면 됨. / 예시는 스크립트 하단 void Start에 적어놓음

    public void StartBattle(string path) => StartBattleAsync(path).Forget();
    public async UniTask<bool> StartBattleAsync(string battlePath)
    {
        isBattleEnd = false; BattleResult = false;
        GameManager.Instance.게임모드_변경(GameMode.Battle);

        if (_pendingRequest.HasValue)
            throw new System.Exception("Battle already pending!");

        //var so = _allBattle.Find(x => x.path == battlePath);
        var so = allBattle.Find(x => x != null && x.path == battlePath);

        if (so == null)
        {
            Debug.LogError($"battlePath : {battlePath}를 못 찾았습니다.");
            return false; 
        }
        _pendingRequest = so.req;
        
        _battleTcs = new UniTaskCompletionSource<bool>();
        _lastScene = so.req.returnScene;
        
        await LoadBattleScene();
        
        return await _battleTcs.Task; 
    }

    public BattleRequest ConsumeRequest()
    {
        if (!_pendingRequest.HasValue)
            throw new System.Exception("No pending battle request!");
        var req = _pendingRequest.Value;
        _pendingRequest = null;
        return req;
    }

    // bool isWin만 받아서 처리
    public async UniTask FinishBattleAsync(bool isWin)
    {
        isBattleEnd = true;
        BattleResult = _battleTcs.TrySetResult(isWin);
        GameManager.Instance.게임모드_변경(GameMode.Battle);

        _battleTcs = null;

        string back = string.IsNullOrEmpty(_pendingRequest?.returnScene)
                        ? _lastScene
                        : _pendingRequest.Value.returnScene;
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(back);
    }

    private async UniTask LoadBattleScene()
    {
        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Battle");
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            await UniTask.Yield();
            if (op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
            }
        }
    }

    void Start()
    {
        
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnBattleStartRequested += StartBattle;
        }
    }

    void OnDestroy()
    {
        // 파괴될 때 구독 해제 (필수)
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnBattleStartRequested -= StartBattle;
        }
    }
}


