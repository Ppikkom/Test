using System.Threading;
using Cysharp.Threading.Tasks;
using BattleCore;

public abstract class StateBase : IState
{
    protected readonly BattleStateMachine fsm;
    protected readonly BattleContext ctx;
    protected CancellationTokenSource localCts;

    protected StateBase(BattleStateMachine fsm, BattleContext ctx)
    {
        this.fsm = fsm;
        this.ctx = ctx;
    }

    public virtual UniTask OnEnter(CancellationToken ct)
    {
        localCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        return UniTask.CompletedTask;
    }

    public virtual UniTaskVoid OnUpdate() => default;

    public virtual UniTask OnExit()
    {
        localCts?.Cancel();
        localCts?.Dispose();
        localCts = null;
        return UniTask.CompletedTask;
    }

    protected bool IsCanceled => localCts == null || localCts.IsCancellationRequested;
}