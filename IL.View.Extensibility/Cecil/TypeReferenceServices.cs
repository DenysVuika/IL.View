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

using System.Text;

namespace Mono.Cecil
{
  public static class TypeReferenceServices
  {
    // TODO: must be optimized (remove code duplication)
    public static string ToShortTypeName(this TypeReference reference)
    {
      if (reference == null) return null;

      var builder = new StringBuilder();

      var genericInstance = reference as GenericInstanceType;
      if (genericInstance != null && genericInstance.HasGenericArguments)
      {
        builder.Append(genericInstance.Name.Substring(0, genericInstance.Name.IndexOf('`')));
        builder.Append("<");

        for (int i = 0; i < genericInstance.GenericArguments.Count; i++)
        {
          if (i > 0) builder.Append(", ");

          var argument = genericInstance.GenericArguments[i];
          if (argument.IsGenericInstance)
            builder.Append(argument.ToShortTypeName());
          else
            builder.Append(argument.Name);          
        }

        builder.Append(">");
      }
      else if (reference.HasGenericParameters)
      {
        builder.Append(reference.Name.Substring(0, reference.Name.IndexOf('`')));
        builder.Append("<");

        for (int i = 0; i < reference.GenericParameters.Count; i++)
        {
          if (i > 0) builder.Append(", ");

          var parameter = reference.GenericParameters[i];
          if (parameter.IsGenericInstance)
            builder.Append(parameter.ToShortTypeName());
          else
            builder.Append(parameter.Name);
        }

        builder.Append(">");
      }
      else
      {
        var primitiveName = GetPrimitiveTypeName(reference);
        builder.Append(string.IsNullOrEmpty(primitiveName) ? reference.Name : primitiveName);
      }

      return builder.ToString();
    }

    public static string GetPrimitiveTypeName(this TypeReference reference)
    {     
      
      switch (reference.FullName)
      {
        case "System.SByte":
          return "int8";
        case "System.Int16":
          return "int16";
        case "System.Int32":
          return "int32";
        case "System.Int64":
          return "int64";
        case "System.Byte":
          return "uint8";
        case "System.UInt16":
          return "uint16";
        case "System.UInt32":
          return "uint32";
        case "System.UInt64":
          return "uint64";
        case "System.Single":
          return "float32";
        case "System.Double":
          return "float64";
        case "System.Void":
          return "void";
        case "System.Boolean":
          return "bool";
        case "System.String":
          return "string";
        case "System.Char":
          return "char";
        case "System.Object":
          return "object";
        default:
          return null;
      }
    }
  }
}
