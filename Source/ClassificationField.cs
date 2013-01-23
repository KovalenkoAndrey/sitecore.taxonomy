namespace Sitecore.Data.Fields
{
  using System.Collections.Generic;
  using System.Linq;

  using Diagnostics;

  /// <summary>
  /// Represents weight-based classification
  /// </summary>
  public class ClassificationField : CustomField
  {
    #region Constants and Fields

    private bool scheduleUpdate;

    private Dictionary<ID, ID> weights;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassificationField"/> class.
    /// </summary>
    /// <param name="innerField">Inner field.</param>
    public ClassificationField([NotNull] Field innerField)
      : base(innerField)
    {
      Assert.ArgumentNotNull(innerField, "innerField");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassificationField"/> class.
    /// </summary>
    /// <param name="innerField">The inner field.</param>
    /// <param name="runtimeValue">The runtime value.</param>
    public ClassificationField([NotNull] Field innerField, [NotNull] string runtimeValue)
      : base(innerField, runtimeValue)
    {
      Assert.ArgumentNotNull(innerField, "innerField");
      Assert.ArgumentNotNull(runtimeValue, "runtimeValue");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the weights.
    /// </summary>
    /// <value>The weights.</value>
    [NotNull]
    private Dictionary<ID, ID> Weights
    {
      get
      {
        if (this.weights != null)
        {
          return this.weights;
        }
        this.weights = Parse(this.GetValue());
        return this.weights;
      }
    }

    #endregion

    #region Indexers

    /// <summary>
    /// Returns weight the specified tag.
    /// </summary>
    /// <value></value>
    public ID this[[NotNull] ID id]
    {
      get
      {
        Assert.ArgumentNotNull(id, "id");

        ID result;
        if (this.Weights.TryGetValue(id, out result))
        {
          return result;
        }
        return ID.Null;
      }
      set
      {
        Assert.ArgumentNotNull(id, "id");

        this.Weights[id] = value;
        this.scheduleUpdate = true;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <returns>The value.</returns>
    protected override string GetValue()
    {
      if (this.scheduleUpdate)
      {
        base.SetValue(Render(this.weights));
        this.scheduleUpdate = false;
      }
      return base.GetValue();
    }

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void SetValue(string value)
    {
      Assert.ArgumentNotNull(value, "value");

      base.SetValue(value);
      this.weights = null;
      this.scheduleUpdate = false;
    }

    #endregion

    /// <summary>
    /// Parses the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    [NotNull]
    public static Dictionary<ID, ID> Parse([NotNull] string value)
    {
      Assert.ArgumentNotNull(value, "value");

      var result = new Dictionary<ID, ID>();

        foreach (var pair in value.Split('|').Select(s => s.Split(":".ToCharArray(), 2)))
        {
          if (pair.Length < 2 || string.IsNullOrEmpty(pair[0]) || !ID.IsID(pair[0]) || string.IsNullOrEmpty(pair[1]) || !ID.IsID(pair[1]))
          {
            continue;
          }

          try
          {
            result[ID.Parse(pair[0])] = ID.Parse(pair[1]);
          } 
          catch(System.FormatException)
          {
            // Format error. Recover by skipping broken numbers
          }
        }
      return result;
    }

    /// <summary>
    /// Renders the specified weights.
    /// </summary>
    /// <param name="weights">The weights.</param>
    /// <returns></returns>
    [NotNull]
    public static string Render([NotNull] Dictionary<ID, ID> weights)
    {
      Assert.ArgumentNotNull(weights, "weights");

      var value = string.Join(
        "|",
        weights.Where(pair => pair.Value != ID.Null).Select(pair => pair.Key.ToString() + ":" + pair.Value).ToArray());
      return value;
    }
  }
}