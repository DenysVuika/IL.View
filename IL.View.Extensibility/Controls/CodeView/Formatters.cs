using System;

namespace IL.View.Controls.CodeView
{
  /// <summary>
  /// Provides easy access to ColorCode's built-in formatters.
  /// </summary>
  internal static class Formatters
  {
    /// <summary>
    /// Gets the default formatter.
    /// </summary>
    /// <remarks>
    /// The default formatter produces HTML with inline styles.
    /// </remarks>
    public static IFormatter Default
    {
      get
      {
        //    return /*new HtmlFormatter()*/ null; 
        throw new InvalidOperationException();
      }
    }
  }
}
