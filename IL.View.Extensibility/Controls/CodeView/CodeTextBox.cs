using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace IL.View.Controls.CodeView
{
  [TemplatePart(Name = TextBoxName, Type = typeof(RichTextBox))]
  public class CodeTextBox : Control, ICodeView
  {
    private static readonly Type ThisType = typeof(CodeTextBox);
    /// <summary>
    /// Shared static color coding system instance.
    /// </summary>
    private static WeakReference _colorizer;

    /// <summary>
    /// The name of the text block part.
    /// </summary>
    private const string TextBoxName = "TextBox";

    private RichTextBox _textBox;

    public CodeTextBox()
    {
      DefaultStyleKey = typeof(CodeTextBox);
    }

    #region DefaultStyleSheet
    public static readonly DependencyProperty DefaultStyleSheetProperty =
      DependencyProperty.Register("DefaultStyleSheet", typeof(IStyleSheet), ThisType, new PropertyMetadata(StyleSheets.IL, OnDefaultStyleSheetChanged));

    private static void OnDefaultStyleSheetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var source = d as CodeTextBox;
      source.HighlightContents();
    }

    public IStyleSheet DefaultStyleSheet
    {
      get { return (IStyleSheet)GetValue(DefaultStyleSheetProperty); }
      set { SetValue(DefaultStyleSheetProperty, value); }
    }
    #endregion

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
            new PropertyMetadata(SourceLanguageType.IL, OnSourceLanguagePropertyChanged));

    /// <summary>
    /// SourceLanguageProperty property changed handler.
    /// </summary>
    /// <param name="d">SyntaxHighlightingTextBlock that changed its SourceLanguage.</param>
    /// <param name="e">Event arguments.</param>
    private static void OnSourceLanguagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var source = d as CodeTextBox;

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
      var source = d as CodeTextBox;
      source.HighlightContents();
      source.ScrollToStart();
    }
    #endregion public string SourceCode

    private void ScrollToStart()
    {
      if (_textBox == null) return;
      var block = _textBox.Blocks.FirstOrDefault();
      if (block == null) return;
      _textBox.Selection.Select(block.ElementStart, block.ElementStart);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _textBox = GetTemplateChild(TextBoxName) as RichTextBox;
      if (_textBox != null && !string.IsNullOrEmpty(SourceCode))
      {
        HighlightContents();
      }
    }

    /// <summary>
    /// Clears and updates the contents.
    /// </summary>
    private void HighlightContents()
    {
      if (_textBox != null)
      {
        _textBox.Blocks.Clear();
        _textBox.Blocks.Add(new Paragraph());

        XamlInlineFormatterEx xif = new XamlInlineFormatterEx(_textBox);

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
