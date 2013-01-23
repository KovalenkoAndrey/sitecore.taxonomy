<%@ Control Language="c#" AutoEventWireup="true" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Import Namespace="Sitecore.Data.Taxonomy" %>
<%@ Import Namespace="Sitecore.StringExtensions" %>
<%@ Import Namespace="Sitecore.Links" %>
<%
  
  Response.Write("<ul>");
  foreach (var item in TaxonomyEngine.GetRelatedDocuments(Sitecore.Context.Item))
  {
    Response.Write("<li>");
    Response.Write("<a href=\"{0}\">{1}</a>".FormatWith(LinkManager.GetItemUrl(item), item.DisplayName));
    Response.Write("</li>");
  }
  Response.Write("</ul>");
  
   %>