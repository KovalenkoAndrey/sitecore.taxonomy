namespace Sitecore.Taxonomies.Layouts.PageExtenders
{
  using Sitecore.Layouts.PageExtenders;
  using Sitecore.Sites;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Layouts;

  public class ClassificationPageExtender : PageExtender
  {
    public override void Insert()
    {
      SiteContext siteContext = Context.Site;

      if (((siteContext != null) && (siteContext.EnableWebEdit && (siteContext.DisplayMode == DisplayMode.Edit))) && (WebUtil.GetQueryString("sc_webedit") != "0"))
      {
        UserControlRenderer editControl = new UserControlRenderer { Src = "/sitecore/shell/Applications/Taxonomy/Controls/PageEditor/TaxomomiesRendering.ascx" };

        RenderingReference menuRenderingReference = new RenderingReference(editControl) { Placeholder = "webedit", AddToFormIfUnused = true };
        Context.Page.AddRendering(menuRenderingReference);
      }
    }
  }
}