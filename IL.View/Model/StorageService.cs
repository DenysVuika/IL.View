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
using System.IO;
using System.Collections.Generic;

namespace IL.View.Model
{
  // File system access facade.
  public class StorageService
  {
    private static string GetApplicationFolder()
    {
      var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      var appForder = Path.Combine(documents, "IL.View");

      if (!Directory.Exists(appForder))
        Directory.CreateDirectory(appForder);

      return appForder;
    }

    private static string GetSilverilghtCacheFolder()
    {
      var folder = Path.Combine(GetAssemblyCacheFolder(), "SILVERLIGHT");
      if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
      return folder;
    }

    private static string GetNetCacheFolder()
    {
      var folder = Path.Combine(GetAssemblyCacheFolder(), "NET");
      if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
      return folder;
    }

    private static string GetAssemblyCacheFolder()
    {
      var assemblyCacheFolder = Path.Combine(GetApplicationFolder(), "AssemblyCache");
      if (!Directory.Exists(assemblyCacheFolder))
        Directory.CreateDirectory(assemblyCacheFolder);

      return assemblyCacheFolder;
    }

    private static void CacheAssembly(string path, Stream data)
    {
      var buffer = new byte[data.Length];
      data.Read(buffer, 0, buffer.Length);
      File.WriteAllBytes(path, buffer);
    }

    public static void CacheSilverlightAssembly(string name, Stream data)
    {
      CacheAssembly(Path.Combine(GetSilverilghtCacheFolder(), name), data);
    }

    public static void CacheNetAssembly(string name, Stream data)
    {
      CacheAssembly(Path.Combine(GetNetCacheFolder(), name), data);
    }

    public static IEnumerable<string> EnumerateAssemblyCache()
    {
      foreach (var path in Directory.EnumerateFiles(GetAssemblyCacheFolder(), "*.dll", SearchOption.AllDirectories))
        yield return path;
    }

    public static Stream OpenCachedAssembly(string path)
    {
      return File.Exists(path) ? File.OpenRead(path) : null;
    }
  }
}
