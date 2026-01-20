using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleCore
{
    /// <summary>
    /// Player/enemy rosters, turn order, current unit selection, and battle-over check.
    /// </summary>
    public class UnitRoster
    {
        public Dictionary<string, GameObject> UnitDB;
        public List<GameObject> Players = new();
        public List<GameObject> Enemies = new();
        public List<GameObject> UseUnit = new();

        private int _index;
        public GameObject CurrentUnit { get; private set; }
        public BaseCombatant CurrentBC { get; private set; }

        public event Action<int, GameObject, BaseCombatant> OnIndexChanged;

        public bool IsBattleOver()
        {
            bool hasPlayer = UseUnit.Any(u => Players.Contains(u));
            bool hasEnemy = UseUnit.Any(u => Enemies.Contains(u));
            return !hasPlayer || !hasEnemy;
        }

        public int Index
        {
            get => _index;
            set
            {
                if (_index == value) return;
                _index = value;
                if (UseUnit == null || UseUnit.Count == 0)
                {
                    CurrentBC = null;
                    OnIndexChanged?.Invoke(_index, null, null);
                    return;
                }
                _index = ((_index % UseUnit.Count) + UseUnit.Count) % UseUnit.Count;
                CurrentUnit = UseUnit[_index];
                CurrentBC = CurrentUnit != null ? CurrentUnit.GetComponent<BaseCombatant>() : null;
                OnIndexChanged?.Invoke(_index, CurrentUnit, CurrentBC);
            }
        }

        public void SortByAgilityDesc()
        {
            foreach (var v in UseUnit)
            {
                var bc = v.GetComponent<BaseCombatant>();
                bc.계산_민첩도 = bc.민첩도_계산();
            }
            UseUnit = UseUnit.OrderByDescending(go => go.GetComponent<BaseCombatant>().계산_민첩도).ToList();
        }

        public void Cleanup()
        {
            if (UseUnit == null || UseUnit.Count == 0)
            {
                CurrentBC = null;
                OnIndexChanged?.Invoke(_index, null, null);
                return;
            }

            int oldIndex = _index;
            int oldCount = UseUnit.Count;

            bool curRemoved = false;
            int removedBefore = 0;

            List<GameObject> newList = new(oldCount);

            for (int i = 0; i < oldCount; i++)
            {
                GameObject u = UseUnit[i];
                bool remove = (u == null) || (!u.activeInHierarchy) ||
                              (u.GetComponent<BaseCombatant>() == null) ||
                              (u.GetComponent<BaseCombatant>().현재체력 <= 0);

                if (remove)
                {
                    if (i < oldIndex) removedBefore++;
                    if (i == oldIndex) curRemoved = true;
                    continue;
                }
                newList.Add(u);
            }

            UseUnit = newList;

            if (UseUnit.Count == 0)
            {
                _index = 0;
                CurrentBC = null;
                OnIndexChanged?.Invoke(_index, null, null);
                return;
            }

            _index = Mathf.Clamp(oldIndex - removedBefore, 0, UseUnit.Count - 1);
            if (curRemoved && _index >= UseUnit.Count) _index = 0;

            RefreshCurrent();
        }

        public void RefreshCurrent()
        {
            if (UseUnit == null || UseUnit.Count == 0)
            {
                CurrentBC = null;
                OnIndexChanged?.Invoke(_index, null, null);
                return;
            }

            _index = ((_index % UseUnit.Count) + UseUnit.Count) % UseUnit.Count;
            CurrentUnit = UseUnit[_index];
            CurrentBC = CurrentUnit != null ? CurrentUnit.GetComponent<BaseCombatant>() : null;
            OnIndexChanged?.Invoke(_index, CurrentUnit, CurrentBC);
        }

        public bool NotPlayerTurn() => CurrentUnit == null || !CurrentUnit.CompareTag("Player");
    }
}