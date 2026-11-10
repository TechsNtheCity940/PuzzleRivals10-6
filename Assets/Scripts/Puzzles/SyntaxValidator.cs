using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

// Simple syntax verification for our modified puzzle files
public class SyntaxValidator
{
    public static void Main()
    {
        Console.WriteLine("Starting syntax validation for modified puzzle files...");
        Console.WriteLine();
        
        List<string> filesToCheck = new List<string>
        {
            @"D:\PuzzleRivals\Assets\Scripts\Puzzles\CircuitScrambleTraps.cs",
            @"D:\PuzzleRivals\Assets\Scripts\Puzzles\CircuitScramblePuzzle.cs",
            @"D:\PuzzleRivals\Assets\Scripts\Puzzles\MirrorGroupManager.cs",
            @"D:\PuzzleRivals\Assets\Scripts\Puzzles\MirrorMazePuzzle.cs",
            @"D:\PuzzleRivals\Assets\Scripts\Puzzles\GridGravitySystem.cs",
            @"D:\PuzzleRivals\Assets\Scripts\Puzzles\CollapseGridPuzzle.cs"
        };
        
        int successCount = 0;
        int errorCount = 0;
        
        foreach (string file in filesToCheck)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"ERROR: File not found - {file}");
                errorCount++;
                continue;
            }
            
            try
            {
                string code = File.ReadAllText(file);
                
                // Basic checks
                if (CheckBracesBalanced(code, Path.GetFileName(file)))
                {
                    Console.WriteLine($"✓ PASS: {Path.GetFileName(file)} - Braces balanced");
                    successCount++;
                }
                else
                {
                    Console.WriteLine($"✗ FAIL: {Path.GetFileName(file)} - Unbalanced braces");
                    errorCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ ERROR: {Path.GetFileName(file)} - {ex.Message}");
                errorCount++;
            }
        }
        
        Console.WriteLine();
        Console.WriteLine($"Validation complete: {successCount} passed, {errorCount} failed");
        
        if (errorCount == 0)
        {
            Console.WriteLine("All files passed basic syntax validation!");
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("Some files failed validation. Please check the output above.");
            Environment.Exit(1);
        }
    }
    
    private static bool CheckBracesBalanced(string code, string fileName)
    {
        int openCount = 0;
        int lineNumber = 0;
        
        using (StringReader reader = new StringReader(code))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                
                // Remove comments to avoid counting braces in comments
                int commentIndex = line.IndexOf("//");
                if (commentIndex >= 0)
                {
                    line = line.Substring(0, commentIndex);
                }
                
                // Count braces
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == '{') openCount++;
                    if (line[i] == '}') openCount--;
                    
                    // Early exit if we go negative (more closing than opening)
                    if (openCount < 0)
                    {
                        Console.WriteLine($"  Unmatched closing brace at line {lineNumber}");
                        return false;
                    }
                }
            }
        }
        
        return openCount == 0;
    }
}