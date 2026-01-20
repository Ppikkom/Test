using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "StartStory", menuName = "Battle_System/Battle_Event/StartStory", order = 4)]
public class BattleStory : BaseBattleEvent
{
    [SerializeField] private int _turn;
    public int Turn => _turn;
    [SerializeField] private string _storyName;
    public string StoryName => _storyName;

    private InputActionMap _battle;
    private bUIView _view;
    //private GameObject _dialog;
    private bool _flag;

    public override async UniTask OccurEvent()
    {
        Init();
        await UniTask.WaitForSeconds(1f);
        InkStoryManager.Instance.스토리시작(_storyName);
        await UniTask.WaitUntil(() => _flag == true);
        End();
    }

    void Init()
    {
        _battle = InputSystem.actions.FindActionMap("Battle");
        _battle.Disable();
        GameManager.Instance.게임모드_변경(GameMode.Story);

        _flag = false;

        _view = GameObject.FindAnyObjectByType<bUIView>();
        _view.StartCoroutine(_view.MoveMainBtn(new Vector2(0, -100)));

        //_dialog = GameObject.Find("Dialog");
        InkStoryManager.Instance.스토리종료시 += SetFlag;
        
    }

    void End()
    {
        _battle.Enable();
        GameManager.Instance.게임모드_변경(GameMode.Battle);
        _view.StartCoroutine(_view.MoveMainBtn(Vector2.zero));
        InkStoryManager.Instance.스토리종료시 -= SetFlag;
    }

    public override bool CheckCondition(int turn)
    {
        if (turn == _turn) return true;
        else return false;
    }

    private void SetFlag() => _flag = !_flag;
}
