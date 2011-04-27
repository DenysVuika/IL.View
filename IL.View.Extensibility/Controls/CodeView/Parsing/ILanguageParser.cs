using System;
using System.Collections.Generic;

namespace IL.View.Controls.CodeView
{
  internal interface ILanguageParser
  {
    void Parse(string sourceCode, ILanguage language, Action<string, IList<Scope>> parseHandler);
  }
}
