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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Linq;

namespace IL.View.Model
{
  [DebuggerDisplay("{Path}")]
  public sealed class ReferenceFolder : INotifyPropertyChanged
  {
    private readonly ReferencePathsSettings _settings;
    private bool _recursiveSearch;

    public string Path { get; set; }

    public bool RecursiveSearch
    {
      get { return _recursiveSearch; }
      set
      {
        if (_recursiveSearch == value) return;
        _recursiveSearch = value;
        OnPropertyChanged("RecursiveSearch");
        if (_settings.AutoSave) _settings.Save();
      }
    }
    
    internal ReferenceFolder(ReferencePathsSettings settings, string path, bool recursiveSearch = false)
    {
      if (settings == null) throw new ArgumentNullException("settings");
      if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");

      _settings = settings;
      _recursiveSearch = recursiveSearch;
      Path = path;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  
  public sealed class ReferencePathsSettings : INotifyPropertyChanged
  {
    public static ReferencePathsSettings Current
    {
      get { return new ReferencePathsSettings(); }
    }

    public bool AutoSave { get; set; }
    
    private ObservableCollection<ReferenceFolder> _folders = new ObservableCollection<ReferenceFolder>();

    public ObservableCollection<ReferenceFolder> Folders
    {
      get { return _folders; }
      private set
      {
        if (_folders == value) return;
        if (_folders != null) _folders.CollectionChanged -= OnFoldersChanged;
        _folders = value;
        if (_folders != null) _folders.CollectionChanged += OnFoldersChanged;
      }
    }

    public ReferencePathsSettings()
      : this(true)
    {
    }

    public ReferencePathsSettings(bool autosave)
    {
      AutoSave = autosave;
      Load();
    }

    public bool AddFolder(string path)
    {
      if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");

      if (!Folders.Any(f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
      {
        Folders.Add(new ReferenceFolder(this, path));
        return true;
      }

      return false;
    }

    private void OnFoldersChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (AutoSave) Save();
    }

    public void Save()
    {
      var data = new XElement("ReferencePaths");

      foreach (var folder in Folders)
        data.Add(new XElement("ReferenceFolder",
            new XAttribute("Path", folder.Path),
            new XAttribute("RecursiveSearch", folder.RecursiveSearch)));

      IsolatedStorageSettings.ApplicationSettings["ReferencePaths"] = data.ToString();
      IsolatedStorageSettings.ApplicationSettings.Save();
    }

    public void Load()
    {
      Folders = new ObservableCollection<ReferenceFolder>(LoadSettings());
      OnPropertyChanged("Folders");
    }

    private IEnumerable<ReferenceFolder> LoadSettings()
    {
      string settings;

      if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("ReferencePaths", out settings))
        yield break;

      var data = XElement.Parse(settings);
      if (data.HasElements)
      {
        foreach (var element in data.Elements())
        {
          var path = (string)element.Attribute("Path");
          var recursiveSearch = (bool)element.Attribute("RecursiveSearch");
          yield return new ReferenceFolder(this, path, recursiveSearch);
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
