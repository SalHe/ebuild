using Spectre.Console;

namespace EBuild.Consoles;

public class TableDisplayer<TTarget, TTargetStatus> : MultiTaskDisplayer<TTarget, TTargetStatus>
{
    private readonly IList<TTarget> _targets;
    private LiveDisplayContext _ctx;
    private Table _table;

    public TableDisplayer(IList<TTarget> targets)
    {
        _targets = targets;
    }

    public override async Task<TR> StartAsync<TR>(Func<IMultiTaskDisplayer<TTarget, TTargetStatus>, Task<TR>> action)
    {
        _table = new Table();
        _table.AddColumns("状态", "目标", "日志");
        foreach (var target in _targets)
            _table.AddRow("", Markup.Escape(TargetName(target)), "");

        TR? res = default;
        await AnsiConsole.Live(_table)
            .StartAsync(async ctx =>
            {
                _ctx = ctx;
                res = await action(this);
            });
        return res;
    }

    public override void SetHeader(string header)
    {
        _table.Title(header);
        _ctx.Refresh();
    }

    public override void Update(TTarget target, TTargetStatus status, string log)
    {
        int id = _targets.IndexOf(target);
        _table.UpdateCell(id, 0, StatusString(status));
        _table.UpdateCell(id, 2, log);
        _ctx.Refresh();
    }
}