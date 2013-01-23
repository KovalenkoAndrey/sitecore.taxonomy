namespace Sitecore.Shell.Applications.PageEditor.Taxonomy
{
  using System.Web.UI;

  using Sitecore.Layouts;
  using Sitecore.Layouts.PageExtenders;
  using Sitecore.Web;

  using Sites;

  public class TaxonomyPageExtender : PageExtender
  {
    public override void Insert()
    {
       SiteContext site = Context.Site;
       if (((site != null) && (site.EnableWebEdit && (site.DisplayMode == DisplayMode.Edit))) && (WebUtil.GetQueryString("sc_webedit") != "0"))
       {
         RenderingReference reference = new RenderingReference(new TaxonomyScriptRegister())
           { Placeholder = "webedit", AddToFormIfUnused = true };
         Context.Page.AddRendering(reference);
       }
    }

    public class TaxonomyScriptRegister : Control
    {
      protected override void OnPreRender(System.EventArgs e)
      {
        this.Page.ClientScript.RegisterClientScriptInclude(
          "jQuery.NoConflict", "/sitecore/shell/Controls/Lib/jQuery/jquery.noconflict.js");
        this.Page.ClientScript.RegisterClientScriptInclude(
          "SetTags", "/sitecore/shell/Applications/Taxonomy/Dialogs/SetTags/SetTags.webedit.js");
      }
    }
  }
}