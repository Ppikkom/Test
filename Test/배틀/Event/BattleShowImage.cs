using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ShowImage", menuName = "Battle_System/Battle_Event/ShowImage", order = 3)]
public class BattleShowImage : BaseBattleEvent
{
    [SerializeField] private int _turn;
    [SerializeField] private Image _image;
    public int Turn => _turn;
    public Image Image => _image;


    public override async UniTask OccurEvent()
    {
        bUIView bUI = FindAnyObjectByType<bUIView>();

        var battleActionMap = InputSystem.actions.FindActionMap("Battle");
        InputAction clickAction = InputSystem.actions.FindActionMap("BattleEvent")?.FindAction("Click");

        battleActionMap.Disable(); clickAction.Enable();
        // 이미지 소환
        Image obj = Instantiate(_image, GameObject.Find("UICanvas").transform);

        // 클릭 또는 엔터 시 누를 때 까지 대기, 이 후 이미지 제거
        var tcs = new UniTaskCompletionSource();
        System.Action<InputAction.CallbackContext> handler = null;
        handler = (ctx) => { tcs.TrySetResult(); };
        clickAction.performed += handler;

        try { await tcs.Task; }
        finally
        {
            clickAction.performed -= handler;

            Destroy(obj);
            battleActionMap.Enable(); clickAction.Disable();
        }
    }


    public override bool CheckCondition(int turn)
    {
        if (turn == _turn) return true;
        else return false;
    }
}