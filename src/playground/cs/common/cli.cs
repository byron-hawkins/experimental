using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace InputParser {

  class Quantity {
    public class Arguments {
      public uint count;
    };

    public Arguments arguments;

    public Quantity() {
      arguments = new();
      _count = new("--quantity", "-q");
      _count.Description = "Quantity of random numbers to generate";
      _count.DefaultValueFactory = parseResult => 0U;
      _root = new("Random number generator");
      _root.Options.Add(_count);
    }

    public bool parse(string[] args) {
      ParseResult result = _root.Parse(args);
      if (result.Errors.Count > 0) {
        Console.WriteLine("Error parsing command line:");
        foreach (ParseError e in result.Errors) {
          Console.WriteLine($"• {e.Message}");
        }
        Environment.Exit(1);
      }
      return ((arguments.count = result.GetValue(_count)) != 0);
    }

    public void query() {
      string? input = "";

      do {
        try {
          Console.Write("Enter a quantity of random numbers: ");
          input = Console.ReadLine();
          if (String.IsNullOrWhiteSpace(input) || input.ToLower() == "x") {
            Console.WriteLine("• exiting");
            Environment.Exit(1);
          }
          arguments.count = UInt32.Parse(input);
          if (arguments.count == 0) {
            Console.WriteLine("• exiting");
            Environment.Exit(1);
          }
        } catch (FormatException e) {
          Console.WriteLine("Error: string '{input}' could not be parsed as an integer");
          Console.WriteLine($"• {e.Message}");
        }
      } while (arguments.count == 0);
    }

    private RootCommand _root;
    private Option<uint> _count;
  }

}
