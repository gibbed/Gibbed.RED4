using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RED4Generators
{
    [Generator]
    public class InstructionInfoGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            //Debugger.Launch();
        }

        private static readonly DiagnosticDescriptor DuplicateOpcodeError = new DiagnosticDescriptor(
#pragma warning disable RS2008 // Enable analyzer release tracking
            id: "RED4GEN001",
#pragma warning restore RS2008 // Enable analyzer release tracking
            title: "Duplicate instruction opcode",
            messageFormat: "Duplicate instruction opcode '{0}'",
            category: "RED4Generators",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var compilation = context.Compilation;

            var attributeSymbol = compilation.GetTypeByMetadataName("Gibbed.RED4.ScriptFormats.InstructionAttribute");

            var classSymbols = new List<ITypeSymbol>();
            foreach (var candidate in receiver.Candidates)
            {
                var model = compilation.GetSemanticModel(candidate.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(candidate);
                if (symbol.GetAttributes().Any(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)) == true)
                {
                    classSymbols.Add(symbol);
                }
            }

            string classSource = ProcessClasses(classSymbols, attributeSymbol, context);
            if (classSource == null)
            {
                return;
            }
            context.AddSource($"InstructionInfo.Generated.cs", SourceText.From(classSource, Encoding.UTF8));
        }

        private string ProcessClasses(List<ITypeSymbol> typeSymbols, ISymbol attributeSymbol, GeneratorExecutionContext context)
        {
            var opcodeHandlers = new Dictionary<byte, ITypeSymbol>();
            foreach (var typeSymbol in typeSymbols)
            {
                foreach (var attributeData in typeSymbol.GetAttributes().Where(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                {
                    var opcodeValue = (byte)attributeData.ConstructorArguments[0].Value;
                    if (opcodeHandlers.ContainsKey(opcodeValue) == true)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DuplicateOpcodeError, Location.None, opcodeValue.ToString()));
                        return null;
                    }
                    opcodeHandlers[opcodeValue] = typeSymbol;
                }
            }

            StringBuilder source = new StringBuilder($@"
namespace Gibbed.RED4.ScriptFormats
{{
    public partial class InstructionInfo
    {{
        internal static (int chainCount, ReadDelegate read, WriteDelegate write) GetInternal(Opcode opcode) =>
            opcode switch
            {{
");

            foreach (var kv in opcodeHandlers.OrderBy(kv => kv.Key))
            {
                var (opcodeValue, typeSymbol) = (kv.Key, kv.Value);
                var displayString = typeSymbol.ToDisplayString();
                source.AppendLine($@"                (Opcode){opcodeValue} => ({displayString}.ChainCount, {displayString}.Read, {displayString}.Write),");
            }

            source.AppendLine($"                _ => throw new System.ArgumentOutOfRangeException(nameof(opcode), $\"no info for {{opcode}}\"),");
            source.Append($@"
            }};
    }}
}}");
            return source.ToString();
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<TypeDeclarationSyntax> Candidates { get; } = new List<TypeDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
                    classDeclarationSyntax.AttributeLists.Count > 0)
                {
                    this.Candidates.Add(classDeclarationSyntax);
                }
                if (syntaxNode is StructDeclarationSyntax structDeclarationSyntax &&
                    structDeclarationSyntax.AttributeLists.Count > 0)
                {
                    this.Candidates.Add(structDeclarationSyntax);
                }
            }
        }
    }
}
