using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleEventContainer : MonoBehaviour
{
    [SerializeField] private List<BaseBattleEvent> _events;

    public BaseBattleEvent[] FindEventsByName(string s)
    {
        return _events.Where(e => string.Equals(e.BattleName, s, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
}