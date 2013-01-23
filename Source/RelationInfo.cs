using System.Collections.Generic;
namespace Sitecore.Data.Taxonomy
{
  /// <summary>
  /// Dynamic information about 
  /// </summary>
  public class RelationInfo 
  {
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    public ID Category { get; set; }
    /// <summary>
    /// Gets or sets the ID of the related category.
    /// </summary>
    /// <value>The ID of the related category.</value>
    public List<ID> RelatedCategory { get; set; }
    /// <summary>
    /// Gets or sets the ID of the conflict categories
    /// </summary>
    public List<ID> ConflictCategory { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="RelationInfo"/> is calculated.
    /// </summary>
    /// <value><c>true</c> if calculated; otherwise, <c>false</c>.</value>
    public bool Calculated { get; set; }
    /// <summary>
    /// Gets or sets the weight.
    /// </summary>
    /// <value>The weight.</value>
    public float Weight { get; set; }
  }
}

