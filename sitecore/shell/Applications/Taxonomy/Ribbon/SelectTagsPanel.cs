namespace Sitecore.Shell.Taxonomy.Ribbon
{
  using System.Web.UI;
  using System.Web.UI.WebControls;

  using Framework.Commands;

  using Sitecore.Data.Items;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.WebControls;

  using Web.UI.WebControls;

  using Control = Sitecore.Web.UI.HtmlControls.Control;

  public class SelectTagsPanel : CustomControl
  {
    public SelectTagsPanel(Item button, CommandContext context)
      : base(button, context)
    {
    }

    protected override void Render(System.Web.UI.HtmlTextWriter writer)
    {
      //"SelectTagsPanel", "/sitecore/shell/Applications/Taxonomy/Ribbon/SelectTagsPanel.js"
      //ScriptFile tagsBoxScript = new ScriptFile();
      //tagsBoxScript.Type = "text/javascript";
      //tagsBoxScript.Key = Control.GetUniqueID("TagsBoxScript");
      //tagsBoxScript.Src = "/sitecore/shell/Applications/Taxonomy/Ribbon/SelectTagsPanel.js";
      //var currentScriptManager = AjaxScriptManager.Current;
      //if (currentScriptManager!=null)
      //{
      //  currentScriptManager.PageScriptManager.ScriptFiles.Add(tagsBoxScript);
      //}
      //else
      //{
      //  Sitecore.Context.ClientPage.PageScriptManager.ScriptFiles.Add(tagsBoxScript);
      //}
      //writer.WriteBeginTag("script");
      //writer.WriteAttribute("type","text/javascript");
      //writer.WriteAttribute("key", "TagsBoxScript");
      //writer.WriteAttribute("src", "/sitecore/shell/Applications/Taxonomy/Ribbon/SelectTagsPanel.js");
      //writer.WriteEndTag("script");
      //writer.Write(
      //  "<script type=\"text/javascript\" key=\"TagsBoxScript\" src=\"/sitecore/shell/Applications/Taxonomy/Ribbon/SelectTagsPanel.js\"></script>");
      writer.Write(
        "<script type=\"text/javascript\" key=\"TagsBoxScript\">function Tags_KeyPressed() {alert('test');}</script>");
      TextBox tagsBox = new TextBox
        { TextMode = TextBoxMode.MultiLine, Width = Unit.Pixel(500), Height = Unit.Pixel(38) };
      tagsBox.Attributes.Add("onKeyPress", "Tags_KeyPressed();");
      tagsBox.RenderControl(writer);
      base.Render(writer);
    }
  }
}