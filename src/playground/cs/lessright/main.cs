using System;
using System.Diagnostics;

class Node {
  public struct Pivot {
    public Pivot(int position) { _integral = (1U << position); }

    public uint narrow(int i) { return ((uint)i) - (_integral >> 1); }

    public void raise(int steps = 1) { _integral <<= steps; }
    public void lower(int steps = 1) { _integral >>= steps; }

    public bool decides(uint i) { return i < _integral; }

    public uint debugvalue() { return _integral; }

    uint _integral;
  }

  public Node(uint to_inset, uint to_count, int height) {
    _inset = to_inset;
    _count = to_count;
    _height = height;
    _pivot = new(0);
    _left = _right = null;
  }

  public uint inset() { return _inset; }
  public uint count() { return _count; }
  public int height() { return _height; }
  public ref Pivot pivot() { return ref _pivot; }
  public void increment(uint amount = 1) { _count += amount; }

  public bool isLeaf() { return _left == null && _right == null; }
  public void left(Node l) { _left = l; }
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

  private uint _inset;
  private uint _count;
  private int _height;
  private Pivot _pivot;

  private Node? _left;
  private Node? _right;
}

static class Extension {
  public static IEnumerable<T> Invert<T>(this T[] c) {
    for (uint i = (uint)c.Length; i > 0U; i--) yield return c[i - 1];
  }
}

class Printer : Node.Visitor {
  public Printer() { _indent = 0; }

  public void enter(Node n) { _indent++; }
  public void at(Node n) {
    Console.WriteLine("".PadLeft(_indent) + $"• inset {n.inset()}, count {n.count()}, height {n.height()}, pivot {n.pivot().debugvalue()}");
  }
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

    _root = new(0U, 0U, Int32.MaxValue);
  }

  public void prepare() { _input = InputGenerator.Uniform.int32(_count); }

  public void start() {
    _timer.Start();

    Console.WriteLine($"• input check:");
    foreach (int i in _input) Console.WriteLine($"  • {i}");
    Console.WriteLine($"• reverse input check:");
    foreach (int i in _input.Invert()) Console.WriteLine($"  • {i}");

    foreach (int i in _input.Invert()) {
      account(i);
      break;
    }
    _root.traverse(new Printer());

    _timer.Stop();
    Console.WriteLine($"• elapsed time: {_timer.Elapsed.TotalMilliseconds.ToString("0.00###")}");
  }

  private void account(int i) {
    Console.WriteLine("account()");

    Node walk = _root;
    while (true) {
      if (walk.isLeaf()) {
        _output[_progress--] = walk.count();
        if (walk.height() == (uint)i) {
          walk.increment();
        } else {
          Console.WriteLine("raise pivot");
          while (!walk.pivot().decides((uint)i)) { walk.pivot().raise(); }
          uint inset = walk.pivot().narrow(i);
          walk.right(new Node(inset, 1, i));  // TODO: how to init the pivot?
        }
        return;
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
