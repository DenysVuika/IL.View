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
using System.Threading;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using System.Collections.ObjectModel;

namespace IL.View.Model
{
  /// <summary>
  /// Options passed to the decompiler.
  /// </summary>
  public class DecompilationOptions
  {
    /// <summary>
    /// Gets whether a full decompilation (all members recursively) is desired.
    /// If this option is false, language bindings are allowed to show the only headers of the decompiled element's children.
    /// </summary>
    public bool FullDecompilation { get; set; }

    /// <summary>
    /// Gets/Sets the directory into which the project is saved.
    /// </summary>
    public string SaveAsProjectDirectory { get; set; }

    /// <summary>
    /// Gets the cancellation token that is used to abort the decompiler.
    /// </summary>
    /// <remarks>
    /// Decompilers should regularly call <c>options.CancellationToken.ThrowIfCancellationRequested();</c>
    /// to allow for cooperative cancellation of the decompilation task.
    /// </remarks>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets the settings for the decompiler.
    /// </summary>
    public DecompilerSettings DecompilerSettings { get; set; }

    public DecompilationOptions()
    {
      DecompilerSettings s = new DecompilerSettings();
      s.AnonymousMethods = false;
      s.YieldReturn = false;
      this.DecompilerSettings = s;
    }
  }

  /// <summary>
  /// Base class for language-specific decompiler implementations.
  /// </summary>
  public abstract class Language
  {
    /// <summary>
    /// Gets the name of the language (as shown in the UI)
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the file extension used by source code files in this language.
    /// </summary>
    public abstract string FileExtension { get; }

    /// <summary>
    /// Gets the syntax highlighting used for this language.
    /// </summary>
    //public virtual ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition SyntaxHighlighting
    //{
    //  get
    //  {
    //    return ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(this.FileExtension);
    //  }
    //}

    public virtual void DecompileMethod(MethodDefinition method, ITextOutput output, DecompilationOptions options)
    {
      WriteCommentLine(output, TypeToString(method.DeclaringType, true) + "." + method.Name);
    }

    public virtual void DecompileProperty(PropertyDefinition property, ITextOutput output, DecompilationOptions options)
    {
      WriteCommentLine(output, TypeToString(property.DeclaringType, true) + "." + property.Name);
    }

    public virtual void DecompileField(FieldDefinition field, ITextOutput output, DecompilationOptions options)
    {
      WriteCommentLine(output, TypeToString(field.DeclaringType, true) + "." + field.Name);
    }

    public virtual void DecompileEvent(EventDefinition ev, ITextOutput output, DecompilationOptions options)
    {
      WriteCommentLine(output, TypeToString(ev.DeclaringType, true) + "." + ev.Name);
    }

    public virtual void DecompileType(TypeDefinition type, ITextOutput output, DecompilationOptions options)
    {
      WriteCommentLine(output, TypeToString(type, true));
    }

    public virtual void DecompileNamespace(string nameSpace, IEnumerable<TypeDefinition> types, ITextOutput output, DecompilationOptions options)
    {
      WriteCommentLine(output, nameSpace);
    }

    public virtual void DecompileAssembly(AssemblyDefinition assembly, string fileName, ITextOutput output, DecompilationOptions options)
    {
      WriteCommentLine(output, fileName);
      WriteCommentLine(output, assembly.Name.FullName);
    }

    public virtual void WriteCommentLine(ITextOutput output, string comment)
    {
      output.WriteLine("// " + comment);
    }

    /// <summary>
    /// Converts a type reference into a string. This method is used by the member tree node for parameter and return types.
    /// </summary>
    public virtual string TypeToString(TypeReference type, bool includeNamespace, ICustomAttributeProvider typeAttributes = null)
    {
      if (includeNamespace)
        return type.FullName;
      else
        return type.Name;
    }

    /// <summary>
    /// Used for WPF keyboard navigation.
    /// </summary>
    public override string ToString()
    {
      return Name;
    }

    public virtual bool ShowMember(MemberReference member)
    {
      return true;
    }
  }

