using UnityEngine;

[CreateAssetMenu(fileName = "BattleStartSO", menuName = "Game Data/BattleStartSO")]
public class BattleStartSO : ScriptableObject
{
    public string path;
    public BattleRequest req; 
}


[System.Serializable]
public struct BattleRequest
{
    public string[] playerKeys;
    public string[] enemyKeys;
    public string fieldId;  
    public string returnScene;
}