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
using System.Windows;
using System.Windows.Resources;
using System.Linq;
using System.Resources;

namespace IL.View.Model
{
  public abstract class AssemblyStream
  {
    public abstract string Name { get; }
    public abstract Stream OpenRead();
  }

  public sealed class AssemblyFileStream : AssemblyStream
  {
    private readonly FileInfo _fileInfo;

    public override string Name
    {
      get { return _fileInfo.Name; }
    }

    public string FullName
    {
      get { return _fileInfo.FullName; }
    }
    
    public AssemblyFileStream(FileInfo fileInfo)
    {
      _fileInfo = fileInfo;
    }

    public override Stream OpenRead()
    {
      return _fileInfo.OpenRead();
    }
  }

  public sealed class AssemblyMemoryStream : AssemblyStream
  {
    private readonly byte[] _buffer;
    private readonly string _name;

    public override string Name
    {
      get { return _name; }
    }

    public AssemblyMemoryStream(string name, Stream stream)
    {
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
      if (stream == null) throw new ArgumentNullException("stream");
      
      _name = name;

      using (var ms = new MemoryStream())
      {
        stream.CopyTo(ms);
        _buffer = ms.ToArray();
      }
    }

    public override Stream OpenRead()
    {
      return new MemoryStream(_buffer, false);
    }
  }

  // TODO: Provide support for in-memory-only scenario when package is stored within buffer!
  public sealed class AssemblyPackageStream : AssemblyStream
  {
    private readonly StreamResourceInfo _package;
    private readonly string _name;

    public override string Name
    {
      get { return _name; }
    }
    
    public IEnumerable<AssemblyStream> EnumerateEntries()
    {
      var entries = ZipUtil
        .GetZipContents(OpenRead())
        .OrderByDescending(p => p.Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar));

      return entries
        .Select(entry => new AssemblyPackageEntryStream(_package, entry))
        .Cast<AssemblyStream>();
    }

    public AssemblyPackageStream(string name, StreamResourceInfo package)
    {
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
      if (package == null) throw new ArgumentNullException("package");

      _name = name;
      _package = package;
    }

    public AssemblyPackageStream(string name, Stream stream)
      : this(name, new StreamResourceInfo(stream, null))
    {
    }

    public AssemblyPackageStream(FileInfo fileInfo)
      : this(fileInfo.Name, fileInfo.OpenRead())
    {
    }

    public override Stream OpenRead()
    {
      return _package.Stream;
    }
  }

  public sealed class AssemblyPackageEntryStream : AssemblyStream
  {
    private readonly StreamResourceInfo _package;
    private readonly string _name;

    public override string Name
    {
      get { return _name; }
    }

    internal AssemblyPackageEntryStream(StreamResourceInfo package, string name)
    {
      _package = package;
      _name = name;
    }

    public override Stream OpenRead()
    {
      var streamInfo = Application.GetResourceStream(_package, new Uri(_name, UriKind.Relative));
      return streamInfo.Stream;
    }
  }

  public sealed class EmbeddedResourceStream : AssemblyStream
  {
    private readonly ResourceSet _source;
    private readonly string _name;
    
    public override string Name
    {
      get { return _name; }
    }

    public EmbeddedResourceStream(ResourceSet source, string name)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

      _source = source;
      _name = name;
    }

    public override Stream OpenRead()
    {
      return _source.GetObject(_name, true) as Stream;
    }
  }
}
