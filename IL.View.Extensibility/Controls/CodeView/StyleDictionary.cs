using System.Collections.ObjectModel;

namespace IL.View.Controls.CodeView
{
  /// <summary>
  /// A dictionary of <see cref="Style" /> instances, keyed by the styles' scope name.
  /// </summary>
  public class StyleDictionary : KeyedCollection<string, Style>
  {
    /// <summary>
    /// When implemented in a derived class, extracts the key from the specified element.
    /// </summary>
    /// <param name="item">The element from which to extract the key.</param>
    /// <returns>The key for the specified element.</returns>
    protected override string GetKeyForItem(Style item)
    {
      return item.ScopeName;
    }
  }
}
