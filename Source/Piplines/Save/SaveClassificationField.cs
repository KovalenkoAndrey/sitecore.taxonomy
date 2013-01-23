using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Sitecore.Pipelines.Save;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using HtmlAgilityPack;

namespace Sitecore.Taxonomies.Pipelines.Save
{
  using Sitecore.Data;

  public class SaveClassificationField
  {
    public void Process(SaveArgs args)
    {
      foreach (SaveArgs.SaveItem item in args.Items)
      {
        Item item2 = Context.ContentDatabase.Items[item.ID, item.Language, item.Version];
        if (item2 != null)
        {
          item2.Editing.BeginEdit();
          string newValue = String.Empty;
          foreach (SaveArgs.SaveField field in item.Fields)
          {
            Field field2 = item2.Fields[field.ID];
            if (field2 != null && field2.Type.Equals("Classification", StringComparison.InvariantCultureIgnoreCase))
            { 
              HtmlDocument doc = new HtmlDocument();
              doc.LoadHtml(field2.Value);
              if (doc.DocumentNode.SelectSingleNode("//span[@class='tagSet' or @class='tagNotFound']") != null)
              {
                foreach (HtmlNode span in doc.DocumentNode.SelectNodes("//span[@class='tagSet' or @class='tagNotFound']"))
                {
                  string tagId = span.Attributes["tagid"].Value;
                  if ((tagId == "Null") || (tagId == ID.Null.ToString()))
                  {
                    continue;
                  }

                  string weightId = span.Attributes["weightid"].Value;
                  newValue = newValue + tagId + ":" + weightId + "|";
                }
                newValue = newValue.Trim('|');
              }
              else 
              {
                List<string> listValue = field2.Value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                listValue.RemoveAll(fielV => fielV.Contains(Sitecore.Data.ID.Null.ToString()));
                newValue = String.Join("|", listValue.ToArray<string>());
              }
              field2.Value = newValue;
              field.OriginalValue = newValue;
            }
          }
          item2.Editing.EndEdit();
          Log.Audit(this, "Save item: {0}", new[] { AuditFormatter.FormatItem(item2) });
          args.SavedItems.Add(item2);

        }
      }
    }
  }
}