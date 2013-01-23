namespace Sitecore.Shell.Applications.Taxonomy.Commands
{
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Linq;

  using Framework.Commands;

  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls.Data;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.XamlSharp;

  using Text;

  public class SetTags : Command
  {
    public override void Execute(CommandContext context)
    {
      Sitecore.Context.ClientPage.Start(this, "Set", new NameValueCollection { {"item", context.Items[0].Paths.Path} });
    }

    protected void Set(ClientPipelineArgs args)
    {
      Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
      if (args.IsPostBack)
      {
        if (args.HasResult && (args.Result!="undefined"))
        {
          Item item = db.GetItem(args.Parameters["item"]);
          using (new EditContext(item))
          {
            item[new ID("{0F347169-F131-4276-A69D-187C0CAC3740}")] = args.Result;
          }
        }
      }
      else
      {
        UrlString url =
          new UrlString(
            ControlManager.GetControlUrl(new ControlName("Sitecore.Shell.Applications.Taxonomy.Dialogs.SetTags")));
        UrlHandle handle = new UrlHandle();
        handle["categories"] = GetCategories(args.Parameters["item"]);
        Item item = db.GetItem(args.Parameters["item"]);
        handle["value"] = item[new ID("{0F347169-F131-4276-A69D-187C0CAC3740}")];
        ID categoriesRootId = new ID("{41E44203-3CB9-45DD-8EED-9E36B5282D68}");
        string source = item.Fields[new ID("{0F347169-F131-4276-A69D-187C0CAC3740}")].Source;
        if (!string.IsNullOrEmpty(source))
        {
          var classificationSourceItems = LookupSources.GetItems(item, source);
          if ((classificationSourceItems != null) && (classificationSourceItems.Length > 0))
          {
            Item taxonomies = classificationSourceItems.ToList().Find(sourceItem => sourceItem.Name.Equals("Taxonomies"));
            if (taxonomies != null)
            {
              categoriesRootId = taxonomies.ID;
            }
          }
        }
        handle["categoriesRootId"] = categoriesRootId.ToString();
        handle.Add(url);
        SheerResponse.ShowModalDialog(url.ToString(), "400px", "150px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    private string GetCategories(string itemPath)
    {
      List<string> categories = new List<string>();
      Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
      Item item = db.GetItem(itemPath);
      ID categoriesRootId = new ID("{41E44203-3CB9-45DD-8EED-9E36B5282D68}");
      string source = item.Fields[new ID("{0F347169-F131-4276-A69D-187C0CAC3740}")].Source;
      if (!string.IsNullOrEmpty(source))
      {
        var classificationSourceItems = LookupSources.GetItems(item, source);
        if ((classificationSourceItems != null) && (classificationSourceItems.Length>0))
        {
          Item taxonomies = classificationSourceItems.ToList().Find(sourceItem => sourceItem.Name.Equals("Taxonomies"));
          if (taxonomies != null)
          {
            categoriesRootId = taxonomies.ID;
          }
        }
      }

      Item categoriesRoot = db.GetItem(categoriesRootId);
      foreach (Item categoryGroupItem in categoriesRoot.Children)
      {
        categories.AddRange(GetCategories(categoryGroupItem.DisplayName, categoryGroupItem));
      }

      return string.Join("|", categories.ToArray());
    }

    private IEnumerable<string> GetCategories(string prefix, Item categoryGroupItem)
    {
      //List<string> categories = new List<string>();
      foreach (Item categoryItem in categoryGroupItem.Children)
      {
        //categories.Add(prefix + "//" + categoryItem.DisplayName);
        //categories.AddRange(this.GetCategories(prefix + "//" + categoryItem.DisplayName, categoryItem));
        yield return string.Format("{0}//{1}:{2}", prefix, categoryItem.DisplayName, categoryItem.ID);
        foreach (var subCategoryPath in this.GetCategories(prefix + "//" + categoryItem.DisplayName, categoryItem))
        {
          yield return subCategoryPath;
        }
      }
      //return categories;
    }
  }
}