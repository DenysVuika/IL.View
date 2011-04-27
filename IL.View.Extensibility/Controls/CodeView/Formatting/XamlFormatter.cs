using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IL.View.Controls.CodeView
{
  internal class XamlFormatter : IFormatter
  {
    public Panel Panel { get; set; }

    private StackPanel _currentLine;

    public XamlFormatter(Panel panel)
      : this()
    {
      Panel = panel;
      StartNewLine();
    }

    private TextBlock CreateText(string text)
    {
      return new TextBlock
      {
        Text = text,
        FontSize = 11,
        //                Padding = new Thickness(1),
        FontFamily = new FontFamily("Consolas, Courier New, Tahoma"),
      };
    }

    private void StartNewLine()
    {
      _currentLine = new StackPanel { Orientation = Orientation.Horizontal };
      _currentLine.Height = 16;
      Panel.Children.Add(_currentLine);
    }

    protected XamlFormatter()
    {
    }

    public void Write(string parsedSourceCode,
                      IList<Scope> scopes,
                      IStyleSheet styleSheet)
    {
      var styleInsertions = new List<TextInsertion>();

      int offset = 0;

      foreach (Scope scope in scopes)
      {
        string t = parsedSourceCode.Substring(
            scope.Index,
            scope.Length);
        offset = scope.Index + scope.Length;
        if (!string.IsNullOrEmpty(t))
        {
          UIElement e;
          TextBlock tb = CreateText(t);
          e = tb;
          if (scope != null && styleSheet.Styles.Contains(scope.Name))
          {
            Style style = styleSheet.Styles[scope.Name];
            tb.Foreground = new SolidColorBrush(style.Foreground);
            tb.FontWeight = style.FontWeight;

            //if (scope.Name == "Comment")
            //{
            //    tb.Margin = new Thickness(0);
            //    Border b = new Border
            //    {
            //        Margin = new Thickness(-1, -1, -1, 1),
            //        Background = new SolidColorBrush(Colors.Yellow),
            //    };
            //    b.Child = tb;
            //    e = b;
            //}
          }

          _currentLine.Children.Add(e);
        }

      }

      string left = parsedSourceCode.Substring(offset).Replace("\r\n", "\n");
      if (!string.IsNullOrEmpty(left))
      {
        for (int i = left.IndexOf("\n"); i >= 0; i = left.IndexOf("\n"))
        {
          if (i > 0)
          {
            TextBlock tby = CreateText(left.Substring(0, i));
            _currentLine.Children.Add(tby);
          }

          left = left.Substring(i + 1);
          StartNewLine();
        }

        if (!string.IsNullOrEmpty(left))
        {
          _currentLine.Children.Add(CreateText(left));
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

    private static void BuildSpanForCapturedStyle(Scope scope,
                                                    IStyleSheet styleSheet)
    {
      Color foreground = Colors.Black;
      Color background = Colors.Transparent;

      if (styleSheet.Styles.Contains(scope.Name))
      {
        Style style = styleSheet.Styles[scope.Name];

        foreground = style.Foreground;
        background = style.Background;
      }

      WriteElementStart("span", foreground, background);
    }

    private static void WriteHeaderDivEnd()
    {
      WriteElementEnd("div");
    }

    private static void WriteElementEnd(string elementName)
    {
    }

    private static void WriteHeaderPreEnd()
    {
      WriteElementEnd("pre");
    }

    private static void WriteHeaderPreStart()
    {
      WriteElementStart("pre");
    }

    private static void WriteHeaderDivStart(IStyleSheet styleSheet)
    {
      Color foreground = Colors.Transparent;
      Color background = Colors.Transparent;

      if (styleSheet.Styles.Contains(ScopeName.PlainText))
      {
        Style plainTextStyle = styleSheet.Styles[ScopeName.PlainText];

        foreground = plainTextStyle.Foreground;
        background = plainTextStyle.Background;
      }

      WriteElementStart("div", foreground, background);
    }

    private static void WriteElementStart(string elementName)
    {
      WriteElementStart(elementName, Colors.Transparent, Colors.Transparent);
    }

    private static void WriteElementStart(string elementName,
                                          Color foreground,
                                          Color background)
    {
      if (foreground != Colors.Transparent || background != Colors.Transparent)
      {
      }
    }
  }
}
