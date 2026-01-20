using UnityEngine;

public interface INode
{
    public enum STATE { RUN, SUCCESS, FAIL }

    public INode.STATE Evaluate();
}
