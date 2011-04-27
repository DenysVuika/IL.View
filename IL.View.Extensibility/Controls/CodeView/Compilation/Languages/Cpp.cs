using System.Collections.Generic;

namespace IL.View.Controls.CodeView
{
  internal class Cpp : ILanguage
  {
    public string Id
    {
      get { return LanguageId.Cpp; }
    }

    public string Name
    {
      get { return "C++"; }
    }

    public string FirstLinePattern
    {
      get { return null; }
    }

    public IList<LanguageRule> Rules
    {
      get
      {
        return new List<LanguageRule>
        {
            new LanguageRule(
                @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                new Dictionary<int, string>
                    {
                        { 0, ScopeName.Comment },
                    }),
            new LanguageRule(
                @"(//.*?)\r?$",
                new Dictionary<int, string>
                    {
                        { 1, ScopeName.Comment }
                    }),
            new LanguageRule(
                @"(?s)(""[^\n]*?(?<!\\)"")",
                new Dictionary<int, string>
                    {
                        { 0, ScopeName.String }
                    }),
            new LanguageRule(
                @"\b(auto|bool|break|case|catch|char|class|const|const_cast|continue|default|delete|do|double|dynamic_cast|else|enum|explicit|export|extern|false|float|for|friend|goto|if|inline|int|long|mutable|namespace|new|operator|private|protected|public|register|reinterpret_cast|return|short|signed|sizeof|static|static_cast|struct|switch|template|this|throw|true|try|typedef|typeid|typename|union|unsigned|using|virtual|void|volatile|wchar_t|while)\b",
                new Dictionary<int, string>
                    {
                        {0, ScopeName.Keyword},
                    }),
        };
      }
    }
  }
}
