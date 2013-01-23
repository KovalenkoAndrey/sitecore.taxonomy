<%@ Control Language="c#" AutoEventWireup="true" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Import Namespace="Sitecore.Data.Taxonomy" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="Sitecore.Data" %>
<%@ Import Namespace="Sitecore.Links" %>
<%@ Import Namespace="Sitecore.StringExtensions" %>
<%
  var ids = Sitecore.StringUtil.GetString(this.Request.QueryString["cat"]).Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(s => ShortID.IsShortID(s)).Select(s => Sitecore.Data.ID.Parse(ShortID.Decode(s)));
  var items = TaxonomyEngine.GetItemsInCategories(ids);
  if (items.Count() > 0)
  {
     Response.Write("<ul>");
     foreach (var item in items)
     {
        Response.Write("<li>");
        Response.Write("<a href=\"{0}\">{1} ({2})</a>".FormatWith(LinkManager.GetItemUrl(item), item.DisplayName, item.Paths.Path));
        foreach (var link in TaxonomyEngine.GetAllCategories(item, item["Tags"]).Values.OrderBy(info => -info.Weight))
        {
           Response.Write("<span>{0} ({1})</span>".FormatWith(item.Database.Items[link.Category].DisplayName, link.Weight));
        }
        Response.Write("</li>");
     }
     Response.Write("</ul>");
  }
  else
  {
     Response.Write("<p>No documents found</p>");
  }
 %>