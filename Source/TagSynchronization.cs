using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Caching;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Publishing.Pipelines.Publish;
using Sitecore.SecurityModel;
using Sitecore.Diagnostics;

namespace Sitecore.Taxonomies.Source
{
  public class TagSynchronization
  {
    protected const string tagTemplateID = "{FCE30413-F1D8-43F4-8B93-B51FDED4D251}";
    protected const string conflictFieldName = "ConflictTags";
    protected const string SitePublisher = "publisher";

    protected void OnItemSaved(object sender, EventArgs args)
    {
      if (Sitecore.Context.GetSiteName() != SitePublisher)
      {
        Item item = Event.ExtractParameter(args, 0) as Item;
        Error.AssertNotNull(item, "No item in parameters");

        var currentDb = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
        if ((item != null) && (item.TemplateID.ToString() == tagTemplateID) && (currentDb != null))
        {
          List<ID> conflictTagIds = new List<ID>();
          if (!string.IsNullOrEmpty(item[conflictFieldName]))
            conflictTagIds = item[conflictFieldName].Split('|').Select(id => new ID(id)).ToList();

          List<ID> assignConflictIds = GetAssignConflict(item.ID).ToList();

          SyncConflicts(conflictTagIds, assignConflictIds, item.ID);
        }
      }
    }

    protected void OnItemDeleted(object sender, EventArgs args)
    {
      if (Sitecore.Context.GetSiteName() != SitePublisher)
      {
        Item item = Event.ExtractParameter(args, 0) as Item;
        Error.AssertNotNull(item, "No item in parameters");
        if (item != null && item.TemplateID.ToString() == tagTemplateID)
        {
          List<ID> assignConflictIds = GetAssignConflict(item.ID).ToList();
          foreach (ID assignTagId in assignConflictIds)
          {
            EditConflicts(assignTagId, item.ID, false);
          }
        }
      }
  }
    
    protected void SyncConflicts(List<ID> conflictTagIds, List<ID> assignConflictIds, ID tagID)
    {
      foreach (ID conflictTagId in conflictTagIds)
      {
        if (!assignConflictIds.Contains(conflictTagId))
          EditConflicts(conflictTagId, tagID, true);
      }

      foreach (ID assignTagId in assignConflictIds)
      {
        if (!conflictTagIds.Contains(assignTagId))
          EditConflicts(assignTagId, tagID, false);
      }
    }

    protected IEnumerable<ID> GetAssignConflict(ID tagId)
    {
      Database currentDb = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
      if (currentDb != null)
      {
        string query = string.Format("/sitecore/content/classification/taxonomies//*[contains(@ConflictTags,'{0}')]",
          tagId.ToString());

        return currentDb.SelectItems(query).Select(item => item.ID);
      }

      return null;
    }

    protected void EditConflicts(ID tagID, ID conflictID, bool addConflict)
    {
      Database currentDb = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
      if (currentDb != null)
      {
        Item editItem = currentDb.GetItem(tagID);
        if (editItem != null)
        {
          List<string> conflictTags = new List<string>();
          if (string.IsNullOrEmpty(editItem[conflictFieldName]) && addConflict)
          {
            conflictTags.Add(conflictID.ToString());
          }
          else
          {
            conflictTags.AddRange(editItem[conflictFieldName].Split('|').ToList());

            if (addConflict)
              conflictTags.Add(conflictID.ToString());
            else
              conflictTags.Remove(conflictID.ToString());
          }

          using (new SecurityDisabler())
          {
            using (new EditContext(editItem, true, true))
            {
              editItem[conflictFieldName] = StringUtil.Join(conflictTags, "|");
            }

            ItemCache cache = CacheManager.GetItemCache(Client.ContentDatabase);
            cache.RemoveItem(editItem.ID);
          }
        }
      }
    }
  }
}