public class NpcCombatant : BaseCombatant
{
    public override void 행동력_변경(int v)
    {
        현재_행동력 += v;
        //if (현재_행동력 <= 0) 턴종료 = true;
    }
}