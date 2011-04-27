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

namespace Mono.Cecil
{
  public static class CodeUriServices
  {
    public static Uri GetCodeUri(this MethodDefinition definition)
    {
      string method = definition.GetCodeUriPart();
      string declaringType = definition.DeclaringType.GetCodeUriPart();
      string assembly = definition.DeclaringType.Module.Assembly.GetCodeUriPart();

      string uri = string.Format("code://{0}/{1}/{2}", assembly, declaringType, method);
      return new Uri(uri, UriKind.Absolute);
    }

    public static string GetCodeUriPart(this TypeDefinition definition)
    {
      var builder = new StringBuilder();
      builder.AppendFormat("{0}.", definition.Namespace);

      if (definition.HasGenericParameters)
      {
        builder.Append(definition.Name.Substring(0, definition.Name.IndexOf('`')));

        builder.Append("<");
        for (int i = 0; i < definition.GenericParameters.Count - 1; i++)
          builder.Append(",");
        builder.Append(">");
      }
      else
      {
        builder.Append(definition.Name);
      }

      return builder.ToString();
    }

    public static string GetCodeUriPart(this MethodDefinition definition)
    {
      var builder = new StringBuilder();

      builder.Append(definition.Name);

      if (definition.HasGenericParameters)
      {
        builder.Append("<");
        for (int i = 0; i < definition.GenericParameters.Count - 1; i++)
          builder.Append(',');
        builder.Append(">");
      }

      builder.Append("(");
      if (definition.HasParameters)
      {
        for (int i = 0; i < definition.Parameters.Count; i++)
        {
          var parameter = definition.Parameters[i];

          if (parameter.ParameterType.IsGenericParameter)
            builder.AppendFormat("<!!{0}>", parameter.Index);
          else
            builder.Append(parameter.ParameterType.Name);

          if (i != definition.Parameters.Count - 1) builder.Append(",");
        }
      }
      builder.Append(")");

      return builder.ToString();
    }

    public static string GetCodeUriPart(this AssemblyDefinition definition)
    {
      var builder = new StringBuilder();

      builder.Append(definition.Name.Name);
      builder.AppendFormat(":{0}", definition.Name.Version.ToString());
      if (definition.Name.PublicKeyToken != null && definition.Name.PublicKeyToken.Length > 0)
      {
        builder.Append(":");
        foreach (var b in definition.Name.PublicKeyToken)
        {
          builder.Append(Utils.PKeyTokenHex[b / 16 & Utils.PKeyTokenMask]);
          builder.Append(Utils.PKeyTokenHex[b & Utils.PKeyTokenMask]);
        }
      }
      return builder.ToString();
    }
  }
}
