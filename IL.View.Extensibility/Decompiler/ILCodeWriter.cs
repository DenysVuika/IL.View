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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace IL.View.Decompiler
{
  public class ILCodeWriter
  {
    public void WriteMethod(MethodDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      output.WriteDefinition(".method ", definition);
      WriteMethodName(definition, output, options);

      OpenBlock(output);

      if (definition.HasCustomAttributes)
        WriteCustomAttributes(definition.CustomAttributes, output);

      if (definition.HasBody && options != null && options.FullDecompilation)
      {
        output.WriteLine(".maxstack {0}", definition.Body.MaxStackSize);
        WriteMethodLocals(definition, output);
        foreach (var instruction in definition.Body.Instructions)
          output.WriteLine(instruction.ToString());
      }

      CloseBlock(output);
    }

    private void WriteMethodName(MethodDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      if (definition.IsAssembly) output.Write("assembly ");
      if (definition.IsFamily) output.Write("family ");
      if (definition.IsPublic) output.Write("public ");
      if (definition.IsPrivate) output.Write("private ");
      if (definition.IsHideBySig) output.Write("hidebysig ");
      if (definition.IsSpecialName) output.Write("specialname ");
      if (definition.IsNewSlot) output.Write("newslot ");
      if (definition.IsVirtual) output.Write("virtual ");
      if (definition.IsFinal) output.Write("final ");
      if (definition.IsRuntimeSpecialName) output.Write("rtspecialname ");
      output.Write(definition.IsStatic ? "static " : "instance ");
      output.Write(definition.ReturnType.ToShortTypeName() + " ");

      // Name
      output.Write(GetMethodName(definition));
      WriteMethodParameters(definition, output, options);

      output.WriteSpace();
      if (definition.IsIL) output.Write("cil ");
      if (definition.IsRuntime) output.Write("runtime ");
      if (definition.IsManaged) output.Write("managed ");
    }

    public static string GetMethodName(MethodDefinition definition)
    {
      var result = new StringBuilder();
      result.Append(definition.Name);

      // Generic parameters
      if (definition.HasGenericParameters)
      {
        result.Append("<");
        for (int i = 0; i < definition.GenericParameters.Count; i++)
        {
          result.Append(definition.GenericParameters[i].Name);
          if (i != definition.GenericParameters.Count - 1)
            result.Append(", ");
        }
        result.Append(">");
      }

      return result.ToString();
    }

    private void WriteMethodParameters(MethodDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      output.Write("(");

      if (definition.HasParameters)
      {
        for (int i = 0; i < definition.Parameters.Count; i++)
        {
          if (i > 0) output.Write(", ");
          var parameter = definition.Parameters[i];
          output.Write("{0} {1}", parameter.ParameterType.ToShortTypeName(), parameter.Name);

        }
      }

      output.Write(")");
    }

    private void WriteMethodLocals(MethodDefinition definition, ISourceCodeOutput output)
    {
      if (definition == null || !definition.HasBody || !definition.Body.InitLocals) return;

      var body = definition.Body;

      output.WriteLine(".locals init (");
      output.Indent();

      for (int i = 0; i < body.Variables.Count; i++)
      {
        var variable = body.Variables[i];
        if (i > 0) output.WriteLine(",");
        output.Write("[{0}] {1} {2}", variable.Index, variable.VariableType.ToShortTypeName(), variable);
      }

      output.WriteLine(")");
      output.Unindent();
    }

    public void WriteProperty(PropertyDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      output.WriteDefinition(".property ", definition);
      output.Write(definition.HasThis ? "instance " : "class ");
      if (definition.PropertyType.IsValueType && !definition.PropertyType.IsPrimitive)
        output.Write("valuetype ");

      output.Write(definition.PropertyType.ToShortTypeName() + " ");
      output.Write(definition.Name);

      OpenBlock(output);

      if (definition.HasCustomAttributes)
        WriteCustomAttributes(definition.CustomAttributes, output);

      if (definition.HasGetter())
      {
        var getter = definition.GetMethod;
        var returnType = getter.MethodReturnType.ReturnType;
        output.Write(".get ");
        if (getter.HasThis) output.Write("instance ");
        if (returnType.IsValueType && !returnType.IsPrimitive) output.Write("valuetype ");
        // TODO: To be unwrapped
        output.WriteLine(getter.ToString());
      }

      if (definition.HasSetter())
      {
        var setter = definition.SetMethod;
        output.Write(".set ");
        if (setter.HasThis) output.Write("instance ");
        output.WriteLine(setter.ToString());
      }

      CloseBlock(output);
    }

    // TODO: look at ILSpy WriteEnum/WriteFlags helpers
    public void WriteField(FieldDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      output.WriteDefinition(".field ", definition);

      if (definition.IsAssembly) output.Write("assembly ");
      if (definition.IsFamily) output.Write("family ");
      if (definition.IsPrivate) output.Write("private ");
      if (definition.IsPublic) output.Write("public ");
      if (definition.IsStatic) output.Write("static ");
      if (!definition.FieldType.IsPrimitive && definition.FieldType.IsValueType) output.Write("valuetype ");
      if (definition.IsSpecialName) output.Write("specialname ");
      if (definition.IsRuntimeSpecialName) output.Write("rtspecialname ");
      if (definition.IsLiteral) output.Write("literal ");
      if (definition.IsInitOnly) output.Write("initonly ");
      output.Write(definition.FieldType.ToShortTypeName() + " ");
      output.Write(definition.Name + " ");

      if (definition.HasConstant)
      {
        string valueFormat = definition.FieldType == definition.Module.TypeSystem.String ? "'{0}'" : "{0}";
        output.Write(" = {0}({1})", definition.FieldType.FullName, string.Format(valueFormat, definition.Constant.ToString()));
      }

      if (definition.HasCustomAttributes)
      {
        OpenBlock(output);
        WriteCustomAttributes(definition.CustomAttributes, output);
        CloseBlock(output);
      }

      output.WriteLine();
    }

    public void WriteEvent(EventDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      output.WriteDefinition(".event ", definition);
      output.Write(definition.EventType.ToShortTypeName() + " ");
      output.Write(definition.Name + " ");

      OpenBlock(output);

      if (definition.HasCustomAttributes)
        WriteCustomAttributes(definition.CustomAttributes, output);

      if (definition.AddMethod != null)
      {
        var setter = definition.AddMethod;
        output.Write(".addon ");
        if (setter.HasThis) output.Write("instance ");

        output.Write(
          setter.ReturnType.IsPrimitive || setter.IsVoid()
          ? setter.ReturnType.Name.ToLowerInvariant()
          : setter.ReturnType.FullName);

        output.WriteSpace();
        //setter.ReturnType == setter.Module.TypeSystem.Void
        //setter.ReturnType.FullName + " " + (setter.DeclaringType.FullName + "::" + setter.Name) + MethodSignatureFullName(setter)
        output.WriteLine("{0}::{1}{2}", setter.DeclaringType.FullName, setter.Name, MethodSignatureFullName(setter));
      }

      if (definition.RemoveMethod != null)
      {
        var getter = definition.RemoveMethod;
        output.Write(".removeon ");
        if (getter.HasThis) output.Write("instance ");

        output.Write(
          getter.ReturnType.IsPrimitive || getter.IsVoid()
          ? getter.ReturnType.Name.ToLowerInvariant()
          : getter.ReturnType.FullName);

        output.WriteSpace();
        output.WriteLine("{0}::{1}{2}", getter.DeclaringType.FullName, getter.Name, MethodSignatureFullName(getter));
      }

      CloseBlock(output);
    }

    private static string MethodSignatureFullName(IMethodSignature method)
    {
      var builder = new StringBuilder();

      builder.Append("(");

      if (method.HasParameters)
      {
        var parameters = method.Parameters;
        for (int i = 0; i < parameters.Count; i++)
        {
          var parameter = parameters[i];
          if (i > 0) builder.Append(",");
          if (parameter.ParameterType.IsSentinel) builder.Append("...,");
          builder.Append(parameter.ParameterType.ToShortTypeName());
        }
      }

      builder.Append(")");

      return builder.ToString();
    }

    public void WriteType(TypeDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      output.WriteDefinition(".class ", definition);

      //if (definition.IsClass || definition.IsInterface) builder.AppendFormat("{0}.class ", indent);
      if (definition.IsPublic || definition.IsNestedPublic) output.Write("public ");
      if (definition.IsNotPublic || definition.IsNestedPrivate) output.Write("private ");
      if (definition.IsInterface) output.Write("interface ");
      if (definition.IsAbstract) output.Write("abstract ");
      if (definition.IsSequentialLayout) output.Write("sequential ");
      if (definition.IsAutoLayout) output.Write("auto ");
      if (definition.IsAnsiClass) output.Write("ansi ");
      if (definition.IsSealed) output.Write("sealed ");
      if (definition.IsNested) output.Write("nested ");
      if (definition.IsNestedFamily) output.Write("family ");
      if (definition.IsBeforeFieldInit) output.Write("beforefieldinit ");

      output.Write(definition.ToShortTypeName());

      if (definition.BaseType != null)
      {
        output.WriteLine();
        output.Indent();
        output.Write("extends {0}", definition.BaseType.ToShortTypeName());
        output.Unindent();
      }

      if (definition.HasInterfaces)
      {
        output.WriteLine();
        output.Indent();
        output.Write("implements ");

        for (int i = 0; i < definition.Interfaces.Count; i++)
        {
          if (i > 0) output.Write(", ");
          output.Write(definition.Interfaces[i].FullName);
        }
        output.Unindent();
      }

      OpenBlock(output);

      if (definition.HasCustomAttributes)
      {
        WriteCustomAttributes(definition.CustomAttributes, output);
        output.WriteLine();
      }

      if (options != null && options.FullDecompilation)
      {
        if (definition.HasNestedTypes)
        {
          foreach (var nestedType in definition.NestedTypes.OrderBy(t => t.Name))
          {
            WriteType(nestedType, output, options);
            output.WriteLine();
          }
        }

        if (definition.HasMethods)
        {
          foreach (var method in definition.Methods.OrderBy(m => m.Name))
          {
            if (method.IsGetter || method.IsSetter) continue;
            if (method.IsAddOn || method.IsRemoveOn) continue;
            WriteMethod(method, output, null);
            output.WriteLine();
          }
        }

        if (definition.HasProperties)
        {
          foreach (var property in definition.Properties.OrderBy(p => p.Name))
          {
            WriteProperty(property, output, null);
            output.WriteLine();
          }
        }

        if (definition.HasEvents)
        {
          foreach (var eventDefinition in definition.Events.OrderBy(e => e.Name))
          {
            WriteEvent(eventDefinition, output, null);
            output.WriteLine();
          }
        }

        if (definition.HasFields)
        {
          foreach (var field in definition.Fields.OrderBy(f => f.Name))
          {
            WriteField(field, output, null);
            output.WriteLine();
          }
        }
      }

      CloseBlock(output);
    }

    public void WriteNamespace(NamespaceDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      output.Write(".namespace {0}", definition.Name);

      OpenBlock(output);

      foreach (var type in definition.Types.OrderBy(t => t.Name))
      {
        WriteType(type, output, null);
        output.WriteLine();
      }

      CloseBlock(output);
    }

    public void WriteAssembly(AssemblyDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      var name = definition.Name;

      output.Write(".assembly " + name.Name);
      OpenBlock(output);

      // Write version
      if (name.Version != null)
        output.WriteLine(".ver {0}", name.Version.ToString().Replace('.', ':'));

      // Write hash algorithm
      if (name.HashAlgorithm != AssemblyHashAlgorithm.None)
      {
        output.Write(".hash algorithm {0}", string.Format("0x{0:X8}", (uint)definition.Name.HashAlgorithm));
        if (name.HashAlgorithm == AssemblyHashAlgorithm.SHA1)
        {
          output.Write(" // SHA1");
          output.WriteLine();
        }
      }

      // Write public key
      if (name.HasPublicKey)
        output.WriteLine(".publickey = ({0})", BitConverter.ToString(name.PublicKey).Replace("-", " "));

      // Try to write custom attributes
      if (definition.HasCustomAttributes)
        WriteCustomAttributes(definition.CustomAttributes, output);

      CloseBlock(output);
    }

    public void WriteModule(ModuleDefinition definition, ISourceCodeOutput output, DecompilerOptions options)
    {
      output.WriteLine(".module {0}", definition.Name);
      output.WriteLine("// MVID: {0}", definition.Mvid.ToString("B"));
      output.WriteLine("// Target Runtime Version: v{0}", definition.GetRuntimeVersion());
      output.WriteLine("// Architecture: {0}", definition.Architecture);
    }

    public void WriteCommentLine(ISourceCodeOutput output, string comment)
    {
      output.WriteLine("//" + comment);
    }

    /// <summary>
    /// Converts a type reference into a string. This method is used by the member tree node for parameter and return types.
    /// </summary>
    public string TypeToString(TypeReference type, bool includeNamespace, ICustomAttributeProvider typeAttributes = null)
    {
      if (includeNamespace)
        return type.FullName;
      else
        return type.Name;
    }


    private void OpenBlock(ISourceCodeOutput output)
    {
      output.WriteLine();
      output.WriteLine("{");
      output.Indent();
    }

    private void CloseBlock(ISourceCodeOutput output)
    {
      output.Unindent();
      output.WriteLine("}");
    }

    private void WriteCustomAttributes(IEnumerable<CustomAttribute> attributes, ISourceCodeOutput output)
    {
      foreach (var attribute in attributes)
      {
        var canProceed = attribute.TryCheckConstructorArguments();
        if (!canProceed)
        {
          Debug.WriteLine("Skipping custom attribute: '{0}'", attribute.Constructor.ToString());
          continue;
        }

        output.Write(".custom ");

        var ctor = attribute.Constructor;
        if (ctor.HasThis) output.Write("instance ");
        output.Write(ctor.ToString());

        output.Write(" = { ");

        if (attribute.HasConstructorArguments)
        {
          for (int i = 0; i < attribute.ConstructorArguments.Count; i++)
          {
            if (i > 0) output.WriteSpace();
            var argument = attribute.ConstructorArguments[i];
            string valueFormat = argument.Type == argument.Type.Module.TypeSystem.String ? "'{0}'" : "{0}";
            output.Write("{0}({1})", argument.Type.Name, string.Format(valueFormat, argument.Value));
          }
        }

        if (attribute.HasProperties)
        {
          output.WriteSpace();
          for (int i = 0; i < attribute.Properties.Count; i++)
          {
            var property = attribute.Properties[i];
            if (string.IsNullOrWhiteSpace(property.Name)) continue;
            if (i > 0) output.WriteSpace();
            var argument = property.Argument;
            string valueFormat = argument.Type == argument.Type.Module.TypeSystem.String ? "'{0}'" : "{0}";
            output.Write("{0}={1}({2})", property.Name, argument.Type.Name, string.Format(valueFormat, argument.Value));
          }
        }

        if (attribute.HasFields)
        {
          output.WriteSpace();
          for (int i = 0; i < attribute.Fields.Count; i++)
          {
            var field = attribute.Fields[0];
            if (string.IsNullOrWhiteSpace(field.Name)) continue;
            if (i > 0) output.WriteSpace();
            var argument = field.Argument;
            string valueFormat = argument.Type == argument.Type.Module.TypeSystem.String ? "'{0}'" : "{0}";
            output.Write("{0}={1}({2})", field.Name, argument.Type.Name, string.Format(valueFormat, argument.Value));
          }
        }

        output.Write(" }");
        output.WriteLine();
      }
    }
  }
}
