namespace EBuild.Project.Cleaners;

public abstract class OptionalProjectCleaner : ProjectCleaner
{
    public override bool Optional => true;
}