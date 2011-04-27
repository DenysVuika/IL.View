namespace IL.View.Controls.CodeView
{
  internal interface ILanguageCompiler
  {
    CompiledLanguage Compile(ILanguage language);
  }
}
