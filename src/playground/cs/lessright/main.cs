using System;
using System.Diagnostics;

class Node {
  struct Counts {
    public uint self;
    public uint left;

    public Counts() {
      self = 1;
      left = 0;
    }

    public uint total() { return self + left; }
  }

  public Node(int height) {
    _height = height;
    _counts = new();
    _left = _right = null;
  }

  public uint count() { return _counts.total(); }
  public int height() { return _height; }
  public void increment(uint amount = 1) { _counts.left += amount; }
  public uint repeat() {
    _counts.self++;
    return _counts.left;
  }

  public bool isLeaf() { return _left == null && _right == null; }
  public Node? left() { return _left; }
  public void left(Node l) { _left = l; }
  public Node? right() { return _right; }
  public void right(Node r) { _right = r; }

  public interface Visitor {
    void enter(Node n);
    void at(Node n);
    void exit(Node n);
  }

  public void traverse(Visitor v) {
    v.enter(this);
    if (_left != null) _left.traverse(v);
    v.at(this);
    if (_right != null) _right.traverse(v);
    v.exit(this);
  }

  private int _height;
  private Counts _counts;

  private Node? _left;
  private Node? _right;
}

static class Extension {
  public static IEnumerable<T> Invert<T>(this T[] c, uint skip = 0) {
    uint end = ((uint)c.Length);
    for (uint i = (end - skip); i > 0U; i--) yield return c[i - 1];
  }
}

class Printer : Node.Visitor {
  public Printer() { _indent = 0; }

  public void enter(Node n) { _indent++; }
  public void at(Node n) { Console.WriteLine("".PadLeft(_indent) + $"• count {n.count()}, height {n.height()}"); }
  public void exit(Node n) { _indent--; }

  private int _indent;
}

class Computation {
  public Computation(uint count, bool verbose) {
    _verbose = verbose;

    _timer = new();
    _count = count;
    _input = new int[0];

    _progress = (count - 1U);
    _output = new uint[count];

    _root = new(0); /* placeholder to avoid possible null */
  }

  public void prepare() { _input = InputGenerator.Uniform.int32(_count, min: -1000000, max: 1000000); }

  public void start() {
    _timer.Start();

    if (_verbose) {
      Console.WriteLine($"• input check:");
      foreach (int i in _input) Console.WriteLine($"  • {i}");
    }

    _output[_progress--] = 0;
    _root = new(_input[_input.Length - 1]);
    foreach (int i in _input.Invert(skip: 1)) { _output[_progress--] = account(i); }

    _timer.Stop();
    if (_verbose) {
      Console.WriteLine("");
      Console.WriteLine("Final tree:");
      _root.traverse(new Printer());
    }
    Console.WriteLine("");
    Console.WriteLine($"• elapsed time: {_timer.Elapsed.TotalMilliseconds.ToString("0.00###")}");
    if (_verbose) {
      Console.WriteLine("");
      Console.WriteLine("• final counts:");
      foreach (uint c in _output) Console.WriteLine($"  • {c}");
    }
  }

  public void check() {
    uint errors = 0;
    for (uint i = 0; i < (_count - 1); i++) {
      uint tally = 0;
      for (uint j = i + 1; j < _count; j++)
        if (_input[i] > _input[j]) tally++;
      if (tally != _output[i]) {
        errors++;
        Console.WriteLine($"! Error: _output[{i}] = {_output[i]} does not match tally {tally}");
      }
    }
    if (errors == 0) Console.WriteLine("• check passed");
  }

  private uint account(int i) {
    uint count = 0;
    Node walk = _root;
    while (true) {
      if (i == walk.height()) return (count + walk.repeat());

      bool left = (i < walk.height());
      if (left) {
        walk.increment();
        if (walk.left() is Node step) {
          walk = step;
          continue;
        } else {
          walk.left(new Node(i));
          return count;
        }
      } else {
        count += walk.count();
        if (walk.right() is Node step) {
          walk = step;
        } else {
          walk.right(new Node(i));
          return count;
        }
      }
    }
  }

  private bool _verbose;

  private Stopwatch _timer;
  private uint _count;
  private int[] _input;

  uint _progress;
  private uint[] _output;

  private Node _root;
}

public class Executable {
  public static void Main(String[] args) {
    InputParser.Quantity cli = new();
    if (!cli.parse(args)) cli.query();

    Computation c = new(cli.arguments.count, cli.arguments.verbose);
    c.prepare();
    c.start();
    c.check();
  }
}
