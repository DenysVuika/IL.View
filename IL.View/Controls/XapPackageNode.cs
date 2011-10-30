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
using System.IO;
using System.Linq;
using IL.View.Model;
using Mono.Cecil;

namespace IL.View.Controls
{
  public sealed class XapPackageNode : TreeNode<AssemblyPackageStream>
  {
    public override AssemblyDefinition DeclaringAssembly
    {
      get { return null; }
    }

    public XapPackageNode(AssemblyPackageStream component)
      : base(component)
    {
      DefaultStyleKey = typeof(XapPackageNode);
      Header = CreateHeaderCore(DefaultImages.AssemblyBrowser.XapPackage, null, component.Name, true);
      DataProvider = LoadSubItems;
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

    private static void LoadSubItems(TreeNode<AssemblyPackageStream> view, AssemblyPackageStream packageStream)
    {
      foreach (var packageEntry in packageStream.EnumerateEntries())
      {
        var fileUri = packageEntry.Name;
        var subfolders = fileUri.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
        
        if (subfolders.Length > 1)
        {
          var folderNode = GetOrCreateFolder(view, subfolders.Take(subfolders.Length - 1));
          var fileName = subfolders.Last();
          if (string.IsNullOrEmpty(fileName)) continue;
          var fileNode = GenerateNode(packageEntry, fileName);
          folderNode.Items.Add(fileNode);
        }
        else
        {
          var node = GenerateNode(packageEntry, Path.GetFileName(fileUri));
          view.Items.Add(node); 
        }
      }
    }

    private static TreeNode GenerateNode(AssemblyStream entry, string header)
    {
      var ext = Path.GetExtension(header);
      if (!string.IsNullOrWhiteSpace(ext)) ext = ext.ToLowerInvariant();

      if (ext == ".dll")
      {
        var definition = AssemblyDefinition.ReadAssembly(entry.OpenRead());
        return new AssemblyNode(definition, entry);
      }

      return new XapEntryNode(entry, header);
    }
  }
}
