namespace IL.View.Controls.CodeView
{
  /// <summary>
  /// Provides easy access to ColorCode's built-in style sheets.
  /// </summary>
  internal static class StyleSheets
  {
    private static IStyleSheet _default;
    private static IStyleSheet _il;
    private static IStyleSheet _csharp;

    /// <summary>
    /// Gets the default style sheet.
    /// </summary>
    /// <remarks>
    /// The default style sheet mimics the default colorization scheme used by Visual Studio 2008 to the extent possible.
    /// </remarks>
    public static IStyleSheet Default
    {
      get { return _default ?? (_default = new DefaultStyleSheet()); }
    }

    public static IStyleSheet IL
    {
      get { return _il ?? (_il = new ILCodeStyleSheet()); }
    }

    public static IStyleSheet CSharp
    {
      get { return _csharp ?? (_csharp = new DefaultStyleSheet()); }
    }
  }
}
