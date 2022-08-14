namespace EBuild.Consoles;

public interface IMultiTaskDisplayer<TTarget, TTargetStatus>
{
    public Func<TTarget, string> TargetName { get; set; }
    public void SetHeader(string header);
    public void Update(TTarget target, TTargetStatus status, string log);
    public Task<TR> StartAsync<TR>(Func<IMultiTaskDisplayer<TTarget, TTargetStatus>, Task<TR>> action);

    public virtual Action<TTargetStatus, string> GetUpdater(TTarget target)
    {
        return (status, log) => Update(target, status, log);
    }
}

public abstract class MultiTaskDisplayer<TTarget, TStatus> : IMultiTaskDisplayer<TTarget, TStatus>
{
    private IMultiTaskDisplayer<TTarget, TStatus> _multiTaskDisplayerImplementation;
    public Func<TTarget, string> TargetName { get; set; }
    public Func<TStatus, string> StatusString { get; set; }
    public abstract void SetHeader(string header);
    public abstract void Update(TTarget target, TStatus status, string log);
    public abstract Task<TR> StartAsync<TR>(Func<IMultiTaskDisplayer<TTarget, TStatus>, Task<TR>> action);
}