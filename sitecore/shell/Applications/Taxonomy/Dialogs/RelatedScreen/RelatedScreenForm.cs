using System.Collections.Generic;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Shell.Applications.Taxonomy.Dialogs
{
  using System.Linq;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.Taxonomy;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.WebControls;
  using Sitecore.Web.UI.XamlSharp.Xaml;

  using System;

  public class RelatedScreenForm : XamlMainControl
  {
    protected Scrollbox ResultBox;
    protected Button CloseButton;

    protected override void OnLoad(System.EventArgs e)
    {
      base.OnLoad(e);
      UrlHandle handle = UrlHandle.Get();
      string itemId = handle["itemId"];
      string taxonomyValue = handle["taxonomyValue"];
      List<string> listValue = taxonomyValue.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
      listValue.RemoveAll(str => str.Contains(Sitecore.Data.ID.Null.ToString()));
      taxonomyValue = String.Join("|", listValue.ToArray<string>());
      if (!string.IsNullOrEmpty(taxonomyValue) && !string.IsNullOrEmpty(itemId) && Sitecore.Data.ID.IsID(itemId))
      {

        Item currentItem = Client.ContentDatabase.GetItem(new ID(itemId));
        IOrderedEnumerable<RelationInfo> categories =
          TaxonomyEngine.GetAllCategories(currentItem, taxonomyValue).Values.OrderBy(info => info.Weight);
        IEnumerable<RelationInfo> setCategories = (from category in categories
                                                   where !category.Calculated
                                                   select category);


            foreach (RelationInfo link in setCategories)
            {
                GridPanel categoriesPanel = new GridPanel();
                ResultBox.Controls.Add(categoriesPanel);
                categoriesPanel.Attributes["class"] = "categoriesPanel";
                categoriesPanel.Columns = 100;
                categoriesPanel.Controls.Add(GetCategoryPanel(link));
                BuildRelatedCategories(categories, categoriesPanel, link);
            }
      }
      else
      {
        Literal noDataLiteral = new Literal("No taxonomy data could be retrieved.");
        ResultBox.Controls.Add(noDataLiteral);
      }
    }

    private static GridPanel GetCategoryPanel(RelationInfo link)
    {
      CategoryItem categoryItem = new CategoryItem(Client.ContentDatabase.GetItem(link.Category));
      GridPanel categoryPanel = new GridPanel();
      string categoryFullName = categoryItem.CategoryName;
      string categoryName = categoryItem.DisplayName;
      categoryPanel.Controls.Add(new Literal(string.Format("{0} ({1:F0}%)", categoryName, link.Weight))
                                   {Class = "categoryName"});
      categoryPanel.Controls.Add(
        new Literal(string.Format("{0}",
                                  categoryFullName.Substring(0, categoryFullName.Length - categoryName.Length - 1)))
          {Class = "categoryPath"});
      categoryPanel.Attributes["class"] = "categoryPanel";
      return categoryPanel;
    }

    private void BuildRelatedCategories(IOrderedEnumerable<RelationInfo> categories, GridPanel categoriesPanel, RelationInfo originCategory)
    {
        IEnumerable<RelationInfo> calculatedCategories = (from category in categories
                                                          where
                                                            (
                                                             (category.RelatedCategory.IndexOf(originCategory.Category) > -1))
                                                          select category);

      foreach (RelationInfo calculatedCategory in calculatedCategories)
      {
        categoriesPanel.Controls.Add(GetSeparator());
        categoriesPanel.Controls.Add(GetCategoryPanel(calculatedCategory));
        BuildRelatedCategories(categories, categoriesPanel, calculatedCategory);
      }
    }

    private static System.Web.UI.Control GetSeparator()
    {
      string iconSrc = Images.GetThemedImageSource("Applications/32x32/navigate_right.png", ImageDimension.id32x32);
      Sitecore.Web.UI.HtmlControls.Image icon = new Sitecore.Web.UI.HtmlControls.Image(iconSrc);
      return icon;
    }

    protected void Close_Click()
    {
      SheerResponse.CloseWindow();
    }
  }
}