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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Resources;
using Mono.Cecil;
using System.IO;
using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Controls;

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

    private static TreeNode GetOrCreateFolder(TreeNode root, IEnumerable<string> path)
    {
      var result = root;

      var subfolders = root.Items.OfType<FolderNode>().ToList();

      foreach (var folderName in path)
      {
        var existing = subfolders.FirstOrDefault(f => f.FolderName.Equals(folderName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
          result = existing;
          subfolders = existing.Items.OfType<FolderNode>().ToList();
          continue;
        }

        existing = new FolderNode(folderName);
        subfolders.Clear();

        if (result != null)
          result.Items.Add(existing);
        
        result = existing;
      }

      return result ?? root;
    }

    private static void DoLoadType(TreeNode<FileInfo> view, FileInfo definition)
    {
      var files = ZipUtil.GetZipContents(definition.OpenRead())
        .OrderByDescending(p => p.Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar));

      var package = new StreamResourceInfo(definition.OpenRead(), null);

      foreach (var fileUri in files)
      {
        var subfolders = fileUri.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
        
        if (subfolders.Length > 1)
        {
          var folderNode = GetOrCreateFolder(view, subfolders.Take(subfolders.Length - 1));
          var fileName = subfolders.Last();
          if (string.IsNullOrEmpty(fileName)) continue;
          var fileNode = GenerateNode(package, fileName, fileUri);
          folderNode.Items.Add(fileNode);
        }
        else
        {
          var node = GenerateNode(package, Path.GetFileName(fileUri), fileUri);
          view.Items.Add(node); 
        }
      }
    }

    private static TreeNode GenerateNode(StreamResourceInfo package, string header, string uri)
    {
      var ext = Path.GetExtension(header);
      if (!string.IsNullOrWhiteSpace(ext)) ext = ext.ToLowerInvariant();

      if (ext == ".dll")
      {
        var streamInfo = Application.GetResourceStream(package, new Uri(uri, UriKind.Relative));
        if (streamInfo == null)
        {
          Debug.WriteLine("Error loading assembly '{0}'", header);
          var node = new SimpleNode(DefaultImages.AssemblyBrowser.BugError, header);
          ToolTipService.SetToolTip(node, string.Format("Error loading assembly '{0}'.", header));
          return node;
        }
        var fileStream = streamInfo.Stream;
        var definition = AssemblyDefinition.ReadAssembly(fileStream);
        return new AssemblyNode(definition);
      }

      return new XapEntryNode(package, header, uri);
    }
  }
}
