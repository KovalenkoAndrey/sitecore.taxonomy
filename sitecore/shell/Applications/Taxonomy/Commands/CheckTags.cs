namespace Sitecore.Shell.Applications.Taxonomy.Commands
{
  using Sitecore.Shell.Framework.Commands;

  public class CheckTags : SelectTags
  {
    public override void Execute(CommandContext context)
    {
      context.Parameters["mode"] = "check";
      base.Execute(context);
    }
  }
}