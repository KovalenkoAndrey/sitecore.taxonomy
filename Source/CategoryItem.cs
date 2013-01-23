namespace Sitecore.Data.Taxonomy
{
  using System.Globalization;
  using Items;

  /// <summary>
  /// 
  /// </summary>
  public class CategoryItem : CustomItem
  {
    /// <summary>
    /// The fields of Category template
    /// </summary>
    public static class Fields
    {
      /// <summary>
      /// Own weight multiplier of the category. Higher for specialized categories.
      /// </summary>
      public static readonly ID Weight = ID.Parse("{4E7C19E6-94D9-45C3-80A3-7BA8EA200BC9}");
      /// <summary>
      /// Child relation multiplier.
      /// </summary>
      public static readonly ID RelatedWithChildren = ID.Parse("{B7E33328-D77F-4050-80F9-672BD7D0C834}");
      /// <summary>
      /// Parent relation multiplier.
      /// </summary>
      public static readonly ID RelatedToParent = ID.Parse("{2024F39E-68CA-4071-95BA-A57DA4C4FB57}");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryItem"/> class.
    /// </summary>
    /// <param name="innerItem">Inner item.</param>
    public CategoryItem(Item innerItem)
      : base(innerItem)
    {
    }

    /// <summary>
    /// Gets the weight multiplier.
    /// </summary>
    /// <value>The weight.</value>
    public float Weight
    {
      get
      {
        float result;
        if (float.TryParse(InnerItem[Fields.Weight], NumberStyles.None, CultureInfo.InvariantCulture, out result))
        {
          return result;
        }
        return 0f;
      }
    }

    /// <summary>
    /// Gets the child relation multiplier (K). When a child is applied with weight W and relation to parent P, this category is applied with weight sqrt(K x P) x W.
    /// </summary>
    /// <value>The relation with children.</value>
    public float RelatedWithChildren
    {
      get
      {
        float result;
        string relatedWithChildren = InnerItem[Fields.RelatedWithChildren].Replace(',', '.');
        if (float.TryParse(relatedWithChildren, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result))
        {
          return result;
        }
        return 0f;
      }
    }

    /// <summary>
    /// Gets the parent relation multiplier (P). When a parent is applied with weight W and relation to child K, this category is applied with weight sqrt(P x K) x W.
    /// </summary>
    /// <value>The relation to parent.</value>
    public float RelatedToParent
    {
      get
      {
        float result;
        string relatedToParent = InnerItem[Fields.RelatedToParent].Replace(',', '.');
        if (float.TryParse(relatedToParent, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result))
        {
          return result;
        }
        return 0f;
      }
    }

    /// <summary>
    /// Gets the parent category or null if this is a top-level category.
    /// </summary>
    /// <value>The parent category.</value>
    [CanBeNull]
    public CategoryItem Parent
    {
      get
      {
        Item parent = InnerItem.Parent;
        if (parent != null && InnerItem.TemplateID.Equals(parent.TemplateID))
        {
          return new CategoryItem(parent);
        }
        return null;
      }
    }

    /// <summary>
    /// Gets the taxonomy of the current category or null of this category is not a part of a taxonomy.
    /// </summary>
    /// <value>The taxonomy.</value>
    [CanBeNull]
    public Item Taxonomy
    {
      get
      {
        Item item = InnerItem.Parent;
        while (item != null)
        {
          if (item.TemplateName == "Taxonomy")
          {
            return item;
          }
        }
        return null;
      }
    }

    /// <summary>
    /// Gets the full name of the category.
    /// </summary>
    /// <value>The category full name.</value>
    public string CategoryName
    {
      get { return GetCategoryName(InnerItem); }
    }

    private string GetCategoryName(Item categoryItem)
    {
      string categoryName = categoryItem.DisplayName;
      if (!categoryItem.Parent.TemplateID.ToString().Equals("{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}"))//!categoryItem.Parent.Name.Equals("Taxonomies")
      {
        categoryName = this.GetCategoryName(categoryItem.Parent) + "/" + categoryName;
      }
      return categoryName;
    }
  }
}