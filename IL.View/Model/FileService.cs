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

using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.Automation;
using System.Windows;
using System.Windows.Threading;
using Mono.Cecil;

namespace IL.View.Model
{
  internal static class FileService
  {
    private const string ReferenceCachePrefix = "REF_";

    // TODO: add possibility to clear cache from UI
    public static AssemblyDefinition FindExternalAssembly(AssemblyNameReference reference, Dispatcher dispatcher)
    {
      // TODO: add support User folders
      /*
       * User folders can be accessed without Automation Factory so it should be possible to 
       * define a reference path based on User folders hierarchy. In this case this feature will work
       * for MacOs platforms.
       * 
       * Possible ways to implement:
       * 
       *  1. Allow "Reference Paths" tab to be visible when running on Macs
       *  2. When adding reference path a check should be performed (whether path belongs to User folder hierarchy)
       *  3. The paths that do not belong to User folder hierachy should not be added (Macs only, provide some warning/notification)
       *  
       * Reference caching should also work fine for User folders.
      */
      if (!Application.Current.HasElevatedPermissions) return null;
      if (!AutomationFactory.IsAvailable) return null;
      if (reference == null) return null;
      
      var referencePaths = ReferencePathsSettings.Current.Folders.ToArray();
      var targetName = string.Format("{0}.dll", reference.Name);

      try
      {
        var savedPath = GetCachedPath(reference);
        // TODO: optimize code and remove duplication
        if (!string.IsNullOrEmpty(savedPath))
        {
          using (var stream = LoadFile(savedPath))
          {
            var definition = AssemblyDefinition.ReadAssembly(stream);

            if (definition != null && definition.FullName == reference.FullName)
            {
              Debug.WriteLine("Successfully resolved external assembly from cache: {0}", savedPath);
              var assemblyStream = new AssemblyMemoryStream(targetName, stream);
              dispatcher.BeginInvoke(() => ApplicationModel.Current.AssemblyCache.LoadAssembly(assemblyStream, definition));
              return definition; 
            }
          }
        }

        var fso = AutomationFactory.CreateObject("Scripting.FileSystemObject");
        
        foreach (var referenceFolder in referencePaths)
        {
          var file = FindFile(fso, referenceFolder.Path, targetName, referenceFolder.RecursiveSearch);
          if (file == null) continue;

          string path = file.Path;
          using (var stream = LoadFile(path))
          {
            var definition = AssemblyDefinition.ReadAssembly(stream);
            
            if (definition == null) continue;
            if (definition.FullName != reference.FullName) continue;

            Debug.WriteLine("Successfully resolved external assembly: {0}", path);

            AddCachedPath(reference, path);

            var assemblyStream = new AssemblyMemoryStream(targetName, stream);
            dispatcher.BeginInvoke(() => ApplicationModel.Current.AssemblyCache.LoadAssembly(assemblyStream, definition));
            return definition;
          }
        }
      }
      catch
      {
        if (Debugger.IsAttached) Debugger.Break();
        return null;
      }

      return null;
    }

    private static dynamic FindFile(dynamic fso, string rootPath, string fileName, bool recursive)
    {
      if (!fso.FolderExists(rootPath)) return null;
      var folder = fso.GetFolder(rootPath);

      var targetPath = Path.Combine(rootPath, fileName);
      if (fso.FileExists(targetPath)) return fso.GetFile(targetPath);

      if (!recursive) return null;

      foreach (var subFolder in folder.SubFolders)
      {
        var assembly = FindFile(fso, subFolder.Path, fileName, true);
        if (assembly != null) return assembly;
      }

      return null;
    }

    private static string GetCachedPath(AssemblyNameReference reference)
    {
      if (reference == null) return null;
      var cachedName = ReferenceCachePrefix + reference.FullName;
      string cachedPath;
      return IsolatedStorageSettings.ApplicationSettings.TryGetValue(cachedName, out cachedPath) ? cachedPath : null;
    }

    private static void AddCachedPath(AssemblyNameReference reference, string path)
    {
      if (reference == null) return;
      if (string.IsNullOrWhiteSpace(path)) return;
      IsolatedStorageSettings.ApplicationSettings[ReferenceCachePrefix + reference.FullName] = path;
    }
    
    // http://stackoverflow.com/questions/3462039/scripting-filesystemobject-write-method-fails
    private static Stream LoadFile(string path)
    {
      byte[] data;
      const int adTypeBinary = 1;

      using (var adoCom = AutomationFactory.CreateObject("ADODB.Stream"))
      {
        adoCom.Type = adTypeBinary;
        adoCom.Open();
        adoCom.LoadFromFile(path);
        data = adoCom.Read();
      }

      return data == null ? Stream.Null : new MemoryStream(data);
    }
  }
}
