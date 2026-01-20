using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class BattleTagContainer : MonoBehaviour
 {
     public HashSet<BattleTagDefinition> Tags = new HashSet<BattleTagDefinition>();
 
     public void Add(BattleTagDefinition battleTag) => Tags.Add(battleTag);
     public void Remove(BattleTagDefinition battleTag) => Tags.Remove(battleTag);
 
     public bool HasTag(BattleTagDefinition battleTag) => Tags.Contains(battleTag);
     public bool HasTag(string tagName) => Tags.Where(t => t != null).Any(t => t.FullPath.Equals(tagName));
 
     public bool HasTagPrefix(string prefix) => Tags.Where(t => t != null).Any(t => t.FullPath.StartsWith(prefix + ".") || t.FullPath == prefix);
 
     public bool HasAny(params string[] prefixes) => prefixes.Any(p => HasTagPrefix(p));
 }