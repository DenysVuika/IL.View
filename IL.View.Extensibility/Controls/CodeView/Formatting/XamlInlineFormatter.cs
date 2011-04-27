using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace IL.View.Controls.CodeView
{
  // CONSIDER: background worker to improve performance, with all UI
  // generation at the end.

  internal class XamlInlineFormatter : IFormatter
  {
    private TextBlock _text;

    public XamlInlineFormatter(Panel panel)
      : this()
    {
      _text = new TextBlock { TextWrapping = TextWrapping.Wrap };
      panel.Children.Add(_text);
    }

    /// <summary>
    /// Initializes a new instance of the XAML inline formatter which will
    /// store the contents of the syntax highlighting results into the
    /// text block instance.
    /// </summary>
    /// <param name="textBlock">The text block.</param>
    public XamlInlineFormatter(TextBlock textBlock)
      : this()
    {
      _text = textBlock;
    }

    protected XamlInlineFormatter()
    {
    }

    public void Write(string parsedSourceCode, IList<Scope> scopes, IStyleSheet styleSheet)
    {
      var styleInsertions = new List<TextInsertion>();

      int offset = 0;
      bool lastScopeWasComment = false;

      foreach (Scope scope in scopes)
      {
        string t = parsedSourceCode.Substring(scope.Index, scope.Length);
        //                    .Replace("\r\n", "\n")
        //                    .Replace("\r", "\n");
        offset = scope.Index + scope.Length;
        if (!string.IsNullOrEmpty(t))
        {
          Inline run = new Run { Text = t.Replace("\r", string.Empty) };
          if (scope != null && styleSheet.Styles.Contains(scope.Name))
          {
            Style style = styleSheet.Styles[scope.Name];
            run.Foreground = new SolidColorBrush(style.Foreground);
            run.FontWeight = style.FontWeight;
          }
          lastScopeWasComment = (scope != null && scope.Name == "Comment");
          _text.Inlines.Add(run);
        }
      }
      string left = parsedSourceCode
          .Substring(offset)
          .Replace("\r\n", "\n")
          .Replace("\r", "\n");
      if (!string.IsNullOrEmpty(left))
      {
        for (int i = left.IndexOf("\n"); i >= 0; i = left.IndexOf("\n"))
        {
          if (i > 0)
          {
            Inline tby = new Run { Text = left.Substring(0, i) };
            _text.Inlines.Add(tby);
          }

          left = left.Substring(i + 1);
          if (lastScopeWasComment)
          {
            lastScopeWasComment = false;
          }
          else
          {
            _text.Inlines.Add(new LineBreak());
          }
        }

        if (!string.IsNullOrEmpty(left))
        {
          Inline nrun = new Run { Text = left };
          _text.Inlines.Add(nrun);
        }
      }
    }

    private static void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
    {
      styleInsertions.Add(new TextInsertion
      {
        Index = scope.Index,
        Scope = scope
      });

      foreach (Scope childScope in scope.Children)
        GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);

      styleInsertions.Add(new TextInsertion
      {
        Index = scope.Index + scope.Length,
        //                Text = ""
      });
    }
  }

  internal class XamlInlineFormatterEx : IFormatter
  {
    private RichTextBox _text;
    private Paragraph _paragraph;      

    /// <summary>
    /// Initializes a new instance of the XAML inline formatter which will
    /// store the contents of the syntax highlighting results into the
    /// text block instance.
    /// </summary>
    /// <param name="textBlock">The text block.</param>
    public XamlInlineFormatterEx(RichTextBox textBlock)
      : this()
    {
      _text = textBlock;
      _paragraph = _text.Blocks.OfType<Paragraph>().FirstOrDefault();
    }

    protected XamlInlineFormatterEx()
    {

    }

    public void Write(string parsedSourceCode, IList<Scope> scopes, IStyleSheet styleSheet)
    {
      var styleInsertions = new List<TextInsertion>();

      int offset = 0;
      bool lastScopeWasComment = false;

      foreach (Scope scope in scopes)
      {
        string t = parsedSourceCode.Substring(scope.Index, scope.Length);
        //                    .Replace("\r\n", "\n")
        //                    .Replace("\r", "\n");
        offset = scope.Index + scope.Length;
        if (!string.IsNullOrEmpty(t))
        {
          Inline run = new Run { Text = t.Replace("\r", string.Empty) };
          if (scope != null && styleSheet.Styles.Contains(scope.Name))
          {
            Style style = styleSheet.Styles[scope.Name];
            run.Foreground = new SolidColorBrush(style.Foreground);
            run.FontWeight = style.FontWeight;
          }
          lastScopeWasComment = (scope != null && scope.Name == "Comment");
          _paragraph.Inlines.Add(run);
        }
      }
      string left = parsedSourceCode
          .Substring(offset)
          .Replace("\r\n", "\n")
          .Replace("\r", "\n");
      if (!string.IsNullOrEmpty(left))
      {
        for (int i = left.IndexOf("\n"); i >= 0; i = left.IndexOf("\n"))
        {
          if (i > 0)
          {
            Inline tby = new Run { Text = left.Substring(0, i) };
            _paragraph.Inlines.Add(tby);
          }

          left = left.Substring(i + 1);
          if (lastScopeWasComment)
          {
            lastScopeWasComment = false;
          }
          else
          {
            _paragraph.Inlines.Add(new LineBreak());
          }
        }

        if (!string.IsNullOrEmpty(left))
        {
          Inline nrun = new Run { Text = left };
          _paragraph.Inlines.Add(nrun);
        }
      }
    }

    private static void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
    {
      styleInsertions.Add(new TextInsertion
      {
        Index = scope.Index,
        Scope = scope
      });

      foreach (Scope childScope in scope.Children)
        GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);

      styleInsertions.Add(new TextInsertion
      {
        Index = scope.Index + scope.Length,
        //                Text = ""
      });
    }
  }
}
