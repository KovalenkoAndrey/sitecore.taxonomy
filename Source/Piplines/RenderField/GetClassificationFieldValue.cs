using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Sitecore.Pipelines.RenderField;
using Sitecore.Data;
using Sitecore.Data.Taxonomy;
using Sitecore.Xml.Xsl;
using System.Web.UI;
using System.IO;
using Sitecore.Web;

namespace Sitecore.Taxonomies.Pipelines.RenderField
{
    public class GetClassificationFieldValue
    {
    
      public void Process(RenderFieldArgs args)
      {
        if (args.FieldTypeKey == "classification")
        {

          string temp = RenderToHtml(args);
          RenderFieldResult result = new RenderFieldResult(temp);

          args.Result.FirstPart = result.FirstPart;
          args.Result.LastPart = result.LastPart;

          args.DisableWebEditContentEditing = false;
          args.DisableWebEditContentSelecting = false;

        }

      }

        private string RenderToHtml(RenderFieldArgs args) 
        {

          Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
          
          
          CategoryItem categoryItem; 
          
          string rowValue = string.Empty;
          string htmlValue = string.Empty;
          string tagId = string.Empty;
          string tagWeight = string.Empty;
          List<String> listId = new List<String>();
          if(!String.IsNullOrEmpty(args.FieldValue))
          {
            rowValue = args.FieldValue;
            args.RawParameters = rowValue;
            listId.AddRange(rowValue.Split("|".ToCharArray()));
            foreach (string tId in listId)
            {
              string span;
              tagId = StringUtil.GetPrefix(tId, ':');
              tagWeight = StringUtil.GetPostfix(tId, ':');
              if ((tagId != "Null") && (tagId != ID.Null.ToString()))
              {
                categoryItem = new CategoryItem(db.GetItem(new ID(tagId)));
                span = "<span class='tagSet' tagId='" + tagId + "' weightId='" + tagWeight + "' onclick='tag_select(event);' ondblclick='showAlert(\"" + tagId + "\")'>" + categoryItem.CategoryName + "<wbr /></span>;";
              }
              else
              {
                  span = "<span class='tagNotFound' tagId='" + tagId + "'>" + tagWeight + "<wbr /></span>;";
              }
              htmlValue += span; 
            }
             
          }
          return htmlValue;
        }
    }
}