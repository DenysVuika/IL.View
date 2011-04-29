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
using System.Text;
using System.Windows;
using Mono.Cecil;

namespace IL.View.Repositories
{
  public class RepositoryClient : DependencyObject
  {
    public static string GetRepositoryAddress(AssemblyDefinition definition)
    {
      if (definition == null) throw new ArgumentNullException("definition");

      var builder = new StringBuilder();
      builder.AppendFormat("/{0}/", definition.MainModule.Architecture.ToString());
      if (definition.IsSilverlight())
        builder.Append("Silverlight");
      else
        builder.Append(definition.MainModule.Runtime.ToString());
      builder.AppendFormat("/assembly?name={0}", definition.Name.Name);
      builder.AppendFormat("&version={0}", definition.Name.Version.ToString());
      builder.AppendFormat("&token={0}", definition.Name.GetPublicTokenKeyString());
      builder.Append("&specificversion=false");

      string result = builder.ToString();
      return result;
    }
  }
}
