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

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using IL.View.Model;
using Mono.Cecil;

namespace IL.View.Views
{
  public partial class AssemblyFileSelector : ChildWindow
  {
    public AssemblyDefinition Definition { get; private set; }
    private readonly AssemblyNameReference _reference;

    public AssemblyFileSelector(AssemblyNameReference reference)
    {
      InitializeComponent();
      _reference = reference;
      DataContext = reference;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      Definition = null;
      DialogResult = false;
    }

    private void BrowseAssembly(object sender, RoutedEventArgs e)
    {
      var dialog = new OpenFileDialog { Filter = "Assembly Files(*.exe, *.dll)|*.exe;*.dll" };
      if (dialog.ShowDialog() == false)
      {
        Definition = null;
        DialogResult = false;
        return;
      }

      Definition = AssemblyDefinition.ReadAssembly(dialog.File.OpenRead());
      if (Definition == null) Debugger.Break();

      if (Definition.FullName == _reference.FullName)
      {
        if (Definition.IsSilverlight())
          StorageService.CacheSilverlightAssembly(dialog.File.Name, dialog.File.OpenRead());
        else
          StorageService.CacheNetAssembly(dialog.File.Name, dialog.File.OpenRead());

        ApplicationModel.Current.AssemblyCache.AddAssembly(Definition);

        DialogResult = true;
      }
    }
  }
}

