using System;
using System.Collections.Generic;

namespace IL.View.Controls.CodeView
{
  internal class LanguageRepository : ILanguageRepository
  {
    private readonly Dictionary<string, ILanguage> loadedLanguages;

    public LanguageRepository(Dictionary<string, ILanguage> loadedLanguages)
    {
      this.loadedLanguages = loadedLanguages;
    }

    public IEnumerable<ILanguage> All
    {
      get { return loadedLanguages.Values; }
    }

    public ILanguage FindById(string languageId)
    {
      Guard.ArgNotNullAndNotEmpty(languageId, "languageId");

      ILanguage language = null;

      if (loadedLanguages.TryGetValue(languageId, out language))
        return language;
            
      return null;
    }

    public void Load(ILanguage language)
    {
      Guard.ArgNotNull(language, "language");

      if (string.IsNullOrEmpty(language.Id))
        throw new ArgumentException("The language identifier must not be null or empty.", "language");

      loadedLanguages[language.Id] = language;
    }
  }
}
