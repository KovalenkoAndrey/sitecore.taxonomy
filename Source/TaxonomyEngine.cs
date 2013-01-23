using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Sitecore.Common;
using Sitecore.Search.Crawlers;
using Sitecore.Taxonomies;

namespace Sitecore.Data.Taxonomy
{
  using Diagnostics;

  using Fields;

  using Items;

  using Search;

  using Convert = System.Convert;

  /// <summary>
  /// Represent fields in the standard template related to taxonomy.
  /// </summary>
  public static class TaxonomyFields
  {
    /// <summary>
    /// Field that holds item's categories as specified by the user.
    /// </summary>
    public static readonly ID Categories = ID.Parse("{00000000-0000-0000-0000-000000000000}");

    /// <summary>
    /// Field that holds the recommended categories for inheritors.
    /// </summary>
    public static readonly ID RecommendedCategories = ID.Parse("{0EF2DEE5-9886-4970-94F3-194111786E52}");
  }

  /// <summary>
  /// Implements taxonomy functionality.
  /// </summary>
  public class TaxonomyEngine
  {
    /// <summary>
    /// Gets the inherited proposed categories.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    [NotNull]
    public static string GetInheritedRecommendedCategories([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      var field = item.Fields[TaxonomyFields.RecommendedCategories];
      if (field.HasValue)
      {
        return field.Value;
      }
      if (item.Parent != null)
      {
        return GetInheritedRecommendedCategories(item.Parent);
      }
      return string.Empty;
    }

    /// <summary>
    /// Proposes the categories.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    [NotNull]
    public static Dictionary<ID, ID> GetRecommendedCategories([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      var field = item.Fields[TaxonomyFields.RecommendedCategories];
      if (field.HasValue)
      {
        return ClassificationField.Parse(field.Value);
      }

      Dictionary<ID, ID> weights = ClassificationField.Parse(StringUtil.GetString(field.GetStandardValue()));
      foreach (var pair in ClassificationField.Parse(GetInheritedRecommendedCategories(item)))
      {
        weights[pair.Key] = pair.Value;
      }
      return weights;
    }

    /// <summary>
    /// Gets all categories.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    [NotNull]
    public static Dictionary<ID, RelationInfo> GetAllCategories([NotNull] Item item, [NotNull] string value)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(value, "value");

      Dictionary<ID, RelationInfo> result;
      if (string.IsNullOrEmpty(value))
      {
        result = GetRecommendedCategories(item).ToDictionary(
          pair => pair.Key, pair => new RelationInfo
          {
              Category = pair.Key,
              RelatedCategory = new List<ID> { ID.Null },
              Calculated = true,
              Weight = Convert.ToSingle(item.Database.GetItem(pair.Value)["Weight"]),
              ConflictCategory = GetConflictIDs(item.Database.GetItem(pair.Value)["ConflictTags"]).ToList()
          });
      }
      else
      {
        result = ClassificationField.Parse(value).ToDictionary(
          pair => pair.Key,
          pair =>
            {
                return new RelationInfo
                  {
                      Category = pair.Key,
                      RelatedCategory = new List<ID> { ID.Null },
                      Calculated = false,
                      Weight = Convert.ToSingle(item.Database.GetItem(pair.Value)["Weight"]),
                      ConflictCategory = GetConflictIDs(item.Database.GetItem(pair.Value)["ConflictTags"]).ToList()
                  };
            });
      }
      var queue = new Queue<RelationInfo>(result.Values);
      while (queue.Count > 0)
      {
        RelationInfo info = queue.Dequeue();
        var innerItem = item.Database.GetItem(info.Category);
        if (innerItem == null)
        {
          continue;
        }

        var category = new CategoryItem(innerItem);
        var parentCategory = category.Parent;
        if (parentCategory == null)
        {
          continue;
        }
        RelationInfo parentInfo;
        if (result.TryGetValue(parentCategory.ID, out parentInfo))
        {

            int entry = parentInfo.RelatedCategory.Count;

            float nWeight = (float)Math.Sqrt(parentCategory.RelatedWithChildren * category.RelatedToParent) *
                                 info.Weight;

            parentInfo.Weight = (float)Math.Pow(parentInfo.Weight, entry);
            parentInfo.Weight = (float)Math.Pow(parentInfo.Weight * nWeight, 1.0 / (entry + 1));

            parentInfo.RelatedCategory.Add(category.ID);

        }
        else
        {
          parentInfo = new RelationInfo
                         {
                           Category = parentCategory.ID,
                           RelatedCategory = new List<ID>{category.ID},
                           Calculated = true,
                           Weight =
                             (float) Math.Sqrt(parentCategory.RelatedWithChildren*category.RelatedToParent)*info.Weight
                         };
          result[parentInfo.Category] = parentInfo;
        }
        queue.Enqueue(parentInfo);
      }
      return result;
    }

    /// <summary>
    /// Gets the items in categories.
    /// </summary>
    /// <param name="categories">The categories.</param>
    /// <returns></returns>
    [NotNull]
    public static IEnumerable<Item> GetItemsInCategories([NotNull] IEnumerable<ID> categories)
    {
      Assert.ArgumentNotNull(categories, "categories");

      var index = SearchManager.SystemIndex;
      if (index == null)
      {
        return new Item[0];
      }

      var query = new CombinedQuery();
      foreach (var id in categories)
      {
        query.Add(new FieldQuery("_categories", ShortID.Encode(id)), QueryOccurance.Must);
      }
      SearchResultCollection collection;

      using (var context = index.CreateSearchContext())
      {
        collection = context.Search(query).FetchResults(0, int.MaxValue);
      }
      return collection.Select(result => SearchManager.GetObject(result)).OfType<Item>();
    }

    /// <summary>
    /// Gets the related documents.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    public static IEnumerable<Item> GetRelatedDocuments([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      return GetRelatedDocuments(GetAllCategories(item, item["Tags"]).Values.OrderBy(info => -info.Weight)).Where(result => result != null && result.ID != item.ID);
    }

