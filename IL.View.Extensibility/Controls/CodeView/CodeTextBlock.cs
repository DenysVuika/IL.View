using System;
using System.Windows;
using System.Windows.Controls;

namespace IL.View.Controls.CodeView
{
  /// <summary>
  /// A simple text control for displaying syntax highlighted source code.
  /// </summary>
  [TemplatePart(Name = TextBlockName, Type = typeof(TextBlock))]
  public class CodeTextBlock : Control, ICodeView
  {
    private static readonly Type ThisType = typeof(CodeTextBlock);

    /// <summary>
    /// Shared static color coding system instance.
    /// </summary>
    private static WeakReference _colorizer;

    /// <summary>
    /// The name of the text block part.
    /// </summary>
    private const string TextBlockName = "TextBlock";

    /// <summary>
    /// Backing field for the text block.
    /// </summary>
    private TextBlock _textBlock;

    public static readonly DependencyProperty DefaultStyleSheetProperty =
      DependencyProperty.Register("DefaultStyleSheet", typeof(IStyleSheet), ThisType, new PropertyMetadata(StyleSheets.IL, OnDefaultStyleSheetChanged));

    private static void OnDefaultStyleSheetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var source = d as CodeTextBlock;
      source.HighlightContents();
    }

    public IStyleSheet DefaultStyleSheet
    {
      get { return (IStyleSheet)GetValue(DefaultStyleSheetProperty); }
      set { SetValue(DefaultStyleSheetProperty, value); }
    }

    #region public SourceLanguageType SourceLanguage
    /// <summary>
    /// Gets or sets the source language type.
    /// </summary>
    public SourceLanguageType SourceLanguage
    {
      get { return (SourceLanguageType)GetValue(SourceLanguageProperty); }
      set { SetValue(SourceLanguageProperty, value); }
    }

    /// <summary>
    /// Identifies the SourceLanguage dependency property.
    /// </summary>
    public static readonly DependencyProperty SourceLanguageProperty =
        DependencyProperty.Register(
            "SourceLanguage",
            typeof(SourceLanguageType),
            ThisType,
            new PropertyMetadata(SourceLanguageType.CSharp, OnSourceLanguagePropertyChanged));

    /// <summary>
    /// SourceLanguageProperty property changed handler.
    /// </summary>
    /// <param name="d">SyntaxHighlightingTextBlock that changed its SourceLanguage.</param>
    /// <param name="e">Event arguments.</param>
    private static void OnSourceLanguagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var source = d as CodeTextBlock;

      switch ((SourceLanguageType)e.NewValue)
      {
        case SourceLanguageType.CSharp:
          source.DefaultStyleSheet = StyleSheets.CSharp;
          break;
        case SourceLanguageType.IL:
          source.DefaultStyleSheet = StyleSheets.IL;
          break;
        default:
          source.DefaultStyleSheet = StyleSheets.Default;
          break;
      }

      source.HighlightContents();

    }
    #endregion public SourceLanguageType SourceLanguage

    #region public string SourceCode
    /// <summary>
    /// Gets or sets the source code to display inside the syntax
    /// highlighting text block.
    /// </summary>
    public string SourceCode
    {
      get { return GetValue(SourceCodeProperty) as string; }
      set { SetValue(SourceCodeProperty, value); }
    }

    /// <summary>
    /// Identifies the SourceCode dependency property.
    /// </summary>
    public static readonly DependencyProperty SourceCodeProperty =
        DependencyProperty.Register(
            "SourceCode",
            typeof(string),
            ThisType,
            new PropertyMetadata(string.Empty, OnSourceCodePropertyChanged));

    /// <summary>
    /// SourceCodeProperty property changed handler.
    /// </summary>
    /// <param name="d">SyntaxHighlightingTextBlock that changed its SourceCode.</param>
    /// <param name="e">Event arguments.</param>
    private static void OnSourceCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var source = d as CodeTextBlock;
      source.HighlightContents();
    }
    #endregion public string SourceCode



    /// <summary>
    /// Initializes a new instance of the SyntaxHighlightingTextBlock
    /// control.
    /// </summary>
    public CodeTextBlock()
    {
      DefaultStyleKey = typeof(CodeTextBlock);
    }

    /// <summary>
    /// Overrides the on apply template method.
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _textBlock = GetTemplateChild(TextBlockName) as TextBlock;
      if (_textBlock != null && !string.IsNullOrEmpty(SourceCode))
      {
        HighlightContents();
      }
    }

    /// <summary>
    /// Clears and updates the contents.
    /// </summary>
    private void HighlightContents()
    {
      if (_textBlock != null)
      {
        _textBlock.Inlines.Clear();
        XamlInlineFormatter xif = new XamlInlineFormatter(_textBlock);

        CodeColorizer cc;
        if (_colorizer != null && _colorizer.IsAlive)
        {
          cc = (CodeColorizer)_colorizer.Target;
        }
        else
        {
          cc = new CodeColorizer();
          _colorizer = new WeakReference(cc);
        }

        ILanguage language = CreateLanguageInstance(SourceLanguage);

        cc.Colorize(SourceCode, language, xif, DefaultStyleSheet);
      }
    }

    /// <summary>
    /// Retrieves the language instance used by the highlighting system.
    /// </summary>
    /// <param name="type">The language type to create.</param>
    /// <returns>Returns a new instance of the language parser.</returns>
    private ILanguage CreateLanguageInstance(SourceLanguageType type)
    {
      switch (type)
      {
        case SourceLanguageType.CSharp:
          return Languages.CSharp;

        case SourceLanguageType.Cpp:
          return Languages.Cpp;

        case SourceLanguageType.JavaScript:
          return Languages.JavaScript;

        case SourceLanguageType.VisualBasic:
          return Languages.VbDotNet;

        case SourceLanguageType.Xaml:
        case SourceLanguageType.Xml:
          return Languages.Xml;

        case SourceLanguageType.IL:
          return Languages.IL;

        default:
          throw new InvalidOperationException("Could not locate the provider.");
      }
    }
  }
}
