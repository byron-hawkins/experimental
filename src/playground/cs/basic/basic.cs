using System;
using System.Diagnostics;

class Computation {
  public Computation() {
    _timer = new();
  }

  public void generate(int count) {
    Console.WriteLine($"• generate {count} random numbers with equal distribution");

    _timer.Start();

    Random r = new();
    List<int> list = new();
    for (int i = 0; i < count; i++) {
      list.Append(r.Next());
      // Console.WriteLine($"  • {r.Next()}");
    }

    _timer.Stop();
    Console.WriteLine($"  • elapsed time: {_timer.Elapsed.TotalMilliseconds.ToString("0.00###")}");
  }

  private Stopwatch _timer;
}

public class Bootstrap {
  public static void Main(String[] args) {
    InputParser.Quantity cli = new();
    if (!cli.parse(args))
      cli.query();

    Computation c = new();
    c.generate(cli.arguments.count);
  }
}
