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
  public sealed class ReferenceFolder
  {
    public string Path { get; set; }

    public ReferenceFolder(string path)
    {
      if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");
      Path = path;
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
        Folders.Add(new ReferenceFolder(path));
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

      foreach (var address in Folders.Select(d => d.Path).Distinct())
        data.Add(new XElement("ReferenceFolder",
            new XAttribute("Path", address)));

      IsolatedStorageSettings.ApplicationSettings["ReferencePaths"] = data.ToString();
      IsolatedStorageSettings.ApplicationSettings.Save();
    }

    public void Load()
    {
      Folders = new ObservableCollection<ReferenceFolder>(LoadSettings());
      OnPropertyChanged("Folders");
    }

    private static IEnumerable<ReferenceFolder> LoadSettings()
    {
      string settings;

      if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("ReferencePaths", out settings))
        yield break;

      var data = XElement.Parse(settings);
      if (data.HasElements)
      {
        foreach (var element in data.Elements())
        {
          var path = element.Attribute("Path");
          if (path != null)
            yield return new ReferenceFolder(path.Value);
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
