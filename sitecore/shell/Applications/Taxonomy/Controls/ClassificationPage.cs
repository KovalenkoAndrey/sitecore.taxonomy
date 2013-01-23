using System;
using System.Drawing;
using System.Linq;
using System.Web.UI.HtmlControls;
using Sitecore.Controls;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Framework.Commands;
using Sitecore.StringExtensions;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

using Sitecore.Web.UI.WebControls;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Text;
using System.Collections.Generic;

namespace Sitecore.Shell.Applications.Taxonomy
{
  public class ClassificationPage : DialogPage, IHasCommandContext
  {
    protected HtmlInputHidden categories;
    protected HtmlInputHidden result;
    protected HtmlInputHidden conflictcatigory;
    protected HtmlGenericControl tagsBox;
    private String currentTagId;
    private String curWeight;

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      if (!Sitecore.Context.ClientPage.IsEvent)
      {
        UrlHandle urlHandle = UrlHandle.Get();
        categories.Value = urlHandle["categories"];
        conflictcatigory.Value = urlHandle["conflictcat"];
        tagsBox.InnerHtml = urlHandle["rendered"];
        SheerResponse.Eval("SaveResult();");
      }
    }

    protected override void ExecuteAjaxCommand(Sitecore.Web.UI.XamlSharp.Ajax.AjaxCommandEventArgs e)
    {
      base.ExecuteAjaxCommand(e);
      if (e.Name=="taxonomy:tags:update")
      {
        SetValue(GetValue());
      }
      else if (e.Name == "taxonomy:tags:changeweight")
      {
          currentTagId = e.Parameters["tagId"];
          EditWeight();

      }

    }

    protected void EditWeight()
    {
        ClientPipelineArgs args = ContinuationManager.Current.CurrentArgs as ClientPipelineArgs;
        if (args.IsPostBack)
        {
            if (args.HasResult && args.Result != "undefined" && curWeight != args.Result)
            {
                
                List<string> CategoryWeightList  = GetValue().Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                int indTochange = CategoryWeightList.FindIndex(str => str.Contains(currentTagId));

                
                if(indTochange > -1){
                    CategoryWeightList[indTochange] = String.Format("{0}:{1}", currentTagId, args.Result);
                    String newValue = String.Join("|", CategoryWeightList.ToArray<string>());
                    SetValue(newValue);
                    SheerResponse.Eval("ReSaveResult('{0}','{1}');", currentTagId, args.Result);

                    
                }

            }
        }
        else
        {
            string weightsRootID = "{4A36115B-6120-4EDD-B6F2-E5F0EB2678EE}";
            curWeight = weightsRootID; 
            UrlHandle handle = UrlHandle.Get();
            string sourceItemId = handle["itemId"];
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
            if (!currentTagId.IsNullOrEmpty())
            {

                char[] delimiters = new char[] { '|' };
                string temp = GetValue();
                string[] parts = temp.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                foreach (String strTemp in parts)
                {
                    if (strTemp.IndexOf(currentTagId) > -1)
                    {
                        curWeight = strTemp.Split(":".ToCharArray(), StringSplitOptions.None)[1];
                    }
                }
            }
            url.Append("fo", curWeight);
            url.Append("sc_content", Sitecore.Context.ContentDatabase.Name);
            //url.Append("flt", "Contains('{EB06CEC0-5E2D-4DC4-875B-01ADCC577D13},{C20ED30F-D974-4C65-AE57-CE745C37940E}', @@templateid)");

            SheerResponse.ShowModalDialog(url.ToString(), "300px", "300px", string.Empty, true);
            args.WaitForPostBack();
        }
    }

    private string GetValue()
    {
        string value = string.Empty;
        string newValue = result.Value;

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
                                                                                   ((tagEntry.TagName.Equals("undefined"))) ? ("{C1453A1D-9ED2-428A-8BB3-50B4A877BEA7}") : (tagEntry.TagName)))
                                                                                 : ("{0}:{1}|".FormatWith(
                                                                                   tagEntry.TagId,
                                                                                   ((tagEntry.TagName.Equals("undefined"))) ? ("{C1453A1D-9ED2-428A-8BB3-50B4A877BEA7}") : (tagEntry.TagName)))))
        .Trim("|".ToCharArray());
        //value = result.Value;
      return value;
    }

    private void SetValue(string value)
    {
      UrlHandle urlHandle = UrlHandle.Get();
      string itemId = urlHandle["itemId"];
      string fieldId = urlHandle["fieldId"];
      string sessionKey = string.Format("{0}_{1}_classification", itemId, fieldId);
      result.Value = value;
      WebUtil.SetSessionValue(sessionKey, value);
    }

    public CommandContext GetCommandContext()
    {
      throw new NotImplementedException();
    }
  }
}