using System.Collections.Generic;

namespace IL.View.Controls.CodeView
{
  internal interface ILanguageRepository
  {
    IEnumerable<ILanguage> All { get; }
    ILanguage FindById(string languageId);
    void Load(ILanguage language);
  }
}
