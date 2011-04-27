using System;

namespace IL.View.Controls.CodeView
{
  public interface ICodeView
  {
    IStyleSheet DefaultStyleSheet { get; set; }
    SourceLanguageType SourceLanguage { get; set; }
    string SourceCode { get; set; }

    [Obsolete("For dev purposes only")]
    object Tag { get; set; }
  }
}
