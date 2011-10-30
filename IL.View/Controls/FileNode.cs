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
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using IL.View.Controls.CodeView;
using IL.View.Model;
using IL.View.Services;
using Mono.Cecil;

namespace IL.View.Controls
{
  public sealed class FileNode : TreeNode
  {
    [Import]
    public IContentViewerService ContentViewerService { get; set; }

    private readonly AssemblyStream _source;
    private readonly string _resourcePath;
    private readonly string _extension;
    private readonly string _fileName;

    public override AssemblyDefinition DeclaringAssembly
    {
      get { return null; }
    }

    public FileNode(AssemblyStream source, string resourcePath)
      : base(null)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (string.IsNullOrWhiteSpace(resourcePath)) throw new ArgumentNullException("resourcePath");

      _source = source;
      _resourcePath = resourcePath;
      _fileName = Path.GetFileName(_resourcePath);
      
      _extension = Path.GetExtension(_fileName);
      if (!string.IsNullOrEmpty(_extension)) _extension = _extension.ToLowerInvariant();

      Header = CreateHeaderCore(DefaultImages.AssemblyBrowser.GetFileIcon(_fileName), null, resourcePath, true);
      DefaultStyleKey = typeof(FileNode);
      CompositionInitializer.SatisfyImports(this);
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
        case ".jpg":
          return OpenAsImage();
        default:
          return false;
      }
    }

    private bool OpenAsXml()
    {
      var content = new StreamReader(_source.OpenRead()).ReadToEnd();
      ContentViewerService.ShowSourceCode(_fileName, SourceLanguageType.Xaml, content);
      return true;
    }

    private bool OpenAsImage()
    {
      var bitmap = new BitmapImage();
      bitmap.SetSource(_source.OpenRead());
      ContentViewerService.ShowImage(_fileName, bitmap);
      return true;
    }
  }
}
