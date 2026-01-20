using System;

public class ActionNode : INode
{
    public Func<INode.STATE> action;

    public ActionNode(Func<INode.STATE> _action)
    {
        action = _action;
    }

    public INode.STATE Evaluate()
    {
        return action?.Invoke() ?? INode.STATE.FAIL;
    }
}