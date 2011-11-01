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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Navigation;
using IL.View.Model;
using System.Runtime.InteropServices.Automation;

namespace IL.View.Views
{
  public partial class Settings : Page
  {
    private readonly RepositorySettings _repositorySettings;
    private readonly ReferencePathsSettings _referencePathsSettings;

    public Settings()
    {
      InitializeComponent();
      _repositorySettings = Resources["RepositorySettings"] as RepositorySettings;
      _referencePathsSettings = Resources["ReferencePathsSettings"] as ReferencePathsSettings;
    }

    // Executes when the user navigates to this page.
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      // Display "Reference Paths" tab only for Windows full-trust installation.
      ReferencePathsTab.Visibility =
        Application.Current.HasElevatedPermissions && AutomationFactory.IsAvailable
          ? Visibility.Visible
          : Visibility.Collapsed;
    }

    private void OnAddRepositoryClick(object sender, RoutedEventArgs e)
    {
      string address = RepositoryAddress.Text;
      if (string.IsNullOrWhiteSpace(address)) return;

      if (_repositorySettings.AddRepository(address))
        RepositoryAddress.Text = string.Empty;
    }

    private void OnAddReferenceFolderClick(object sender, RoutedEventArgs e)
    {
      var path = ReferencePath.Text;
      if (string.IsNullOrWhiteSpace(path)) return;

      if (_referencePathsSettings.AddFolder(path))
        ReferencePath.Text = string.Empty;
    }

    private void OnRemoveEntryClick(object sender, RoutedEventArgs e)
    {
      var menuItem = sender as MenuItem;
      if (menuItem == null) return;

      var repositoryDefinition = menuItem.DataContext as RepositoryDefinition;
      if (repositoryDefinition != null) _repositorySettings.Definitions.Remove(repositoryDefinition);

      var referenceFolder = menuItem.DataContext as ReferenceFolder;
      if (referenceFolder != null) _referencePathsSettings.Folders.Remove(referenceFolder);
    }

    private void OnReferenceFolderSelected(object sender, SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count > 0)
      {
        RecursiveSearch.IsEnabled = true;
        RecursiveSearch.SetBinding(ToggleButton.IsCheckedProperty, new Binding("RecursiveSearch")
        {
          Mode = BindingMode.TwoWay,
          Source = e.AddedItems[0]
        });
      }
      else
      {
        RecursiveSearch.IsEnabled = false;
        RecursiveSearch.DataContext = null;
        RecursiveSearch.ClearValue(ToggleButton.IsCheckedProperty);
      }
    }

  }
}
