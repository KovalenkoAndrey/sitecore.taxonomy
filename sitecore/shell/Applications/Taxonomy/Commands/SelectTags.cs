namespace Sitecore.Shell.Applications.Taxonomy.Commands
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Linq;

  using Sitecore.Shell.Applications.WebEdit.Commands;
  using Framework.Commands;

  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Web;
  using Sitecore.Web.UI.HtmlControls.Data;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.XamlSharp;
  using Sitecore.Data.Taxonomy;
  using Sitecore.Diagnostics;
  using Sitecore.Web.UI.WebControls;
  using Sitecore.Data.Fields;
  using Sitecore.Globalization;

  using HtmlAgilityPack;

  using Text;
    
  public class SelectTags : WebEditCommand
  {

    protected HtmlDocument doc;

    public override void Execute(CommandContext context)
    {

      Assert.ArgumentNotNull(context, "context");
      WebEditCommand.ExplodeParameters(context);
      bool containsTagNotFound = false;
      string currentValue = WebUtil.GetFormValue("scPlainValue");
      
      doc = new HtmlDocument();
      doc.LoadHtml(currentValue);
      if (doc.DocumentNode.SelectSingleNode("//span[@class='tagSet' or @class='tagNotFound']") != null)
      {
        string taxValue = string.Empty;
        foreach (HtmlNode span in doc.DocumentNode.SelectNodes("//span[@class='tagSet' or @class='tagNotFound']"))
        {
          string tagId;
          string weightId;
          switch (span.Attributes["class"].Value)
          {
            case "tagSet":
              tagId = span.Attributes["tagid"].Value;
              weightId = span.Attributes["weightid"].Value;
              taxValue = taxValue + tagId + ":" + weightId + "|";
              break;
            case "tagNotFound":
              tagId = ID.Null.ToString();
              weightId = span.InnerText;
              taxValue = taxValue + tagId + ":" + weightId + "|";
              containsTagNotFound = true;
              break;
          }
        }

        taxValue = taxValue.Trim('|');
        if (!string.IsNullOrEmpty(taxValue)) 
        {
          currentValue = taxValue;
        }
      }

      if ((context.Parameters["mode"] == "check") && !containsTagNotFound)
      {
        SheerResponse.Alert("There are not any unrecognized categories.");
      }
      else
      {
        context.Parameters.Add("currentValue", currentValue);
        SheerResponse.SetAttribute("scHtmlValue", "value", string.Empty);
        SheerResponse.SetAttribute("scPlainValue", "value", string.Empty);
        Sitecore.Context.ClientPage.Start(this, "SelectTag", context.Parameters);
      }

    }

      private static string RenderTags(ClientPipelineArgs args) 
      {
        Assert.ArgumentNotNull(args, "args");
        string result = args.Result;
        string str3 = args.Parameters["itemid"];
        string name = args.Parameters["language"];
        string str5 = args.Parameters["version"];
        string str6 = args.Parameters["fieldid"];
        string currentValue = "|" + args.Parameters["currentValue"] + "|";

        Item item = Context.ContentDatabase.GetItem(ID.Parse(str3), Language.Parse(name), Sitecore.Data.Version.Parse(str5));
        if (item == null)
        {
          SheerResponse.Alert("The item was not found.\n\nIt may have been deleted by another user.", new string[0]);
          return null;
        }
        Field field = item.Fields[ID.Parse(str6)];
        if (field == null)
        {
          SheerResponse.Alert("The field was not found.\n\nIt may have been deleted by another user.", new string[0]);
          return null;
        }

        string newTag = string.Format("{0}:{1}", result, "{C1453A1D-9ED2-428A-8BB3-50B4A877BEA7}");
        if (args.Parameters["mode"] == "check")
        {
          string tagNotFound = string.Format("|{0}:{1}|", ID.Null, args.Parameters["tagNotFound"]);
          currentValue = args.HasResult && args.Result != "undefined" ? currentValue.Replace(tagNotFound, "|" + newTag + "|") : currentValue.Replace(tagNotFound, "|");
        }
        else
        {
          currentValue += string.Format("{0}|", newTag);
        }

        currentValue = currentValue.Trim("|".ToCharArray());
        args.Parameters["currentValue"] = currentValue;

        FieldRenderer renderer = new FieldRenderer();
        renderer.Item = item;
        renderer.FieldName = field.Name;
        renderer.OverrideFieldValue(currentValue);
        renderer.DisableWebEditing = false;
        return renderer.Render();         
      }

      protected void SelectTag(ClientPipelineArgs args)
      {
        Assert.ArgumentNotNull(args, "args");
        string currentValue = args.Parameters["currentValue"];
        bool checkTagsMode = args.Parameters["mode"] == "check";
          if (args.IsPostBack)
          {
            if ((args.HasResult && (args.Result != "undefined")) || checkTagsMode)
            {
              string renderedValue = RenderTags(args);
              SheerResponse.SetAttribute("scHtmlValue", "value", renderedValue);
              SheerResponse.SetAttribute("scPlainValue", "value", args.Parameters["currentValue"]);
              SheerResponse.Eval("scSetHtmlValue('" + args.Parameters["controlid"] + "')");
            }

            if (checkTagsMode)
            {
              args.Result = "undefined";
              args.IsPostBack = false;
              this.SelectTag(args);
            }
          }
          else
          {
              UrlString url =
                new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Taxonomy.Dialogs.TagBrowser.aspx");
              UrlHandle handle = new UrlHandle();
              if (checkTagsMode)
              {
                string tagNotFound = currentValue.Split("|".ToCharArray()).FirstOrDefault(tag => StringUtil.GetPrefix(tag, ':').Equals("Null") || StringUtil.GetPrefix(tag, ':').Equals(ID.Null.ToString()));
                if (string.IsNullOrEmpty(tagNotFound))
                {
                  return;
                }
                tagNotFound = StringUtil.GetPostfix(tagNotFound, ':');
                handle["tagNotFound"] = tagNotFound;
                args.Parameters["tagNotFound"] = tagNotFound;
              }
              handle["categoriesRootId"] = "{41E44203-3CB9-45DD-8EED-9E36B5282D68}";
              handle["value"] = args.Parameters["currentValue"] ?? string.Empty;
              handle.Add(url);
              SheerResponse.ShowModalDialog(url.ToString(), "650px", "600px", string.Empty, true);
              args.WaitForPostBack();
          }
      }
  }
}