  public static class Languages
  {
    /// <summary>
    /// A list of all languages.
    /// </summary>
    public static readonly ReadOnlyCollection<Language> AllLanguages = Array.AsReadOnly(
      new Language[] {
				new CSharpLanguage(),
				new ILLanguage(true)
			});

    /// <summary>
    /// Gets a language using its name.
    /// If the language is not found, C# is returned instead.
    /// </summary>
    public static Language GetLanguage(string name)
    {
      return AllLanguages.FirstOrDefault(l => l.Name == name) ?? AllLanguages.First();
    }
  }

  /// <summary>
  /// IL language support.
  /// </summary>
  /// <remarks>
  /// Currently comes in two versions:
  /// flat IL (detectControlStructure=false) and structured IL (detectControlStructure=true).
  /// </remarks>
  public class ILLanguage : Language
  {
    bool detectControlStructure;

    public ILLanguage(bool detectControlStructure)
    {
      this.detectControlStructure = detectControlStructure;
    }

    public override string Name
    {
      get { return "IL"; }
    }

    public override string FileExtension
    {
      get { return ".il"; }
    }

    public override void DecompileMethod(MethodDefinition method, ITextOutput output, DecompilationOptions options)
    {
      new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken).DisassembleMethod(method);
    }

    public override void DecompileField(FieldDefinition field, ITextOutput output, DecompilationOptions options)
    {
      new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken).DisassembleField(field);
    }

    public override void DecompileProperty(PropertyDefinition property, ITextOutput output, DecompilationOptions options)
    {
      new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken).DisassembleProperty(property);
    }

    public override void DecompileEvent(EventDefinition ev, ITextOutput output, DecompilationOptions options)
    {
      new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken).DisassembleEvent(ev);
    }

    public override void DecompileType(TypeDefinition type, ITextOutput output, DecompilationOptions options)
    {
      new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken).DisassembleType(type);
    }

    public override void DecompileNamespace(string nameSpace, IEnumerable<TypeDefinition> types, ITextOutput output, DecompilationOptions options)
    {
      new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken).DisassembleNamespace(nameSpace, types);
    }

    public override void DecompileAssembly(AssemblyDefinition assembly, string fileName, ITextOutput output, DecompilationOptions options)
    {
      output.WriteLine("// " + fileName);
      output.WriteLine();

      new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken).WriteAssemblyHeader(assembly);
    }

    public override string TypeToString(TypeReference t, bool includeNamespace, ICustomAttributeProvider attributeProvider)
    {
      PlainTextOutput output = new PlainTextOutput();
      t.WriteTo(output, true, shortName: !includeNamespace);
      return output.ToString();
    }
  }

  /// <summary>
  /// Decompiler logic for C#.
  /// </summary>
  public class CSharpLanguage : Language
  {
    bool showAllMembers = false;
    Predicate<IAstTransform> transformAbortCondition = null;

    public override string Name
    {
      get { return "C#"; }
    }

    public override string FileExtension
    {
      get { return ".cs"; }
    }

    public override void DecompileMethod(MethodDefinition method, ITextOutput output, DecompilationOptions options)
    {
      AstBuilder codeDomBuilder = CreateAstBuilder(options, method.DeclaringType);
      codeDomBuilder.AddMethod(method);
      codeDomBuilder.GenerateCode(output, transformAbortCondition);
    }

    public override void DecompileProperty(PropertyDefinition property, ITextOutput output, DecompilationOptions options)
    {
      AstBuilder codeDomBuilder = CreateAstBuilder(options, property.DeclaringType);
      codeDomBuilder.AddProperty(property);
      codeDomBuilder.GenerateCode(output, transformAbortCondition);
    }

    public override void DecompileField(FieldDefinition field, ITextOutput output, DecompilationOptions options)
    {
      AstBuilder codeDomBuilder = CreateAstBuilder(options, field.DeclaringType);
      codeDomBuilder.AddField(field);
      codeDomBuilder.GenerateCode(output, transformAbortCondition);
    }

    public override void DecompileEvent(EventDefinition ev, ITextOutput output, DecompilationOptions options)
    {
      AstBuilder codeDomBuilder = CreateAstBuilder(options, ev.DeclaringType);
      codeDomBuilder.AddEvent(ev);
      codeDomBuilder.GenerateCode(output, transformAbortCondition);
    }

    public override void DecompileType(TypeDefinition type, ITextOutput output, DecompilationOptions options)
    {
      AstBuilder codeDomBuilder = CreateAstBuilder(options, type);
      codeDomBuilder.AddType(type);
      codeDomBuilder.GenerateCode(output, transformAbortCondition);
    }

    public override void DecompileAssembly(AssemblyDefinition assembly, string fileName, ITextOutput output, DecompilationOptions options)
    {
      /*
      if (options.FullDecompilation)
      {
        //if (options.SaveAsProjectDirectory != null)
        //{
        //  HashSet<string> directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        //  var files = WriteCodeFilesInProject(assembly, options, directories).ToList();
        //  files.AddRange(WriteResourceFilesInProject(assembly, fileName, options, directories));
        //  WriteProjectFile(new TextOutputWriter(output), files, assembly.MainModule);
        //}
        //else
        {
          foreach (TypeDefinition type in assembly.MainModule.Types)
          {
            if (AstBuilder.MemberIsHidden(type, options.DecompilerSettings))
              continue;
            AstBuilder codeDomBuilder = CreateAstBuilder(options, type);
            codeDomBuilder.AddType(type);
            codeDomBuilder.GenerateCode(output, transformAbortCondition);
            output.WriteLine();
          }
        }
      }
      else
      */
      {
        base.DecompileAssembly(assembly, fileName, output, options);
        AstBuilder codeDomBuilder = CreateAstBuilder(options, currentType: null);
        codeDomBuilder.AddAssembly(assembly, onlyAssemblyLevel: true);
        codeDomBuilder.GenerateCode(output, transformAbortCondition);
      }
    }

    public override string TypeToString(TypeReference type, bool includeNamespace, ICustomAttributeProvider typeAttributes)
    {
      AstType astType = AstBuilder.ConvertType(type, typeAttributes);
      if (!includeNamespace)
      {
        var tre = new TypeReferenceExpression { Type = astType };
        tre.AcceptVisitor(new RemoveNamespaceFromType(), null);
        astType = tre.Type;
      }

      StringWriter w = new StringWriter();
      if (type.IsByReference)
      {
        ParameterDefinition pd = typeAttributes as ParameterDefinition;
        if (pd != null && (!pd.IsIn && pd.IsOut))
          w.Write("out ");
        else
          w.Write("ref ");

        if (astType is ComposedType && ((ComposedType)astType).PointerRank > 0)
          ((ComposedType)astType).PointerRank--;
      }

      astType.AcceptVisitor(new OutputVisitor(w, new CSharpFormattingPolicy()), null);
      return w.ToString();
    }

    AstBuilder CreateAstBuilder(DecompilationOptions options, TypeDefinition currentType)
    {
      return new AstBuilder(
        new DecompilerContext
        {
          CancellationToken = options.CancellationToken,
          CurrentType = currentType,
          Settings = options.DecompilerSettings
        });
    }

    sealed class RemoveNamespaceFromType : DepthFirstAstVisitor<object, object>
    {
      public override object VisitMemberType(MemberType memberType, object data)
      {
        base.VisitMemberType(memberType, data);
        SimpleType st = memberType.Target as SimpleType;
        if (st != null && !st.TypeArguments.Any())
        {
          SimpleType newSt = new SimpleType(memberType.MemberName);
          memberType.TypeArguments.MoveTo(newSt.TypeArguments);
          memberType.ReplaceWith(newSt);
        }
        return null;
      }
    }

    public override bool ShowMember(MemberReference member)
    {
      return showAllMembers || !AstBuilder.MemberIsHidden(member, new DecompilationOptions().DecompilerSettings);
    }
  }
}
