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
using System.Windows.Resources;
using IL.View.Model;
using Mono.Cecil;
using System.IO;
using System;
using System.Linq;

namespace IL.View.Controls
{
  public sealed class XapPackageNode : TreeNode<FileInfo>
  {
    public override AssemblyDefinition DeclaringAssembly
    {
      get { return null; }
    }

    public XapPackageNode(FileInfo component)
      : base(component)
    {
      DefaultStyleKey = typeof(XapPackageNode);
      Header = CreateHeaderCore(DefaultImages.AssemblyBrowser.XapPackage, null, component.Name, true);
      DataProvider = DoLoadType;
    }

    private static void DoLoadType(TreeNode<FileInfo> view, FileInfo definition)
    {
      var files = ZipUtil.GetZipContents(definition.OpenRead())
        .OrderByDescending(p => p.Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar));

      foreach (var file in files)
      {
        var ext = Path.GetExtension(file);
        if (!string.IsNullOrWhiteSpace(ext)) ext = ext.ToLowerInvariant();

        //var icon = DefaultImages.AssemblyBrowser.FileMisc;
        //if (ext == ".xml") icon = DefaultImages.AssemblyBrowser.FileXml;
        //if (ext == ".xaml") icon = DefaultImages.AssemblyBrowser.FileXml;
        //if (ext == ".clientconfig") icon = DefaultImages.AssemblyBrowser.FileXml;
        //if (ext == ".dll") icon = DefaultImages.AssemblyBrowser.Assembly;
        
        var subfolders = file.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        if (subfolders.Length > 1)
        {
          TreeNode rootNode = null;
          for (var i = 0; i < subfolders.Length - 1; i++)
          {
            var node = new SimpleNode(DefaultImages.AssemblyBrowser.FolderClosed, subfolders[i]);
            if (rootNode != null) rootNode.Items.Add(node);
            rootNode = node;
          }

          //var fileNode = new SimpleNode(icon, subfolders[subfolders.Length - 1]);
          var fileNode = GenerateNode(definition, subfolders[subfolders.Length - 1]);
          if (rootNode != null)
          {
            rootNode.Items.Add(fileNode);
            view.Items.Add(rootNode);
          }
          else
          {
            view.Items.Add(fileNode);
          }
        }
        else
        {
          //var node = new SimpleNode(icon, file);
          var node = GenerateNode(definition, file);
          view.Items.Add(node); 
        }
      }
    }

    private static TreeNode GenerateNode(FileInfo package, string uri)
    {
      var ext = Path.GetExtension(uri);

      if (ext == ".dll")
      {
        var zipInfo = new StreamResourceInfo(package.OpenRead(), null);
        var streamInfo = Application.GetResourceStream(zipInfo, new Uri(uri, UriKind.Relative));
        var fileStream = streamInfo.Stream;
        var definition = AssemblyDefinition.ReadAssembly(fileStream);
        return new AssemblyNode(definition);
      }

      var icon = DefaultImages.AssemblyBrowser.FileMisc;
      if (ext == ".xml") icon = DefaultImages.AssemblyBrowser.FileXml;
      if (ext == ".xaml") icon = DefaultImages.AssemblyBrowser.FileXml;
      if (ext == ".clientconfig") icon = DefaultImages.AssemblyBrowser.FileXml;
      if (ext == ".dll") icon = DefaultImages.AssemblyBrowser.Assembly;

      return new SimpleNode(icon, uri);
    }
  }
}
