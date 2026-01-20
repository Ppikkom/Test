using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class bUIView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainButtonPanel;
    [SerializeField] private GameObject _skillButtonPanel;

    [Header("Main Buttons & UI")]
    [SerializeField] private Button _attackBtn;
    [SerializeField] private Button _defenceBtn;
    [SerializeField] private Button _ultimateBtn;
    [SerializeField] private Button _moveBtn;
    [SerializeField] private Button _turnEndBtn;
    [SerializeField] private Slider ultimateGauge;

    [Header("Input Systems")]
    [SerializeField] private InputActionAsset _inputActionAsset;
    private InputActionMap _battle;
    [SerializeField] private InputActionReference _attackInput;
    [SerializeField] private InputActionReference _defenceInput;
    [SerializeField] private InputActionReference _ultimateInput;
    [SerializeField] private InputActionReference _moveInput;
    [SerializeField] private InputActionReference _turnEndInput;
    [SerializeField] private InputActionReference _previousInput;


    [Header("Attack Buttons")]
    [SerializeField] private Button[] _attackButtons;
    [SerializeField] private TextMeshProUGUI _skillDescriptionText;
    [Header("Move Buttons")]
    [SerializeField] InputActionReference _wheelScroll;
    [SerializeField] InputActionReference _wheelClick;

    [Header("Visuals")]
    [SerializeField] private Color disabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color dTurnOrderIcons = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color cTurnOrderIcons = new Color(1f, 1f, 0.5f, 1f);
    [SerializeField] private Color nTurnOrderIcons = new Color(0.8f, 1f, 0.8f, 1f);

    [Header("Turn-Order")]
    [SerializeField] private TextMeshProUGUI turnCountText;
    [SerializeField] private Transform turnOrderPanel;
    [SerializeField] private GameObject turnOrderIconPrefab;
    private List<GameObject> turnOrderIcons = new();

    [Header("Team Status UI")]
    [SerializeField] private Transform playerTeamPanel;
    [SerializeField] private Transform enemyTeamPanel;
    [SerializeField] private GameObject characterStatusPrefab;
    [SerializeField] private GameObject buffPanelPrefab;
    [SerializeField] private GameObject buffIconPrefab;
    [SerializeField] private Color buffIconDeActiveColor;
    
    private List<GameObject> statusCards = new();
    private List<GameObject> buffCards = new();

    private bUIPresenter _presenter;

    // Test
    private bool _isSkillMenuActive;
    private bool _isInitPanel = false, isStart = false; // 로드되었는 지 확인용
    private System.Action<InputAction.CallbackContext>[] _attackHandler = new System.Action<InputAction.CallbackContext>[5];

    //[HideInInspector] public PlayerCombatant pc;


    void Start()
    {
        var ctx = FindAnyObjectByType<BattleBootstrapper>()?.GetCtx();
        isStart = true;
        _presenter = new bUIPresenter(ctx, this);

        SetUpSkillButtons();
        AddInputAndButton();

        TogglePanels(false);

        // 이거 나중에 presenter의 생성자로 옮기기.
        if (ctx != null) 
        {
            ctx.OnIndexChanged += (_, __, ___) =>
            {
                _presenter.CheckTurnOrderIcons();
                _presenter.IncreaseTurnCount();
                UpdateButtonsInteractive();
            };

            ctx.OnUIChanged += _presenter.UpdateAllStatus;
            ctx.OnBuffUIChanged += _presenter.UpdateBuffIcons;
            ctx.OnTurnCountChanged += _presenter.IncreaseTurnCount;
            ctx.OnTurnOrderChanged += _presenter.UpdateTurnOrderIconsIfNeeded;
            ctx.OnButtonInteractiveChanged += UpdateButtonsInteractive;
            ctx.OnUltiGaugeChanged += _presenter.IsChangeUltiGauge;
            ctx.OnTeamPanelChanged += _presenter.RefreshTeamPanelsIfNeeded;
        }
        
    }

    void Update()
    {
        if (isStart == false) return;

        if (!_isInitPanel && _presenter.HasUnit())
        {
            BuildInitPanels();
        }
    }

    void OnDestroy()
    {
        _battle.Disable();
    }

    private void AddInputAndButton()
    {
        _battle = _inputActionAsset.FindActionMap("Battle"); _battle.Enable();
        _moveInput.action.performed += ctx => _presenter.OnMoveButton();
        _attackInput.action.performed += ctx => _presenter.OnAttackButton();
        _ultimateInput.action.performed += ctx => _presenter.OnUltimateButton();
        _defenceInput.action.performed += ctx => _presenter.OnDefendButton();
        _turnEndInput.action.performed += ctx => _presenter.OnEndTurnButton();
        _previousInput.action.performed += ctx => _presenter.OnPressPrevious();

        _moveBtn.onClick.AddListener(() => _presenter.OnMoveButton());
        _attackBtn.onClick.AddListener(() => _presenter.OnAttackButton());
        _ultimateBtn.onClick.AddListener(() => _presenter.OnUltimateButton());
        _defenceBtn.onClick.AddListener(() => _presenter.OnDefendButton());
        _turnEndBtn.onClick.AddListener(() => _presenter.OnEndTurnButton());

        _wheelScroll.action.performed += _presenter.OnWheelIndex;
        _wheelClick.action.performed += _presenter.OnWheelClick;
    }

    public void AddSkillBtns()
    {
        for (int i = 0; i < 5; i++)
        {
            var index = i;
            _attackHandler[index] = ctx => _presenter.OnPressSkillButton(index);
        }
            
        _moveInput.action.performed += _attackHandler[0];
        _attackInput.action.performed += _attackHandler[1];
        _ultimateInput.action.performed += _attackHandler[2];
        _defenceInput.action.performed += _attackHandler[3];
        _turnEndInput.action.performed += _attackHandler[4];
    }

    public void DeleteSkillBtns()
    {
        _moveInput.action.performed -= _attackHandler[0];
        _attackInput.action.performed -= _attackHandler[1];
        _ultimateInput.action.performed -= _attackHandler[2];
        _defenceInput.action.performed -= _attackHandler[3];
        _turnEndInput.action.performed -= _attackHandler[4];
    }

    public bool IsOnMainPanel() => _mainButtonPanel.activeSelf;
    public bool IsOnSkillPanel() => _skillButtonPanel.activeSelf;

    public void TogglePanels(bool showSkillPanel)
    {
        _isSkillMenuActive = showSkillPanel;
        if (_mainButtonPanel != null) _mainButtonPanel.SetActive(!showSkillPanel);
        if (_skillButtonPanel != null) _skillButtonPanel.SetActive(showSkillPanel);
        if (!showSkillPanel) _presenter.CancelSkillDescription();
    }

    public void HideSkillTextObject()
    {
        if (_skillDescriptionText != null)
            _skillDescriptionText.gameObject.SetActive(false);
    }

    public void UpdateSkillText(string skillName, string desc)
    {
        if (_skillDescriptionText == null) return;

        _skillDescriptionText.text = $"{skillName}: {desc}";
        _skillDescriptionText.gameObject.SetActive(true);
    }

    private void SetUpSkillButtons()
    {
        var equipped = DataManager.Instance.CurrentPlayerData.characterStats.combatStats.equippedSkills;
        int cnt = Mathf.Min(equipped.Count, _attackButtons.Length);

        for (int i = 0; i < _attackButtons.Length; i++)
        {
            var btn = _attackButtons[i];
            bool active = i < cnt;
            btn.interactable = active;

            var cb = btn.colors;
            cb.disabledColor = disabledColor;
            btn.colors = cb;

            var existing = btn.GetComponent<EventTrigger>();
            if (existing != null) Destroy(existing);

            if (!active) continue;

            int skillIndex = i;
            btn.onClick.AddListener(() => _presenter.OnPressSkillButton(skillIndex));

            var trigger = btn.gameObject.AddComponent<EventTrigger>();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => { _presenter.ShowSkillDescription(skillIndex);  _presenter.ChangeAttackIndex(skillIndex);});
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => _presenter.CancelSkillDescription());
            trigger.triggers.Add(exit);
        }
    }

    private void UpdateButtonsInteractive()
    {
        var v = _presenter.GetButtonsCondition();

        SetButtonInteractable(_moveBtn, !v.notPlayer && v.mp > 0);
        SetButtonInteractable(_attackBtn, !v.notPlayer && v.ap > 0);
        SetButtonInteractable(_defenceBtn, !v.notPlayer && v.ap > 0);
        SetButtonInteractable(_turnEndBtn, !v.notPlayer ? true : false);
        SetButtonInteractable(_ultimateBtn, !v.notPlayer && v.ap >= v.maxup); // 이건 나중에 수정해야할듯? 아마도
    }

    private void SetButtonInteractable(Button btn, bool interactable)
    {
        btn.interactable = interactable;
        var cb = btn.colors;
        cb.disabledColor = disabledColor;
        btn.colors = cb;
    }

    public bool GetButtonInteractable(int i) // enum으로 바꿀 수 있긴한데
    {
        if (i == 0) return _moveBtn.interactable;
        else if (i == 1) return _attackBtn.interactable;
        else if (i == 2) return _ultimateBtn.interactable;
        else if (i == 3) return _defenceBtn.interactable;
        else if (i == 4) return _turnEndBtn.interactable;
        else return false;
    }
    ///////

    public void UpdateTurnOrderIcon(List<GameObject> objs, int idx)
    {
        for (int i = 0; i < turnOrderIcons.Count; i++)
        {
            if (i >= objs.Count) continue;
            var icon = turnOrderIcons[i];
            var unit = objs[i];
            var bc = unit.GetComponent<BaseCombatant>();
            var img = icon.GetComponent<Image>();

            if (!unit.activeSelf || bc.현재체력 <= 0)
                img.color = dTurnOrderIcons;
            else if (i == idx)
                img.color = cTurnOrderIcons;
            else
                img.color = nTurnOrderIcons;
        }
    }

    public void ClearTurnOrderIcons()
    {
        foreach (Transform child in turnOrderPanel)
            Destroy(child.gameObject);
        turnOrderIcons.Clear();
    }

    public GameObject CreateTurnOrderIcons()
    {
        var icon = Instantiate(turnOrderIconPrefab, turnOrderPanel);
        turnOrderIcons.Add(icon);
        return icon;
    }

    public void UpdateTurnCount(int turn) => turnCountText.text = $"Turn: {turn}";

    /////

    public void ClearTeamPanels()
    {
        foreach (Transform c in playerTeamPanel) Destroy(c.gameObject);
        foreach (Transform c in enemyTeamPanel) Destroy(c.gameObject);
        statusCards.Clear();
        buffCards.Clear();
    }

    public void CreateStatusIcons(int idx, string characterName, float maxHealth, float currentHealth, float maxWill, float currentWill, int movePoints, int attackPoints, bool isP = true)
    {
        GameObject obj = isP == true ? Instantiate(characterStatusPrefab, playerTeamPanel) : Instantiate(characterStatusPrefab, enemyTeamPanel);
        statusCards.Add(obj);
        UpdateStatusData(idx, characterName, maxHealth, currentHealth, maxWill, currentWill, movePoints, attackPoints);
    }

    public void UpdateStatusData(int i, string characterName, float maxHealth, float currentHealth, float maxWill, float currentWill, int movePoints, int attackPoints)
    {
        if (statusCards.Count == 0) return;
        
        var nameText = statusCards[i].transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var healthSlider = statusCards[i].transform.Find("HealthBar").GetComponent<Slider>();
        var willSlider = statusCards[i].transform.Find("WillBar").GetComponent<Slider>();
        var moveText = statusCards[i].transform.Find("MovePoints").GetComponent<TextMeshProUGUI>();
        var attackText = statusCards[i].transform.Find("AttackPoints").GetComponent<TextMeshProUGUI>();

        nameText.text = characterName;
        healthSlider.maxValue = maxHealth; healthSlider.value = currentHealth;
        willSlider.maxValue = maxWill; willSlider.value = currentWill;
        moveText.text = $"Move: {movePoints}";
        attackText.text = $"Attack: {attackPoints}";
    }

    public void CreateBuffIcons(bool isP = true) // 플레이어 정보 넣고 바로 밑 업데이트에 달아줘야하나?
    {
        GameObject obj = isP == true ? Instantiate(buffPanelPrefab, playerTeamPanel) : Instantiate(buffPanelPrefab, enemyTeamPanel);
        buffCards.Add(obj);
    }

    public void UpdateBuffIcons(List<GameObject> obj)
    {
        if (buffCards.Count == 0) return;
        
        // 그냥 전체 업데이트
        
        //GameObject buffPanel = buffCards[idx];

        for (int i = 0; i < obj.Count; i++)
        {
            GameObject buffPanel = buffCards[i];
            List<Image> buffIcons = buffPanel.GetComponentsInChildren<Image>().ToList();
            buffIcons.RemoveAt(0);
            Sprite[] sprites = EffectManager.Instance.GetBuffIcons(obj[i]);
            
            if (sprites == null)
            {
                foreach (var v in buffIcons)
                {
                    v.sprite = null;
                    v.color = new Color(1, 1, 1, 0);
                }

                return;
            }
        
            for (int j = 0; j < 5; j++)
            {
                if (j >= sprites.Length)
                {
                    buffIcons[j].sprite = null;
                    buffIcons[j].color = new Color(1, 1, 1, 0);
                }
                else
                {
                    buffIcons[j].sprite = sprites[i];
                    buffIcons[j].color = new Color(1, 1, 1, 1);
                }
            }
        }
    }

    public void BuildInitPanels()
    {
        _isInitPanel = true;
        //pc = _presenter.GetPlayerCombatant(); // view에다가 pc 이거 별로일거같은데

        _presenter.BuildTeamStatusPanels();
        _presenter.RebuildTurnOrderIcons();
        SetUpSkillButtons();
    }

    public void UpdateUltiGauge(int v) => ultimateGauge.value = v;
    public void ChangeUltiButton(bool b) => _ultimateBtn.interactable = b;

    // Test Functions
    public IEnumerator MoveMainBtn(Vector2 pos, int spd = 1)
    {

        var rect = _mainButtonPanel.GetComponent<RectTransform>();
        var startPos = rect.anchoredPosition;
        var endPos = pos;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * spd;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        rect.anchoredPosition = endPos;
    }

}