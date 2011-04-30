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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using IL.View.Model;
using IL.View.Repositories;
using Mono.Cecil;

namespace IL.View.Views
{
  public partial class AssemblyFileSelector : ChildWindow
  {
    private AssemblyDefinition _callingAssembly;

    public AssemblyDefinition Definition { get; private set; }
    private readonly AssemblyNameReference _reference;
    private string _repository;
    private RepositoryClient _repositoryClient = new RepositoryClient();

    public AssemblyFileSelector(AssemblyDefinition callingAssembly, AssemblyNameReference reference)
    {
      if (callingAssembly == null) throw new ArgumentNullException("callingAssembly");
      _callingAssembly = callingAssembly;

      InitializeComponent();
      _reference = reference;
      DataContext = reference;

      var resolver = new RepositoryAssemblySearch(
        RepositorySettings.Current.Definitions.Select(d => d.Address).ToArray(),
        callingAssembly,
        reference,
        OnAssemblySearchComplete);
      resolver.Start();
    }

    private void OnAssemblySearchComplete(string repository, bool result)
    {
      if (result)
      {
        _repository = repository;
        RemoteCheckLabel.Text = "Assembly can be downloaded from repository.";
      }
      else
      {
        spRemoteRepositories.Visibility = Visibility.Collapsed;
      }
      RemoteCheckProgress.IsIndeterminate = false;
      DownloadButton.Visibility = result ? Visibility.Visible : Visibility.Collapsed;
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

    private void OnDownloadAssemblyClick(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(_repository)) return;

      BrowseButton.IsEnabled = false;
      DownloadButton.IsEnabled = false;
      RemoteCheckLabel.Text = "Downloading assembly...";
      RemoteCheckProgress.IsIndeterminate = true;

      _repositoryClient.GetAssembly(_repository, _callingAssembly, _reference, OnAssemblyDownloaded);
    }

    private void OnAssemblyDownloaded(Stream data)
    {
      RemoteCheckProgress.IsIndeterminate = false;

      if (data == null || data.Length == 0) return;

      var definition = AssemblyDefinition.ReadAssembly(data, new ReaderParameters(ReadingMode.Immediate));
      if (definition == null || !definition.FullName.Equals(_reference.FullName, StringComparison.OrdinalIgnoreCase))
      {
        RemoteCheckLabel.Text = "Failed to download assembly.";
        BrowseButton.IsEnabled = true;
        DownloadButton.IsEnabled = true;
        return;
      }

      string fileName = Path.Combine(definition.Name.Name, ".dll");

      // TODO: take into account partial trust in-browser mode
      
      if (Definition.IsSilverlight())
        StorageService.CacheSilverlightAssembly(fileName, data);
      else
        StorageService.CacheNetAssembly(fileName, data);
      
      ApplicationModel.Current.AssemblyCache.AddAssembly(definition);

      Definition = definition;
      DialogResult = true;
    }
  }
}