    /// <summary>
    /// Gets the related documents.
    /// </summary>
    /// <param name="categories">The categories.</param>
    /// <returns></returns>
    [NotNull]
    public static IEnumerable<Item> GetRelatedDocuments([NotNull] IEnumerable<RelationInfo> categories)
    {
      Assert.ArgumentNotNull(categories, "categories");

      var index = SearchManager.SystemIndex;
      if (index == null)
      {
        return new Item[0];
      }

      var query = new CombinedQuery(categories.Select(info => new QueryClause(new FieldQuery("_categories", ShortID.Encode(info.Category)), QueryOccurance.Should)));
      SearchResultCollection collection;

      using (var context = index.CreateSearchContext())
      {
        collection = context.Search(query).FetchResults(0, 10);
      }
      return collection.Select(result => SearchManager.GetObject(result) as Item).Where(item => item != null);
    }

    [NotNull]
    public static IEnumerable<ID> GetConflictTags([NotNull] ID tagId)
    {
      Assert.ArgumentNotNull(tagId, "tagId is null");

      Item tag = Client.ContentDatabase.GetItem(tagId);
      if (tag != null && !string.IsNullOrEmpty(tag["ConflictTags"]))
      {
        return StringUtil.Split(tag["ConflictTags"], '|', true).Select(id => new ID(id));
      }
      else
      {
        return Enumerable.Empty<ID>();
      }
      //string query = string.Format("/sitecore/content/classification/taxonomies//*[contains(@ConflictTags,'{0}')]",
      //                             tagId.ToString());

      //Item[] items = Client.ContentDatabase.SelectItems(query);

      //return items.Select(item => item.ID);
    }

    private static IEnumerable<ID> GetConflictIDs(string conflicts)
    {
        if(!string.IsNullOrEmpty(conflicts))
        {
            return conflicts.Split('|').Select(id => new ID(id));
        }
        else
        {
            return Enumerable.Empty<ID>();
        }
    }
  }
}
