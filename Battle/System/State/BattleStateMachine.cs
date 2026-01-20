using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleStateMachine
{
    public IState Current { get; private set; }
    CancellationTokenSource _cts = new();

    public async UniTask InitAsync(IState initial, CancellationToken ct)
    {
        Current = initial;
        await Current.OnEnter(ct);
    }

    public async UniTask ChangeState(IState next)
    {
        if (Current != null)
            await Current.OnExit();

        Current = next;
        await Current.OnEnter(_cts.Token);
    }

    public void Tick() => Current?.OnUpdate().Forget();

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public interface IState
{
    UniTask OnEnter(CancellationToken ct);
    UniTaskVoid OnUpdate();
    UniTask OnExit();
}