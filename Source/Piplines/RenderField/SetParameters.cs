namespace Sitecore.Taxonomies.Piplines.RenderField
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Globalization;
  using Sitecore.Pipelines.RenderField;
  using Sitecore.Web;

  /// <summary>
  /// Sets classification field parameters
  /// </summary>
  public class SetParameters
  {
    /// <summary>
    /// Processor main method.
    /// </summary>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void Process(RenderFieldArgs args)
    {
      if (args.FieldTypeKey == "classification")
      {
        using (new LanguageSwitcher(WebUtil.GetCookieValue("shell", "lang", Sitecore.Context.Language.Name)))
        {
          Database coreDb = Factory.GetDatabase("core");
          Item fieldTexts = coreDb.GetItem("/sitecore/content/Applications/WebEdit/WebEdit Texts/Classification Texts");
          if (fieldTexts != null)
          {
            args.RenderParameters["default-text"] = fieldTexts["Default Text"];
          }
        }
      }
    }
  }
}