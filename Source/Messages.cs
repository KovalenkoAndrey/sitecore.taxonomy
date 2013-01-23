namespace Sitecore.Taxonomies
{
  /// <summary>
  /// Contains messages used in the taxonomy UI elements.
  /// </summary>
  public static class Messages
  {
    /// <summary>
    /// Gets CategoryConflictsWithAlreadyAssigned message.
    /// </summary>
    public static string CategoryConflictsWithAlreadyAssigned
    {
      get
      {
        return "{0} conflicts with already assigned category. Please choose another category or remove the conflicting ones:\n{1}";
      }
    }

    /// <summary>
    /// Gets CategoryConflictsWithAlreadyAssignedMoreThanThree message.
    /// </summary>
    public static string CategoryConflictsWithAlreadyAssignedMoreThanThree
    {
      get
      {
        return "{0} conflicts with already assigned category. Please choose another category or remove the conflicting ones:\n{1}And {2} more.";
      }
    }
  }
}