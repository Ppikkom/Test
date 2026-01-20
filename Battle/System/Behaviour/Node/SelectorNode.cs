using System;
using System.Collections.Generic;

public class SelectorNode : INode
{
    List<INode> children;

    public SelectorNode() { children = new(); }

    public void Add(INode node) { children.Add(node); }

    public INode.STATE Evaluate()
    {
        foreach (INode child in children)
        {
            INode.STATE state = child.Evaluate();
            switch (state)
            {
                case INode.STATE.SUCCESS:
                    return INode.STATE.SUCCESS;
                case INode.STATE.RUN:
                    return INode.STATE.RUN;
            }
        }
        return INode.STATE.FAIL;
    }
}