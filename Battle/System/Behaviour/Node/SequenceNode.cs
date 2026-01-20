using System;
using System.Collections.Generic;

public class SequenceNode : INode
{
    List<INode> children;

    public SequenceNode() { children = new(); }

    public void Add(INode node) { children.Add(node); }

    public INode.STATE Evaluate()
    {
        if (children.Count <= 0)
            return INode.STATE.FAIL;
        
        foreach (INode child in children)
        {
            INode.STATE state = child.Evaluate();
            switch (state)
            {
                case INode.STATE.RUN:
                    return INode.STATE.RUN;
                case INode.STATE.SUCCESS:
                    continue;
                case INode.STATE.FAIL:
                    return INode.STATE.FAIL;
            }
        }
        return INode.STATE.SUCCESS;
    }
}