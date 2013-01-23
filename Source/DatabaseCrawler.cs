// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseCrawler.cs" company="Sitecore">
//   Copyright (c) Sitecore. All rights reserved.
// </copyright>
// <summary>
//   Implements crawler that scans and watches a specific location in the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.Search.Crawlers;

namespace Sitecore.Taxonomies
{
  #region Namespaces

  using Data.Taxonomy;

  using Lucene.Net.Documents;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;

  #endregion

  /// <summary>
  /// Implements crawler that scans and watches a specific location in the database.
  /// </summary>
  public class Crawler : DatabaseCrawler
  {

    /// <summary>
    /// Adds all fields.
    /// </summary>
    /// <param name="document">
    /// The document.
    /// </param>
    /// <param name="item">
    /// The item.
    /// </param>
    /// <param name="versionSpecific">
    /// Ignored, must always be true.
    /// </param>
    protected override void AddAllFields([NotNull] Document document, [NotNull] Item item, bool versionSpecific)
    {
      Assert.ArgumentNotNull(document, "document");
      Assert.ArgumentNotNull(item, "item");

      base.AddAllFields(document, item, versionSpecific);

      if (item.Name == "__Standard Values")
      {
        return;
      }

      var fields = item.Fields;
      fields.ReadAll();
      foreach (Data.Fields.Field field in fields)
      {
        if (string.IsNullOrEmpty(field.Key))
        {
          continue;
        }

        if (field.Type == "Classification")
        {
          var relations = TaxonomyEngine.GetAllCategories(field.Item, field.Value);
          foreach (var info in relations.Values)
          {
            document.Add(CreateTextField("_categories", ShortID.Encode(info.Category)));
            continue;
          }
        }
      }
    }
  }
}
