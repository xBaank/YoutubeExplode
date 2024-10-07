using Acornima;
using Acornima.Ast;

namespace YoutubeExplode.Utils;

internal static class Js
{
    public static string? ExtractFunction(string jsCode, string functionName)
    {
        // Parse the JavaScript code into an AST
        var parser = new Parser();
        var program = parser.ParseScript(jsCode);
        var function = FindFunctionByName(program, functionName);

        return function?.ToString();
    }

    // Recursive method to find a function declaration by name
    private static AssignmentExpression? FindFunctionByName(Program program, string functionName)
    {
        foreach (var statement in program.Body)
        {
            // Recursively search in child nodes if it's a statement
            var childFunction = FindFunctionInChildNodes(statement, functionName);
            if (childFunction != null)
            {
                return childFunction;
            }
        }
        return null; // Function not found
    }

    // Helper method to recursively search for function declarations in child nodes
    private static AssignmentExpression? FindFunctionInChildNodes(Node node, string functionName)
    {
        // For other node types, check if they have child nodes
        foreach (var childNode in node.ChildNodes)
        {
            if (
                childNode is AssignmentExpression functionDecl
                && functionDecl.Left is Identifier identifier
                && identifier.Name == functionName
            )
            {
                return functionDecl;
            }

            // Recursively search in the child node
            var result = FindFunctionInChildNodes(childNode, functionName);
            if (result is not null)
            {
                return result;
            }
        }

        return null; // Function not found in child nodes
    }
}
