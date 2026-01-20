using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    #region 인스턴스
    public static EffectManager Instance { get; private set; }

    [SerializeField] private List<BaseEffectSo> _effectSoList;
    private Dictionary<string, BaseEffectSo> _soMap = new();
    private Dictionary<EffectType, IEffectLogic> _logicMap = new();
    private Dictionary<GameObject, List<EffectInstance>> _activeEffect = new();
    
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            Init();
            
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }
    #endregion

    private void Init()
    {
        foreach (var so in _effectSoList)
        {
            if (so != null && !string.IsNullOrEmpty(so.id))
                _soMap[so.id] = so;
        }

        _logicMap = new Dictionary<EffectType, IEffectLogic>()
        {
            { EffectType.Anger, new AngerSo() },
            { EffectType.Defence, new DefenceSo() },
            { EffectType.Enhance, new EnhanceSo() },
            { EffectType.Focus, new FocusSo() },
            { EffectType.InstinctHeightened, new InstinctHeightenedSo() },
            { EffectType.IronDefense, new IronDefenseSo() },
            { EffectType.Regen, new RegenerationSo() },
            { EffectType.SoL, new SoLSo() },
            { EffectType.WildOpen, new WildOpenSo() },
            { EffectType.Berserk, new BerserkSo() },
            { EffectType.Bleeding, new BleedingSo() },
            { EffectType.Lethargy, new BLethargySo() },
            { EffectType.Blindness, new BlindnessSo() },
            { EffectType.Burst, new BurstSo() },
            { EffectType.Fainting, new FaintingSo() },
            { EffectType.Fear, new FearSo() },
            { EffectType.Loss, new LossSo() },
            { EffectType.Stagger, new StaggerSo() },
        };
    }

    public void 효과등록(GameObject obj, string id)
    {
        if (!_soMap.TryGetValue(id, out var so))
        {
            Debug.LogWarning($"존재하지 않는 id : {id}");
            return;
        }

        if (!_activeEffect.ContainsKey(obj))
            _activeEffect[obj] = new List<EffectInstance>();

        var list = _activeEffect[obj];
        var existingInst = list.Find(x => x.Data.Id == id);

        if (existingInst != null)
        {
            existingInst.Data.AddStack();
        }
        else
        {
            if (_logicMap.TryGetValue(so.effectType, out var logic))
            {
                var newData = new EffectData(so);
                var newInst = new EffectInstance(newData, logic);
                list.Add(newInst);
                logic.OnRegister(obj, newInst);
            }
            else Debug.LogError($"{so.effectType}에 해당하는 로직이 없음.");
        }
    }

    public EffectResult 효과사용(GameObject obj, string id)
    {
        var inst = GetInstance(obj, id);
        if (inst == null) return new EffectResult();

        return inst.Logic.OnUse(obj, inst);
    }
    
    public void 효과턴종료(GameObject obj)
    {
        if (!_activeEffect.TryGetValue(obj, out var list)) return;
        
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var inst = list[i];

            // 전략 실행: 턴 종료 로직 (도트 데미지 등)
            inst.Logic.OnTurnEnd(obj, inst);

            // 시간 감소 및 만료 체크
            bool isExpired = inst.Data.TickTurn();
            
            if (isExpired)
            {
                // 전략 실행: 제거 로직
                inst.Logic.OnRemove(obj, inst);
                list.RemoveAt(i);
            }
        }
        if (_activeEffect[obj].Count == 0) _activeEffect.Remove(obj);
    }

    public Sprite[] GetBuffIcons(GameObject obj)
    {
        
        if (_activeEffect.TryGetValue(obj, out var list) == false) return null;
        //Debug.Log(obj.name);
        Sprite[] sprites = new Sprite[list.Count < 6 ? list.Count : 6];
        for (int i = 0; i < list.Count; i++)
        {
            if (i == 6) break; // 6개 초과 표시 x 아니면 다르게?
            EffectData data = list[i].Data;
            if (data.IsContinue)
                sprites[i] = data.BuffIcons[data.CurDuration - 1];
            else if (data.IsStack)
                sprites[i] = data.BuffIcons[data.CurStack - 1];
            else
                sprites[i] = data.BuffIcons[0];
        }

        return sprites;
    }
    
    // Helper
    private EffectInstance GetInstance(GameObject obj, string id)
    {
        if (_activeEffect.TryGetValue(obj, out var list))
        {
            return list.Find(x => x.Data.Id == id);
        }

        return null;
    }
    
    
}

