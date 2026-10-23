using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace InputParser {

  class Quantity {
    public class Arguments {
      public int count;
    };

    public Arguments arguments;

    public Quantity() {
      arguments = new();
      root = new("Random number generator");
      count = new("--quantity", "-q");
      count.Description = "Quantity of random numbers to generate";
      count.DefaultValueFactory = parseResult => 0;
      root.Options.Add(count);
    }

    public bool parse(string[] args) {
      ParseResult result = root.Parse(args);
      if (result.Errors.Count > 0) {
        Console.WriteLine("Error parsing command line:");
        foreach (ParseError e in result.Errors) {
          Console.WriteLine($"• {e.Message}");
        }
        Environment.Exit(1);
      }
      return ((arguments.count = result.GetValue(count)) != 0);
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
          arguments.count = Int32.Parse(input);
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

    private RootCommand root;
    private Option<int> count;
  }

}
