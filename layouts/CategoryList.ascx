<%@ Control Language="c#" AutoEventWireup="true" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Import Namespace="Sitecore.Data.Items" %>
<%@ Import Namespace="Sitecore.Data" %>
<%@ Import Namespace="Sitecore.Text" %>
<%@ Import Namespace="Sitecore.StringExtensions" %>
<script runat="server">
  
  private bool IsCategorySelected(Item category)
  {
    return Sitecore.StringUtil.GetString(Request.QueryString["cat"]).IndexOf(ShortID.Encode(category.ID), StringComparison.InvariantCultureIgnoreCase) >= 0;
  }
  
  private string GetUrlAddCategory(Item category)
  {
    var url = new UrlString(Request.RawUrl);
    url["cat"] = Sitecore.StringUtil.GetString(HttpUtility.UrlDecode(url["cat"])) + "|" + ShortID.Encode(category.ID);
    return url.ToString();
  }
  
  private string GetUrlRemoveCategory(Item category)
  {
    var url = new UrlString(Request.RawUrl);
    url["cat"] = Sitecore.StringUtil.GetString(HttpUtility.UrlDecode(url["cat"])).Replace(ShortID.Encode(category.ID), string.Empty).Replace("||", "|");
    return url.ToString();
  } 

  private void RenderCategory(Item category)
  {
    Response.Write("<li>");
    if (this.IsCategorySelected(category))
    {
      Response.Write("<b><a href=\"{0}\">{1}</a></b>".FormatWith(this.GetUrlRemoveCategory(category), category.Name));
    }
    else
    {
      Response.Write(
        "<a href=\"{0}\">{1}</a>".FormatWith(this.GetUrlAddCategory(category), category.Name));
    }
    if (category.HasChildren)
    {
      Response.Write("<ul>");
      foreach (Item child in category.Children)
      {
        this.RenderCategory(child);
      }
      Response.Write("</ul>");
    }
    Response.Write("</li>");
  }


</script>
<style type="text/css">
    ul li a:link,
    ul li a:visited,
    ul li a:hover,
    ul li a:active {
      color:blue;
    }
</style>
<%
  foreach (Item taxonomy in Sitecore.Context.Database.GetItem("/sitecore/content/Classification/Taxonomies").Children)
  {
    if (!taxonomy.HasChildren)
    {
      continue;
    }
    Response.Write("<p>");
    Response.Write("<h3>" + taxonomy.Name + "</h3>");
    Response.Write("<ul>");
    foreach (Item category in taxonomy.Children)
    {
      this.RenderCategory(category);
    }
    Response.Write("</ul>");
    Response.Write("</p>");
  }
   %>