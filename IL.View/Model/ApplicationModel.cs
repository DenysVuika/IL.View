/*
 * The MIT License
 * 
 * Copyright © 2011, Denys Vuika
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * */

using System;
using System.Linq;
using Mono.Cecil;
using System.ComponentModel;

namespace IL.View.Model
{
  public class ApplicationModel : INotifyPropertyChanged
  {
    private static ApplicationModel _current;
    public static ApplicationModel Current
    {
      get { return _current ?? (_current = new ApplicationModel()); }
    }

    public AssemblyCache AssemblyCache { get; private set; }

    public event EventHandler CurrentLanguageChanged;

    private void OnCurrentLanguageChanged()
    {
      var handler = CurrentLanguageChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }

    private LanguageInfoList _availableLanguages = new LanguageInfoList();

    public LanguageInfoList AvailableLanguages
    {
      get { return _availableLanguages; }
    }

    private LanguageInfo _currentLanguage = LanguageInfoList.IL;
    public LanguageInfo CurrentLanguage
    {
      get { return _currentLanguage; }
      set
      {
        if (_currentLanguage == value) return;
        _currentLanguage = value;
        OnPropertyChanged("CurrentLanguage");
        OnCurrentLanguageChanged();
      }
    }

    private ApplicationModel()
    {
      AssemblyCache = new AssemblyCache();
    }
       
    // TODO: Check the performance
    public MethodDefinition FindMethodDefinition(Uri codeUri)
    {
      // Complex Example
      //code://ClassLibrary1:1.0.0.0/ClassLibrary1.GenericClass3<,,>/GenericMethod<,,>(<!!0>,<!!1>,<!!2>)

      if (!codeUri.Scheme.Equals("code", StringComparison.OrdinalIgnoreCase)) return null;

      var assemblyPart = codeUri.Host;

      var typeParts = codeUri.LocalPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
      if (typeParts.Length != 2) return null;

      var assemblyDefinition = AssemblyCache.Assemblies.FirstOrDefault(asm => asm.GetCodeUriPart().Equals(assemblyPart, StringComparison.OrdinalIgnoreCase));
      if (assemblyDefinition == null) return null;

      var typeDefinition = assemblyDefinition.MainModule.Types.FirstOrDefault(t => t.GetCodeUriPart().Equals(typeParts[0]));
      if (typeDefinition == null) return null;

      var methodDefinition = typeDefinition.Methods.FirstOrDefault(m => m.GetCodeUriPart().Equals(typeParts[1]));

      return methodDefinition;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
