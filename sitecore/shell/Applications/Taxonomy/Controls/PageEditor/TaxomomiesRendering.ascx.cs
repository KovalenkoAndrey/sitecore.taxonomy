using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.Taxonomies.Layouts
{
  public partial class TaxomomiesRendering : System.Web.UI.UserControl
  {
    protected override void OnInit(EventArgs e) {
      this.Page.ClientScript.RegisterClientScriptInclude("JQuery", "/sitecore/shell/Controls/Lib/jQuery/jquery.js");
      if (UIUtil.IsIE())
      {
        this.Page.ClientScript.RegisterClientScriptInclude("JSON", "/scripts/json2.js");
      }
      this.Page.ClientScript.RegisterClientScriptInclude("EditorRendering", "/sitecore/shell/Applications/Taxonomy/Controls/PageEditor/ClassificationEditor.js");
      HtmlLink contextMenuLink = new HtmlLink();
      contextMenuLink.Href = "/sitecore/shell/Applications/Taxonomy/Controls/PageEditor/ClassificationEtitor.css";
      contextMenuLink.Attributes.Add("rel", "Stylesheet");
      contextMenuLink.Attributes.Add("type", "text/css");
      contextMenuLink.Attributes.Add("key", "menuStyle");
      this.Page.Header.Controls.Add(contextMenuLink);


      base.OnInit(e);
    }
    protected void Page_Load(object sender, EventArgs e)
    {
      categories.Value = String.Join("|", GetCategories().ToArray<String>()).Replace(@"//", @"/");
      taxWeightDefault.Value = "{C1453A1D-9ED2-428A-8BB3-50B4A877BEA7}";
      conflictcatigory.Value = StringUtil.Join(GetConflictCategories(), "|");
    }

    private IEnumerable<string> GetCategories()
    {
      Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;

      Item sourceItem = db.GetItem(new ID("{FF4E71C4-34D9-4432-BB93-A593D81F4260}"));
      if ((sourceItem != null) && (sourceItem.Children["Taxonomies"] != null))
      {
        Item taxonomiesRoot = sourceItem.Children["Taxonomies"];
        foreach (Item categoriesGroup in taxonomiesRoot.Children)
        {
          foreach (string categoryName in GetCategories(categoriesGroup.DisplayName, categoriesGroup))
          {
            yield return categoryName;
          }
        }
      }
    }

    private static IEnumerable<string> GetCategories(string prefix, Item categoryGroupItem)
    {
      foreach (Item categoryItem in categoryGroupItem.Children)
      {
        yield return string.Format("{0}//{1}:{2}", prefix, categoryItem.DisplayName, categoryItem.ID);
        foreach (var subCategoryPath in GetCategories(prefix + "//" + categoryItem.DisplayName, categoryItem))
        {
          yield return subCategoryPath;
        }
      }
    }

    private IEnumerable<string> GetConflictCategories()
    {
      Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
      Item sourceItem = db.GetItem(new ID("{FF4E71C4-34D9-4432-BB93-A593D81F4260}"));
      if ((sourceItem != null) && (sourceItem.Children["Taxonomies"] != null))
      {
        Item taxonomiesRoot = sourceItem.Children["Taxonomies"];
        foreach (Item categoriesGroup in taxonomiesRoot.Children)
        {
          foreach (string categoryConflict in GetConflictCategories(categoriesGroup))
          {
            yield return categoryConflict;
          }
        }
      }
    }

    private IEnumerable<string> GetConflictCategories(Item categoryGroupItem)
    {
      foreach (Item categoryItem in categoryGroupItem.Children)
      {
        if (!string.IsNullOrEmpty(categoryItem["ConflictTags"]))
        {
          yield return string.Format("{0}:{1}", categoryItem.ID, categoryItem["ConflictTags"].Replace('|', '&'));
        }
        foreach (var categoryConflict in GetConflictCategories(categoryItem))
        {
          yield return categoryConflict;
        }
      }
    }

  }
}