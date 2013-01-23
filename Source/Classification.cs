namespace Sitecore.Shell.Applications.ContentEditor
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Linq;
  using Diagnostics;

  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.Taxonomy;
  using Sitecore.Text;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.HtmlControls.Data;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.XamlSharp;

  using StringExtensions;

  /// <summary>
  /// Classification manipulation control
  /// </summary>
  public class Classification : Frame, IContentField
  {
    private const string FrameName = "classificationFrame";
    private readonly string scriptBase = string.Format("document.getElementsByName('{0}')[0].contentWindow.", FrameName);
    protected static List<string> notFoundTags = new List<string>();

    protected override void OnLoad(EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      base.OnLoad(e);
      if (!Sitecore.Context.ClientPage.IsEvent)
      {
        UrlString urlString = new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Taxonomy.Classification.aspx");
        UrlHandle urlHandle = new UrlHandle();
        urlHandle["itemId"] = ItemId;
        urlHandle["fieldId"] = FieldId;
        urlHandle["db"] = Sitecore.Context.ContentDatabase.Name;
        urlHandle["categories"] = StringUtil.Join(GetCategories(), "|").Replace(@"//", @"/");
        urlHandle["rendered"] = ValueRendered;
        //urlHandle["conflictcat"] = "{E0B7FD46-ADB2-4AE9-A0D7-44D91806A2BE}:{C23B8255-A0BF-4BAA-93B3-83D072FA940B}&{DF566E92-EEE8-417A-B6BA-4002D2632374}&{9EAB572E-4076-40D5-BDF3-EED53B78629F}|{7CE38A45-1A35-409D-902E-1EC106D54E00}:{6B3DE552-69E6-4150-92B5-6CC071C916C7}"; 
        urlHandle["conflictcat"] = StringUtil.Join(GetConflictCategories(), "|");
        if (Disabled)
        {
          urlString["di"] = "1";
        }
        urlHandle.Add(urlString);
        SourceUri = urlString.ToString();
        Name = FrameName;
      }
    }

    /// <summary>
    /// Gets or sets the item id.
    /// </summary>
    /// <value>The item id.</value>
    public string ItemId
    {
      get
      {
        return GetViewStateString("ItemId");
      }
      set
      {
        SetViewStateString("ItemId", value);
      }
    }
    /// <summary>
    /// Gets or sets the item language.
    /// </summary>
    /// <value>The item language.</value>
    public string ItemLanguage
    {
      get
      {
        return GetViewStateString("ItemLanguage");
      }
      set
      {
        SetViewStateString("ItemLanguage", value);
      }
    }
    /// <summary>
    /// Gets or sets the item version.
    /// </summary>
    /// <value>The item version.</value>
    public string ItemVersion
    {
      get
      {
        return GetViewStateString("ItemVersion");
      }
      set
      {
        SetViewStateString("ItemVersion", value);
      }
    }

    /// <summary>
    /// Gets or sets the field id.
    /// </summary>
    /// <value>The field id.</value>
    public string FieldId
    {
      get
      {
        return GetViewStateString("FieldId");
      }
      set
      {
        SetViewStateString("FieldId", value);
      }
    }

    /// <summary>
    /// Gets or sets the source of the field.
    /// </summary>
    /// <value>The field source.</value>
    public string Source
    {
      get
      {
        return GetViewStateString("Source");
      }
      set
      {
        SetViewStateString("Source", value);
      }
    }

    /// <summary>
    /// Gets the source item id.
    /// </summary>
    /// <value>The source item id.</value>
    public string SourceItemId
    {
      get
      {
        Item currentItem = Client.ContentDatabase.GetItem(new ID(ItemId));
        if (currentItem != null)
        {
          Item[] sourceItems = LookupSources.GetItems(currentItem, Source);
          if ((sourceItems != null) && (sourceItems.Length > 0))
          {
            return sourceItems[0].ParentID.ToString();
          }
        }
        return "{FF4E71C4-34D9-4432-BB93-A593D81F4260}";
      }
    }

    public string CategoriesRootId
    {
      get
      {
        Item sourceItem = Client.ContentDatabase.GetItem(new ID(SourceItemId));
        if (sourceItem != null)
        {
          Item categoriesRootItem = sourceItem.Children["Taxonomies"];
          if (categoriesRootItem != null)
          {
            return categoriesRootItem.ID.ToString();
          }
        }
        return string.Empty;
      }
    }

    class RenderState
    {
      public bool Automatic { get; set; }
      public IEnumerable<RelationInfo> Relations { get; set; }
    }

    private IEnumerable<string> GetCategories()
    {
      Item sourceItem = Client.ContentDatabase.GetItem(new ID(SourceItemId));
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
      Item sourceItem = Client.ContentDatabase.GetItem(new ID(SourceItemId));
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

    protected void UpdateValue()
    {

    }

    protected string ValueRendered
    {
      get
      {
        string valueRendered = string.Empty;
        string[] categoryWeightPair = Value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        foreach (string categoryWeight in categoryWeightPair)
        {
          string weightId = StringUtil.GetPostfix(categoryWeight, ':');
          if (weightId == "undefined")
          {
            weightId = "{C1453A1D-9ED2-428A-8BB3-50B4A877BEA7}";
          }
          string categoryId = StringUtil.GetPrefix(categoryWeight, ':');
          if (categoryId != Sitecore.Data.ID.Null.ToString())
          {
            Item categoryInnerItem = Client.ContentDatabase.GetItem(new ID(categoryId));
            if (categoryInnerItem != null)
            {
              CategoryItem categoryItem = new CategoryItem(categoryInnerItem);
              string categoryName = categoryItem.CategoryName;
              valueRendered += string.Format(
                "<span class=\"tagSet\" tagId=\"{0}\" weightId=\"{1}\" onclick=\"tag_select(event);\" onDblclick=\"change_weight(this);\">{2}<wbr /></span>;", categoryId, weightId, categoryName);
            }
          }
          else
          {
            string tagName = StringUtil.GetPostfix(categoryWeight, ':');
            valueRendered += string.Format("<span class=\"tagNotFound\" tagId=\"Null\">{0}<wbr /></span>;",
                                           tagName);
          }
        }

        return valueRendered;
      }
    }

    public string GetValue()
    {
      return WebUtil.GetSessionString(string.Format("{0}_{1}_classification", ItemId, FieldId), string.Empty);
    }

    public void SetValue(string value)
    {
      Value = value;
      SheerResponse.SetAttribute(ID, "value", value);
      SheerResponse.SetModified(true);
    }

    public override void HandleMessage(Message message)
    {
      base.HandleMessage(message);
      if (message.Name != null)
      {
        switch (message.Name)
        {
          case "taxonomy:edit":
            Sitecore.Context.ClientPage.Start(this, "Edit");
            break;
          case "taxonomy:tags:update":
            string value = string.Empty;
            string newValue = WebUtil.GetFormValue("result");
            value =
              newValue.Split("|".ToCharArray()).Select(
                tagEntry =>
                new { TagId = StringUtil.GetPrefix(tagEntry, ':'), TagName = StringUtil.GetPostfix(tagEntry, ':') }).Where
                (tagEntry => !string.IsNullOrEmpty(tagEntry.TagId)).Aggregate(value,
                                                                              (current, tagEntry) =>
                                                                              current +
                                                                              ((tagEntry.TagId.Equals("Null"))
                                                                                 ? ("{0}:{1}|".FormatWith(
                                                                                   Sitecore.Data.ID.Null,
                                                                                   tagEntry.TagName))
                                                                                 : (tagEntry.TagId +
                                                                                    ":{C1453A1D-9ED2-428A-8BB3-50B4A877BEA7}|")))
                .Trim("|".ToCharArray());
            SetValue(value);
            Sitecore.Context.ClientPage.ClientResponse.Refresh(this);
            break;
          case "taxonomy:selecttag":
            Sitecore.Context.ClientPage.Start(this, "SelectTag");
            break;
          case "taxonomy:checktags":
            string newValueNotFound = GetValue();
            notFoundTags = newValueNotFound.Split("|".ToCharArray()).Where(
                tagEntry => StringUtil.GetPrefix(tagEntry, ':').Equals(Sitecore.Data.ID.Null.ToString())).
              Select(name => StringUtil.GetPostfix(name, ':')).ToList();
            if (notFoundTags.Count >= 1)
            {
              this.CheckTags();
            }
            else
            {
              SheerResponse.Alert("There are not any unrecognized categories.");
            }

            break;
          case "taxonomy:related":
            Sitecore.Context.ClientPage.Start(this, "ShowRelated");
            break;
        }
      }
    }

    protected void CheckTags()
    {
      if(notFoundTags.Count > 0)
      {
        Sitecore.Context.ClientPage.Start(this, "SelectTag", new NameValueCollection { { "tagNotFound", notFoundTags.FirstOrDefault() } });
        notFoundTags.RemoveAt(0);
      }
    }


    protected void SelectTag(ClientPipelineArgs args)
    {
      string tagNotFound = args.Parameters["tagNotFound"];
      if (args.IsPostBack)
      {
        string oldValue = "|" + GetValue() + "|";
        string newValue = oldValue;
        if (!string.IsNullOrEmpty(tagNotFound))
        {
          newValue = newValue.Replace(string.Format("|{0}:{1}|", Sitecore.Data.ID.Null, tagNotFound), "|");
          SetValue(newValue.Trim("|".ToCharArray()));
          SheerResponse.Eval("{0}RemoveTagNotFound('{1}');", scriptBase, tagNotFound);
        }
        if (args.HasResult && args.Result != "undefined")
        {
          Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
          CategoryItem categoryItem = new CategoryItem(db.GetItem(new ID(args.Result)));
          string tagId = categoryItem.ID.ToString();
          string tagValue = categoryItem.CategoryName;
          newValue += string.Format("{0}:{1}|", tagId, "{C1453A1D-9ED2-428A-8BB3-50B4A877BEA7}");
          SetValue(newValue.Trim("|".ToCharArray()));
          SetModified();
          SheerResponse.Eval("{0}SetTag('{1}','{2}');", scriptBase, tagValue, tagId);
        }
        CheckTags();
      }
      else
      {
        UrlString url =
          new UrlString(
            ControlManager.GetControlUrl(new ControlName("Sitecore.Shell.Applications.Taxonomy.Dialogs.TagBrowser")));
        UrlHandle handle = new UrlHandle();
        if (!string.IsNullOrEmpty(tagNotFound))
        {
          handle["tagNotFound"] = tagNotFound;
        }
        handle["categoriesRootId"] = CategoriesRootId;
        handle["value"] = GetValue() ?? string.Empty;
        handle.Add(url);
        SheerResponse.ShowModalDialog(url.ToString(), "650px", "600px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    protected void ShowRelated(ClientPipelineArgs args)
    {
      if (args.IsPostBack)
      {

      }
      else
      {
        UrlString url =
          new UrlString(
            ControlManager.GetControlUrl(new ControlName("Sitecore.Shell.Applications.Taxonomy.Dialogs.RelatedScreen")));
        UrlHandle handle = new UrlHandle();
        handle["itemId"] = ItemId;
        handle["taxonomyValue"] = GetValue();
        handle.Add(url);
        SheerResponse.ShowModalDialog(url.ToString(), "900px", "400px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    protected virtual void SetModified()
    {
      Sitecore.Context.ClientPage.Modified = true;
      SheerResponse.Eval("scContent.startValidators()");
    }

    protected void Edit(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (args.IsPostBack)
      {
        if ((args.Result != null) && (args.Result != "undefined"))
        {
          SetValue(args.Result);
        }
      }
      else
      {
        UrlString url =
          new UrlString(
            ControlManager.GetControlUrl(new ControlName("Sitecore.Shell.Applications.Taxonomy.TaxonomyEditor")));
        UrlHandle urlHandle = new UrlHandle();
        urlHandle["value"] = GetValue() ?? string.Empty;
        urlHandle["sourceItemId"] = SourceItemId;
        urlHandle.Add(url);
        SheerResponse.ShowModalDialog(url.ToString(), "300px", "300px", string.Empty, true);
        args.WaitForPostBack();
      }
    }
  }
}