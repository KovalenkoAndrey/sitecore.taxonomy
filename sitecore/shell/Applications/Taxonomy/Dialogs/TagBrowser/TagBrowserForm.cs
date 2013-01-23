using System.Linq;
using System.Text;
using Sitecore.Data.Managers;
using Sitecore.Data.Taxonomy;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Shell.Applications.Taxonomy.Dialogs
{
  using ComponentArt.Web.UI;

  using Sitecore.Controls;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.Treeviews;
  using Sitecore.Taxonomies;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.XamlSharp.Xaml;
    using System.Collections.Generic;

  public class TagBrowserForm : DialogPage
  {
    protected Literal messageLiteral;
    protected Scrollbox treeviewBox;
    protected TreeView categoryTreeview;
    protected Literal newTagParentLiteral;
    protected MaskedInput categoryNameEdit;
    protected NumberInput categoryWeightEdit;
    protected NumberInput categoryRelatedToParentEdit;
    protected NumberInput categoryRelatedToChildEdit;
    protected Button createTagBtn;

    protected override void OnLoad(System.EventArgs e)
    {
      base.OnLoad(e);
      if (!XamlControl.AjaxScriptManager.IsEvent)
      {
        UrlHandle handle = UrlHandle.Get();
        string notFoundTag = handle["tagNotFound"];
        if (!string.IsNullOrEmpty(notFoundTag))
        {
          messageLiteral.Text = string.Format("Sitecore couldn't recognize the '<b>{0}</b>' tag.<br/>", notFoundTag) + messageLiteral.Text;
          categoryNameEdit.Text = notFoundTag;
        }

        string categoriesRootId = handle["categoriesRootId"];
        //Item rootItem = Client.ContentDatabase.GetItem(new ID(categoriesRootId));
        Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
        Item rootItem = db.GetItem(new ID(categoriesRootId));
        TreeviewSource treeviewSource = new TreeviewSource();
        
        categoryTreeview.Nodes.Clear();
        treeviewSource.Render(categoryTreeview, rootItem, true);
        categoryTreeview.ClientEvents.NodeSelect = new ClientEvent("categoryTreeview_OnNodeSelect");
      }
    }

    protected void Node_Selected()
    {
      string selected = WebUtil.GetFormValue("categoryTreeview_Selected");
      selected = ShortID.Decode(StringUtil.Mid(selected, 1));
      if (Sitecore.Data.ID.IsID(selected))
      {
        UrlHandle handle = UrlHandle.Get();
        Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
        Item selectedItem = db.GetItem(new ID(selected));
        Item rootItem = db.GetItem(new ID(handle["categoriesRootId"]));

        if (selectedItem.TemplateID.ToString() != "{A69C0097-5CE1-435B-99E9-FA2B998D1C70}")
        {
            SheerResponse.Eval("enableButton_Ok();");
        }
        else 
        {
            SheerResponse.Eval("disableButton_Ok();");
        }

        string categoryPath = selectedItem.Paths.Path.Substring(rootItem.Paths.Path.Length);
        newTagParentLiteral.Text = string.Format("{0}", categoryPath);
        createTagBtn.Disabled = !(selectedItem.Access.CanCreate() && selectedItem.Access.CanWrite());
      }
    }

    protected void createTag_Click()
    {
      string parentPath = newTagParentLiteral.Text;
      string categoryName = categoryNameEdit.Text;
      string categoryWeight = categoryWeightEdit.Value.ToString();
      string categoryParentRelated = categoryRelatedToParentEdit.Value.ToString();
      string categoryChildRelated = categoryRelatedToChildEdit.Value.ToString();
      if (string.IsNullOrEmpty(parentPath) ||
        string.IsNullOrEmpty(categoryName) ||
        string.IsNullOrEmpty(categoryWeight) ||
        string.IsNullOrEmpty(categoryParentRelated) ||
        string.IsNullOrEmpty(categoryChildRelated))
      {
        SheerResponse.Alert("You need to fill all the fields and select parent category/group to create new category.");
        return;
      }

      Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
      UrlHandle handle = UrlHandle.Get();
      Item rootItem = db.GetItem(new ID(handle["categoriesRootId"]));
      parentPath = rootItem.Paths.Path + parentPath;
      Item parentItem = db.GetItem(parentPath);
      Item newCategoryItem = parentItem.Add(categoryName, new TemplateID(new ID("{FCE30413-F1D8-43F4-8B93-B51FDED4D251}")));
      using (new EditContext(newCategoryItem))
      {
        newCategoryItem[CategoryItem.Fields.Weight] = categoryWeight;
        newCategoryItem[CategoryItem.Fields.RelatedToParent] = categoryParentRelated;
        newCategoryItem[CategoryItem.Fields.RelatedWithChildren] = categoryChildRelated;
      }
      SheerResponse.Eval("categoryTreeview_AddNode('{0}','I{1}','{2}');", categoryName, newCategoryItem.ID.ToShortID(),
                         Images.GetThemedImageSource(newCategoryItem.Appearance.Icon, ImageDimension.id16x16));
    }

    protected override void OK_Click()
    {
      string selected = WebUtil.GetFormValue("categoryTreeview_Selected");
      selected = ShortID.Decode(StringUtil.Mid(selected, 1));
      if (string.IsNullOrEmpty(selected))
      {
        SheerResponse.Alert("Please select a category.");
        return;
      }

      if (Sitecore.Data.ID.IsID(selected))
      {
        UrlHandle handle = UrlHandle.Get();
        Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
        Item selectedItem = db.GetItem(new ID(selected));
        Item rootItem = db.GetItem(new ID(handle["categoriesRootId"]));

        if (selectedItem.TemplateID.ToString() != "{A69C0097-5CE1-435B-99E9-FA2B998D1C70}")
        {      
          string taxValue = handle["value"];
          if (taxValue.Contains(selectedItem.ID.ToString()))
          {
            SheerResponse.Alert("Selected category is already assigned.\nPlease select another category.");
            return;
          }

          if (!string.IsNullOrEmpty(taxValue))
          {
            List<string> conflictCategoryNames = new List<string>();

            List<ID> tagConflicts = TaxonomyEngine.GetConflictTags(selectedItem.ID).ToList();
            if (tagConflicts.Count > 0)
            {
              foreach (ID itemTagId in StringUtil.Split(taxValue, '|', true).Select(id => new ID(StringUtil.GetPrefix(id, ':'))))
              {
                if (tagConflicts.Contains(itemTagId))
                {
                  CategoryItem categoryItem = new CategoryItem(Client.ContentDatabase.GetItem(itemTagId));
                  conflictCategoryNames.Add(categoryItem.CategoryName);
                }
              }
            }

            if (conflictCategoryNames.Count > 0)
            {
              CategoryItem categoryItem = new CategoryItem(selectedItem);

              if (conflictCategoryNames.Count > 3)
              {
                SheerResponse.Alert(
                  string.Format(
                    Messages.CategoryConflictsWithAlreadyAssignedMoreThanThree,
                    categoryItem.CategoryName,
                    StringUtil.Join(conflictCategoryNames.Take(3).Select(categoryName => "  -" + categoryName + "\n"), string.Empty),
                    conflictCategoryNames.Count - 3));
              }
              else
              {
                SheerResponse.Alert(
                string.Format(
                  Messages.CategoryConflictsWithAlreadyAssigned,
                  categoryItem.CategoryName,
                  StringUtil.Join(conflictCategoryNames.Select(categoryName => "  -" + categoryName + "\n"), string.Empty)));
              }
              return;
            }
          }
          SheerResponse.SetDialogValue(selected);
          base.OK_Click();
        }
        else
        {
          SheerResponse.Alert("Please select a category.");
        }
      }
    }
  }
}