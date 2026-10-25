using System;
using System.Diagnostics;

class Node {
  public Node(int height) {
    _height = height;
    _count = 1;
    _left = _right = null;
  }

  public uint count() { return _count; }
  public int height() { return _height; }
  public void increment(uint amount = 1) { _count += amount; }

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
  private uint _count;

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
  public Computation(uint count) {
    _timer = new();
    _count = count;
    _input = new int[0];

    _progress = (count - 1U);
    _output = new uint[count];

    _root = new(0); /* placeholder to avoid possible null */
  }

  public void prepare() { _input = InputGenerator.Uniform.int32(_count); }

  public void start() {
    _timer.Start();

    Console.WriteLine($"• input check:");
    foreach (int i in _input) Console.WriteLine($"  • {i}");
    Console.WriteLine($"• reverse input check:");
    foreach (int i in _input.Invert()) Console.WriteLine($"  • {i}");

    _root = new(_input[_input.Length - 1]);
    foreach (int i in _input.Invert(skip: 1)) {
      account(i);
      break;
    }
    _root.traverse(new Printer());

    _timer.Stop();
    Console.WriteLine($"• elapsed time: {_timer.Elapsed.TotalMilliseconds.ToString("0.00###")}");
  }

  private uint account(int i) {
    Console.WriteLine("account()");

    uint count = 0;
    Node walk = _root;
    while (true) {
      if (i == walk.height()) {
        walk.increment();
        count += (walk.left()?.count() ?? 0);
        return count;
      }

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
        }
      }
    }
  }

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

    Computation c = new(cli.arguments.count);
    c.prepare();
    c.start();
  }
}
