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
using System.IO;
using System.Text;

namespace IL.View
{
  // Based on: http://blogs.msdn.com/b/blemmon/archive/2009/11/25/reading-zip-files-from-silverlight.aspx
  public static class ZipUtil
  {
    /// <summary>
    /// Reads the file names from the header of the zip file
    /// </summary>
    /// <param name="zipStream">The stream to the zip file</param>
    /// <returns>An array of file names stored within the zip file. These file names may also include relative paths.</returns>
    public static string[] GetZipContents(Stream zipStream)
    {
      var names = new List<string>();
      var reader = new BinaryReader(zipStream);
      while (reader.ReadUInt32() == 0x04034b50)
      {
        // Skip the portions of the header we don't care about
        reader.BaseStream.Seek(14, SeekOrigin.Current);
        uint compressedSize = reader.ReadUInt32();
        uint uncompressedSize = reader.ReadUInt32();
        int nameLength = reader.ReadUInt16();
        int extraLength = reader.ReadUInt16();
        byte[] nameBytes = reader.ReadBytes(nameLength);
        names.Add(Encoding.UTF8.GetString(nameBytes, 0, nameLength));
        reader.BaseStream.Seek(extraLength + compressedSize, SeekOrigin.Current);

      }
      // Move the stream back to the begining
      zipStream.Seek(0, SeekOrigin.Begin);
      return names.ToArray();
    }
  }
}