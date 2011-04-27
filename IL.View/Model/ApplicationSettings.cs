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

using System.ComponentModel;
using System.IO.IsolatedStorage;

namespace IL.View.Model
{ 
  public class AssemblyBrowserSettings : INotifyPropertyChanged
  {
    private static AssemblyBrowserSettings _current;
    public static AssemblyBrowserSettings Current
    {
      get { return _current ?? (_current = new AssemblyBrowserSettings()); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool AutoDisassemble
    {
      get
      {
        bool value;
        if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>("AutoDisassemble", out value)) return value;
        return false;
      }
      set
      {
        IsolatedStorageSettings.ApplicationSettings["AutoDisassemble"] = value;
        IsolatedStorageSettings.ApplicationSettings.Save();
        OnPropertyChanged("AutoDisassemble");
      }
    }
    
    private void OnPropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }  
}
