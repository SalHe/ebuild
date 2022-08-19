using Spectre.Console;

namespace EBuild.Consoles;

public class LinesDisplayer<TTarget, TStatus> : MultiTaskDisplayer<TTarget, TStatus>
{
    public override void SetHeader(string header)
    {
        AnsiConsole.MarkupLine(header);
    }

    public override void Update(TTarget target, TStatus status, string log)
    {
        var content = $"{Markup.Escape("[") + Markup.Escape(TargetName(target)) + Markup.Escape("]")} > {log}";
        AnsiConsole.MarkupLine(string.Format(StatusString(status), content));
    }

    public override Task<TR> StartAsync<TR>(Func<IMultiTaskDisplayer<TTarget, TStatus>, Task<TR>> action)
    {
        AnsiConsole.Profile.Width = int.MaxValue; // 防止长日志自动折行(soft-warp)
        return action(this);
    }
}