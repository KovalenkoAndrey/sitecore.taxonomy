namespace Sitecore.Shell.Applications.Taxonomy.Dialogs
{
  using System;
  using System.Linq;
  using System.Web.UI.HtmlControls;

  using Sitecore.Controls;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Web;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.WebControls;
  using Sitecore.Web.UI.XamlSharp;

  using Text;

  public class SetTagsForm : DialogPage
  {
    protected HtmlInputHidden categories;
    protected HtmlInputHidden result;
    protected HtmlGenericControl tagsBox;

    protected override void OnInit(System.EventArgs e)
    {
      UrlHandle handle = UrlHandle.Get();
      string categoriesValue = handle["categories"];
      categories.Value = categoriesValue.Replace(@"//", @"/");
      base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
      UrlHandle handle = UrlHandle.Get();
      string currentValue = handle["value"];
      if (!string.IsNullOrEmpty(currentValue))
      {
        foreach (string tagValue in currentValue.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
        {
          string categoryId = StringUtil.GetPrefix(tagValue, ':');
          Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
          Item categoryItem = db.GetItem(new ID(categoryId));
          string categoryName = this.GetCategoryName(categoryItem);
          tagsBox.InnerHtml += string.Format(
            "<span class=\"tagSet\"><span class=\"tagId\">{0}</span>{1}</span>;", categoryId, categoryName);
        }
      }
      base.OnLoad(e);
    }

    protected override void OK_Click()
    {
      string value = GetValue();
      SheerResponse.SetDialogValue(value);
      base.OK_Click();
    }

    private string GetValue()
    {
      CheckTags();
      string html = result.Value;
      return
        html.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(tagEntry => tagEntry.Contains(":")).
          Aggregate(
            string.Empty,
            (current, tagEntry) =>
            current +
            string.Format("{0}:{1}|", StringUtil.GetPrefix(tagEntry, ':'), "{C1453A1D-9ED2-428A-8BB3-50B4A877BEA7}"));
    }
    private void CheckTags()
    {
      string html = result.Value;
      foreach (string tagEntry in html.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
      {
        if (!tagEntry.Contains(":"))
        {
          CheckTag(tagEntry);
        }
      }
    }

    private void CheckTag(string tagEntry)
    {
      tagsBox.InnerHtml.Replace(string.Format("<span class=\"tagNotFound\">{0}</span>;", tagEntry), string.Empty);
      this.ShowTags(tagEntry);
    }

    protected void ShowTags()
    {
      this.ShowTags(string.Empty);
    }

    protected void ShowTags(string notFoundTag)
    {
      ClientPipelineArgs args = ContinuationManager.Current.CurrentArgs as ClientPipelineArgs;
      if (args.IsPostBack)
      {
        if (args.HasResult && args.Result != "undefined")
        {
          Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
          Item categoryItem = db.GetItem(new ID(args.Result));
          string tagValue = GetCategoryName(categoryItem);
          string tagId = categoryItem.ID.ToString();
          SheerResponse.Eval("SetTag('{0}','{1}')", tagValue, tagId);
        }
      }
      else
      {
        UrlString url =
          new UrlString(
            ControlManager.GetControlUrl(new ControlName("Sitecore.Shell.Applications.Taxonomy.Dialogs.TagBrowser")));
        UrlHandle currentHandle = UrlHandle.Get();
        UrlHandle handle = new UrlHandle();
        handle["tagNotFound"] = notFoundTag;
        handle["categoriesRootId"] = currentHandle["categoriesRootId"];
        handle.Add(url);
        SheerResponse.ShowModalDialog(url.ToString(), "650px", "600px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    private string GetCategoryName(Item categoryItem)
    {
      string categoryName = categoryItem.DisplayName;
      if (!categoryItem.ParentID.ToString().Equals("{41E44203-3CB9-45DD-8EED-9E36B5282D68}"))
      {
        categoryName = this.GetCategoryName(categoryItem.Parent) + "/" + categoryName;
      }
      return categoryName;
    }
  }
}