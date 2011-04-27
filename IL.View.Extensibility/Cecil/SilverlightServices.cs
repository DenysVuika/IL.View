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
namespace Mono.Cecil
{
  public static class SilverlightServices
  {
    public static bool IsSilverlight(this AssemblyDefinition definition)
    {
      if (definition == null) return false;

      // check whether "mscorlib" was provided
      if (definition.Name.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
        return HasSilverlightToken(definition.Name);

      // check the "mscorlib" version            
      var mscorlib = definition.MainModule.TypeSystem.Corlib as AssemblyNameReference;
      return mscorlib != null && mscorlib.HasSilverlightToken();
    }

    public static bool HasSilverlightToken(this AssemblyNameReference assemblyName)
    {
      if (assemblyName == null || assemblyName.PublicKeyToken == null || assemblyName.PublicKeyToken.Length != Utils.SLPKeyToken.Length) return false;

      bool result = true;

      for (int i = 0; i < Utils.SLPKeyToken.Length; i++)
      {
        result = (Utils.SLPKeyToken[i] == assemblyName.PublicKeyToken[i]);
        if (!result) return result;
      }

      return result;
    }
  }
}
