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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Resources;
using IL.View.Controls.CodeView;
using IL.View.Services;
using Mono.Cecil;
using System.Windows;
using System.Windows.Media.Imaging;

namespace IL.View.Controls
{
  public sealed class XapEntryNode : TreeNode
  {
    [Import]
    public IContentViewerService ContentViewerService { get; set; }

    private readonly StreamResourceInfo _package;
    private readonly string _resourceName;
    private readonly string _resourceUri;
    private readonly string _extension;

    public override AssemblyDefinition DeclaringAssembly
    {
      get { return null; }
    }

    public XapEntryNode(StreamResourceInfo package, string resourceName, string resourceUri)
      : base(null)
    {
      DefaultStyleKey = typeof(XapEntryNode);

      _package = package;
      _resourceName = resourceName;
      _resourceUri = resourceUri;

      _extension = Path.GetExtension(resourceName);
      if (!string.IsNullOrEmpty(_extension)) _extension = _extension.ToLowerInvariant();

      Header = base.CreateHeaderCore(GetEntryIcon(_resourceName), null, resourceName, true);
      CompositionInitializer.SatisfyImports(this);
    }

    private Stream OpenRead()
    {
      var streamInfo = Application.GetResourceStream(_package, new Uri(_resourceUri, UriKind.Relative));
      if (streamInfo == null)
      {
        Debug.WriteLine("Error loading file '{0}'", _resourceUri);
        return null;
      }
      return streamInfo.Stream;
    }
    
    private static string GetEntryIcon(string resouceUri)
    {
      var ext = Path.GetExtension(resouceUri);
      if (!string.IsNullOrWhiteSpace(ext)) ext = ext.ToLowerInvariant();

      switch (ext)
      {
        case ".dll":
          return DefaultImages.AssemblyBrowser.Assembly;
        case ".xml":
        case ".xaml":
        case ".clientconfig":
          return DefaultImages.AssemblyBrowser.FileXml;
        case ".png":
          return DefaultImages.AssemblyBrowser.FileImage;
        default:
          return DefaultImages.AssemblyBrowser.FileMisc;
      }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (e.Key == Key.Space)
      {
        e.Handled = DisplayContent();
        return;
      }

      base.OnKeyDown(e);
    }

    private bool DisplayContent()
    {
      switch (_extension)
      {
        case ".xml":
        case ".xaml":
        case ".clientconfig":
          return OpenAsXml();
        case ".png":
          return OpenAsImage();
        default:
          return false;
      }
    }

    private bool OpenAsXml()
    {
      var content = new StreamReader(OpenRead()).ReadToEnd();
      ContentViewerService.ShowSourceCode(_resourceName, SourceLanguageType.Xaml, content);
      return true;
    }

    private bool OpenAsImage()
    {
      var bitmap = new BitmapImage();
      bitmap.SetSource(OpenRead());
      ContentViewerService.ShowImage(_resourceName, bitmap);
      return true;
    }
  }
}
