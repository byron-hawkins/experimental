using System;
using System.Runtime.InteropServices;

struct Pair<T> {
  public T a;
  public T b;

  public Pair(T to_a, T to_b) {
    a = to_a;
    b = to_b;
  }
}


/*
struct Strings {
  string a;
  string b;

  public Strings(string to_a, string to_b) {
    a = to_a;
    b = to_b;
  }
}

struct Ints {
  int a;
  int b;
}
*/

public class StructTrial {

  public static void Main(string[] args) {
    StructTrial t = new();
    t.start();
  }

  private void morph(Pair<string> p) {
    p.a = "mno";
    p.b = "pqr";

    Console.WriteLine($"• morphed s on stack: [{p.a}, {p.b}]");
  }

  private void morph(Pair<int> p) {
    p.a = 3;
    p.b = 4;

    Console.WriteLine($"• morphed i on stack: [{p.a}, {p.b}]");
  }

  private void start() {
    Pair<string> s = new("abc", "def");
    morph(s);
    Console.WriteLine($"• morphed s globally? [{s.a}, {s.b}]");

    Pair<int> i = new(1, 2);
    Pair<int> j = new(1, 2);
    morph(i);
    Console.WriteLine($"• morphed i globally? [{i.a}, {i.b}]");

    /*
    GCHandle h = GCHandle.Alloc(i, GCHandleType.WeakTrackResurrection);
    nint address = GCHandle.ToIntPtr(h);
    Console.WriteLine($"• maybe i is at 0x{address:X}?");
    */

    List<Pair<int>> list = new();
    for (int k = 0; k < 10; k++) {
      list.Add(new Pair<int>(k, k + 1));
    }

    Console.WriteLine($"• list[{list.Count}]:");
    for (int l = 0; l < list.Count; l++) {
      Pair<int> p = list[l];
      Console.WriteLine($"  • [{p.a}, {p.b}]");
    }

    Span<Pair<int>> span = CollectionsMarshal.AsSpan(list);
    Console.WriteLine($"• span[{span.Length}]:");
    foreach (Pair<int> p in span) {
      Console.WriteLine($"  • [{p.a}, {p.b}]");
    }
    /*
    for (int l = 0; l < span.Length; l++) {
      Pair<int> p = span[l];
      Console.WriteLine($"  • [{p.a}, {p.b}]");
    }
    */

    // Console.WriteLine($"> waiting 20 seconds for gdb attach on {System.Diagnostics.Process.GetCurrentProcess().Id}");
    // System.Threading.Thread.Sleep(20000);
  }
}
