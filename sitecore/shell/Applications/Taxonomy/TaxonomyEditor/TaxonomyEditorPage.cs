namespace Sitecore.Shell.Applications.Taxonomy.TaxonomyEditor
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;

  using Sitecore.Controls;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Globalization;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.WebControls;

  using Text;

  public class TaxonomyEditorPage : DialogPage
  {
    private const string TaxonomyValueKey = "Taxonomy_Value";
    private const string TaxonomySourceItemIdKey = "Taxonomy_SourceItemId";

    private List<string> CategoryWeightList
    {
      get
      {
        return this.Value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
      }
    }

    private string Value
    { 
      get
      {
        return HttpContext.Current.Session[TaxonomyValueKey].ToString();
      }
      set
      {
        HttpContext.Current.Session[TaxonomyValueKey] = value;
      }
    }

    protected Border Categories;

    protected override void OnPreRender(EventArgs e)
    {
      base.OnPreRender(e);
      if (!AjaxScriptManager.IsEvent)
      {
        UrlHandle handle = UrlHandle.Get();
        Value = handle["value"];
        RenderTaxonomies();
      }
    }

    private void RenderTaxonomies()
    {
      string category = "<div>";
      int index = 0;
      foreach (string categoryValue in CategoryWeightList)
      {
        string categoryId = StringUtil.GetPrefix(categoryValue, ':');
        string weightId = StringUtil.GetPostfix(categoryValue, ':');
        string categoryName = Translate.Text("select category");
        string categoryStyle = "scConditionNotSet";
        if (!string.IsNullOrEmpty(categoryId) && Sitecore.Data.ID.IsID(categoryId))
        {
          Item categoryItem = Sitecore.Context.ContentDatabase.GetItem(new ID(categoryId));
          if (categoryItem!=null)
          {
            categoryName = categoryItem.DisplayName;
            categoryStyle = "scCondition";
          }
        }
        string weightName = Translate.Text("select weight");
        string weightStyle = "scConditionNotSet";
        if (!string.IsNullOrEmpty(weightId) && Sitecore.Data.ID.IsID(weightId))
        {
          Item weightItem = Sitecore.Context.ContentDatabase.GetItem(new ID(weightId));
          if (weightItem != null)
          {
            weightName = weightItem.DisplayName;
            weightStyle = "scCondition";
          }
        }
        string editCategoryMethod = StringUtil.EscapeQuote(string.Format("EditCategory(\"{0}\")", index));
        string editCategory = string.Format("javascript:return scForm.postRequest('','','','{0}')", editCategoryMethod);
        string editWeightMethod = StringUtil.EscapeQuote(string.Format("EditWeight(\"{0}\")", index));
        string editWeight = string.Format("javascript:return scForm.postRequest('','','','{0}')", editWeightMethod);
        string removeTag = StringUtil.EscapeQuote(string.Format("javascript:return scForm.postRequest('','','','RemoveTag(\"{0}\")')", index));
        category +=
          string.Format(
            "<div>" + "<a href=\"#\" class=\"{0}\" onclick=\"{1}\">{2}</a>" + " is " +
            "<a href=\"#\" class=\"{3}\" onclick=\"{4}\">{5}</a>" + "<a href=\"#\" onclick=\"{6}\" class=\"scConditionBtn\">x</a>" +
            "</div>",
            categoryStyle,
            editCategory,
            categoryName,
            weightStyle,
            editWeight,
            weightName,
            removeTag);
        index++;
      }
      string newCategory = StringUtil.EscapeQuote(string.Format("javascript:return scForm.postRequest('','','','EditCategory(\"{0}\")')", -1));
      string newWeight = StringUtil.EscapeQuote(string.Format("javascript:return scForm.postRequest('','','','EditWeight(\"{0}\")')", -1));
      category +=
        string.Format(
          "<div>" + "<a href=\"#\" class=\"scConditionNotSet\" onclick=\"{0}\">{1}</a>" + " is " +
          "<a href=\"#\" class=\"scConditionNotSet\" onclick=\"{2}\">{3}</a>" + "</div>",
          newCategory,
          "select category",
          newWeight,
          "select weight");
      category += "</div>";
      this.Categories.InnerHtml = category;
    }

    protected void EditCategory(string index)
    {
      int indexValue = Int32.Parse(index);
      ClientPipelineArgs args = ContinuationManager.Current.CurrentArgs as ClientPipelineArgs;
      if (args.IsPostBack)
      {
        if (args.HasResult && args.Result != "undefined")
        {
          if (indexValue > -1)
          {
            string oldValue = CategoryWeightList[indexValue];
            string newValue = string.Format("{0}:{1}", args.Result, StringUtil.GetPostfix(oldValue, ':'));
            Value = Value.Replace(oldValue, newValue);
          }
          else
          {
            Value += string.Format("|{0}:{1}|", args.Result, Sitecore.Data.ID.Null);
            Value = Value.Replace("||", "|").Trim("|".ToCharArray());
          }
          this.RenderTaxonomies();
        }
      }
      else
      {
        string taxonomiesRootID = "{41E44203-3CB9-45DD-8EED-9E36B5282D68}";
        UrlHandle handle = UrlHandle.Get();
        string sourceItemId = handle["sourceItemId"];
        Item classificationSourceItem = Client.ContentDatabase.GetItem(new ID(sourceItemId));
        if (classificationSourceItem != null)
        {
          Item taxonomies = classificationSourceItem.Children["Taxonomies"];
          if (taxonomies != null)
          {
            taxonomiesRootID = taxonomies.ID.ToString();
          }
        }

        UrlString url = new UrlString("/sitecore/shell/Applications/Item browser.aspx");
        url.Append("ro", taxonomiesRootID);
        url.Append("sc_content", Sitecore.Context.ContentDatabase.Name);
        url.Append("id", taxonomiesRootID);
        //url.Append("flt", "Contains('{EB06CEC0-5E2D-4DC4-875B-01ADCC577D13},{C20ED30F-D974-4C65-AE57-CE745C37940E}', @@templateid)");
        SheerResponse.ShowModalDialog(url.ToString(), "300px", "300px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    protected void EditWeight(string index)
    {
      int indexValue = Int32.Parse(index);
      ClientPipelineArgs args = ContinuationManager.Current.CurrentArgs as ClientPipelineArgs;
      if (args.IsPostBack)
      {
        if (args.HasResult && args.Result != "undefined")
        {
          if (indexValue > -1)
          {
            string oldValue = CategoryWeightList[indexValue];
            string newValue = string.Format("{0}:{1}", StringUtil.GetPrefix(oldValue, ':'), args.Result);
            Value = Value.Replace(oldValue, newValue);
          }
          else
          {
            Value += string.Format("|{0}:{1}|", Sitecore.Data.ID.Null, args.Result);
            Value = Value.Replace("||", "|").Trim("|".ToCharArray());
          }
          this.RenderTaxonomies();
        }
      }
      else
      {
        string weightsRootID = "{4A36115B-6120-4EDD-B6F2-E5F0EB2678EE}";
        UrlHandle handle = UrlHandle.Get();
        string sourceItemId = handle["sourceItemId"];
        Item classificationSourceItem = Client.ContentDatabase.GetItem(new ID(sourceItemId));
        if (classificationSourceItem != null)
        {
          Item taxonomies = classificationSourceItem.Children["Weights"];
          if (taxonomies != null)
          {
            weightsRootID = taxonomies.ID.ToString();
          }
        }

        UrlString url = new UrlString("/sitecore/shell/Applications/Item browser.aspx");
        url.Append("ro", weightsRootID);
        url.Append("id", weightsRootID);
        url.Append("sc_content", Sitecore.Context.ContentDatabase.Name);
        //url.Append("flt", "Contains('{EB06CEC0-5E2D-4DC4-875B-01ADCC577D13},{C20ED30F-D974-4C65-AE57-CE745C37940E}', @@templateid)");
        SheerResponse.ShowModalDialog(url.ToString(), "300px", "300px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    protected void RemoveTag(string index)
    {
      int indexValue = Int32.Parse(index);
      string oldValue = CategoryWeightList[indexValue];
      Value = Value.Replace(oldValue, string.Empty).Replace("||", "|");
      this.RenderTaxonomies();
    }

    protected override void OK_Click()
    {
      if (Value.Contains(Sitecore.Data.ID.Null + ":") || Value.Contains(":" + Sitecore.Data.ID.Null))
      {
        SheerResponse.Alert(Translate.Text("Please set category/weight or remove tag."));
        return;
      }
      SheerResponse.SetDialogValue(Value.Trim("|".ToCharArray()));
      base.OK_Click();
    }
  }
}