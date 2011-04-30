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
  [DebuggerDisplay("{Address}")]
  public class RepositoryDefinition
  {
    public string Address { get; set; }

    public RepositoryDefinition() { }

    public RepositoryDefinition(string address)
    {
      if (string.IsNullOrEmpty(address)) throw new ArgumentNullException("address");
      Address = address;
    }
  }

  public class RepositorySettings : INotifyPropertyChanged
  {
    public static RepositorySettings Current
    {
      get { return new RepositorySettings(); }
    }

    public bool AutoSave { get; set; }

    private ObservableCollection<RepositoryDefinition> _definitions = new ObservableCollection<RepositoryDefinition>();

    public ObservableCollection<RepositoryDefinition> Definitions
    {
      get { return _definitions; }
      private set
      {
        if (_definitions == value) return;
        if (_definitions != null) _definitions.CollectionChanged -= OnDefinitionsChanged;
        _definitions = value;
        if (_definitions != null) _definitions.CollectionChanged += OnDefinitionsChanged;
      }
    }

    public RepositorySettings()
      : this(true)
    {
    }

    public RepositorySettings(bool autosave)
    {
      AutoSave = autosave;
      Load();
    }

    public bool AddRepository(string address)
    {
      if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException("address");

      if (!Definitions.Any(d => d.Address.Equals(address, StringComparison.OrdinalIgnoreCase)))
      {
        Definitions.Add(new RepositoryDefinition(address));
        return true;
      }

      return false;
    }

    private void OnDefinitionsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (AutoSave) Save();
    }

    public void Save()
    {
      var data = new XElement("Repositories");

      foreach (var address in Definitions.Select(d => d.Address).Distinct())
        data.Add(new XElement("Repository",
            new XAttribute("Address", address)));

      IsolatedStorageSettings.ApplicationSettings["RepositorySettings"] = data.ToString();
      IsolatedStorageSettings.ApplicationSettings.Save();
    }

    public void Load()
    {
      Definitions = new ObservableCollection<RepositoryDefinition>(LoadSettings());
      OnPropertyChanged("Definitions");
    }

    private IEnumerable<RepositoryDefinition> LoadSettings()
    {
      string settings = null;

      if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("RepositorySettings", out settings))
        yield break;

      var data = XElement.Parse(settings);
      if (data.HasElements)
      {
        foreach (var element in data.Elements())
        {
          var address = element.Attribute("Address");
          if (address != null)
            yield return new RepositoryDefinition(address.Value);
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
