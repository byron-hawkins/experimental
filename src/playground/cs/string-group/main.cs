using System;
using System.Diagnostics;
using System.Collections.Concurrent;

class StringRadix {
  public StringRadix(string s) {
    _buckets = new uint[size];
    foreach (char c in s) _buckets[(c - a)]++;

    _hash = hash();
  }

  public static bool operator<(StringRadix left, StringRadix right) {
    for (uint i = 0; i < size; i++) {
      if (left._buckets[i] < right._buckets[i]) return true;
      if (left._buckets[i] > right._buckets[i]) return false;
    }
    return false;
  }

  public static bool operator>(StringRadix left, StringRadix right) {
    for (uint i = 0; i < size; i++) {
      if (left._buckets[i] > right._buckets[i]) return true;
      if (left._buckets[i] < right._buckets[i]) return false;
    }
    return false;
  }

  public static bool operator ==(StringRadix left, StringRadix right) {
    for (uint i = 0; i < size; i++)
      if (left._buckets[i] != right._buckets[i]) return false;
    return true;
  }

  public static bool operator !=(StringRadix left, StringRadix right) {
    for (uint i = 0; i < size; i++)
      if (left._buckets[i] != right._buckets[i]) return true;
    return false;
  }

  public override bool Equals(object? o) { return (o is StringRadix r) ? (this == r) : false; }
  public override int GetHashCode() { return _hash; }

  private static readonly int size = 26;
  private static readonly int a = (int)'a';

  private int hash() {
    void shift(ref int h, int i) { h = (((h << 5) + h) ^ i); }

    int h = 0;
    int zeros = 0;
    foreach (uint count in _buckets) {
      if (count == 0) {
        zeros++;
        continue;
      }
      if (zeros > 0) {
        shift(ref h, zeros);
        zeros = 0;
      }
      shift(ref h, (int)count);
    }
    if (zeros > 0) shift(ref h, zeros);
    return h;
  }

  private readonly int _hash;
  private readonly uint[] _buckets;
}

struct Swap {
  public string root;
  public string self;
  public int left;
  public int right;
}

class Group {
  public Group(string root) {
    _root = root;
    _members = new();
    _members.Add(_root);
  }

  public void Add(string s) { _members.Add(s); }

  public void print() {
    Console.WriteLine($"• group '{_root}' with {_members.Count()} members:");
    foreach (string s in _members) Console.WriteLine($"• {s}");
  }

  private string _root;
  private List<string> _members;
}

class Anagram : IEnumerable<Group>, System.Collections.IEnumerable {
  public Anagram() {
    _g = new();
    _q = new();
  }

  public int Count() { return _q.Count(); }
  public void Add(string s) { _q.AddLast(s); }

  System.Collections.IEnumerator System.Collections.IEnumerator.GetEnumerator() { return this.GetEnumerator(); }

  public IEnumerator<Group> GetEnumerator() { return _g.GetEnumerator(); }

  public void decompose() {
    List<Swap> swaps;
    while (_q.Count > 0) {
      LinkedListNode<string> top = _q.First, walk = top.Next, remove = null;
      Group g = new(top.Value);
      _g.Add(g);
      _q.RemoveFirst();
      while (walk is LinkedListNode<string>) {
        Swap? swap = findSwap(top.Value, walk.Value);
        if (swap is Swap member) {
          g.Add(member.self);
          swaps.Add(member);
          remove = walk;
        }
        walk = walk.Next;
        if (remove is LinkedListNode<string>) {
          _q.Remove(remove);
          remove = null;
        }
      }
      // drain swaps against _q
      for (int i = 0; i < swaps.Count(); i++) {
        if (_q.First is null) break;
        LinkedListNode<string> walk = _q.First, remove = null;
        do {
          Swap? swap = findSwap(swaps[i], walk.Value);
          if (swap is Swap member) {
            g.Add(member.self);
            swaps.Add(member);
            remove = walk;
          }
          walk = walk.Next;
          if (remove is LinkedListNode<string>) {
            _q.Remove(remove);
            remove = null;
          }
        } while (walk is LinkedListNode<string>);
      }
      swaps.Clear();
    }
  }

  private Swap? findSwap(string root, string s) { return null; }

  private Swap? findSwap(Swap member, string s) { return null; }

  private List<Group> _g;

  private LinkedList<string> _q;
}

namespace InputGenerator {

  class StringProcessor {
    public static string[] expand(string[] samples, pod.Range<int> radixRange, pod.Range<int> swapRange) {
      Random r = new();
      List<string> expanded = new(), radixSet = new(), swapSet = new();

      foreach (string s in samples) {
        if (s.Length < 2) throw new InvalidOperationException($"Cannot expand string of length {s.Length}");

        radixSet.Clear();
        swapSet.Clear();

        int radixCount = r.Next(radixRange.min, radixRange.max);
        for (int i = 0; i < radixCount; i++) {
          char[] shuffled = s.ToArray();
          r.Shuffle(shuffled);
          radixSet.Add(new string(shuffled));
        }

        int swapCount = r.Next(swapRange.min, swapRange.max);
        for (int i = 0; i < swapCount; i++) {
          char[] swapped = radixSet[r.Next(radixSet.Count())].ToArray();
          int first = r.Next(swapped.Length), second;
          do { second = r.Next(swapped.Length); } while (first == second);
          char swap = swapped[second];
          swapped[second] = swapped[first];
          swapped[first] = swap;
          swapSet.Add(new string(swapped));
        }

        expanded.AddRange(radixSet);
        expanded.AddRange(swapSet);
      }

      return expanded.ToArray();
    }
  }

}

/*
 * metrics: n total strings, s = |string|, r = unique radix bins
 * • for each string (n)
 *   • radix bin each string (s)
 *   • join s to r in sorted tree (s * log(r))
 * • for each radix bin (r)
 *   • for each s in r (n):
 */
public class Executable {
  public static void Main(string[] args) {
    InputParser.Quantity cli = new();
    if (!cli.parse(args)) cli.query();

    string[] corpus =
        InputGenerator.Uniform.strings(cli.arguments.count, new InputGenerator.LowerCaseAlphabet(), new InputGenerator.FixedRandomStringLength(5));
    corpus = InputGenerator.StringProcessor.expand(corpus, new pod.Range<int>(3, 5), new pod.Range<int>(5, 8));

    if (cli.arguments.verbose) {
      Console.WriteLine($"• {cli.arguments.count} random strings:");
      foreach (string s in corpus) Console.WriteLine($"  • {s}");
    }

    ConcurrentDictionary<StringRadix, Anagram> radixGroups = new();
    foreach (string s in corpus) {
      StringRadix r = new(s);
      Anagram a = radixGroups.GetOrAdd(r, new Anagram());
      a.Add(s);
    }

    if (cli.arguments.verbose) {
      Console.WriteLine("• radix groups:");
      foreach (KeyValuePair<StringRadix, Anagram> entry in radixGroups) {
        Console.WriteLine($"  • group of {entry.Value.Count()} strings:");
        foreach (Group g in entry.Value) g.print();
      }
    }
  }
}

/*

   • A: alphabet size
   • L: string length
   • N: string count
   • D: diversity (radix group count)
   • R: largest radix group size

   • case: L < A
   • case: high radix diversity
     • radix in N² * A
     • similarity graph in C: negligible radix group size
   • case: low radix diversity
     • radix in (N * A) + C
     • similarity graph in R² * L
   • case: balanced radix density
     • radix in Nsqrt(N) * A
     • similarity graph in N


   • within each radix group
     • every member gets a string group
     • enqueue all and foreach pop S
     • compare each q on Q and if swappable
       • add q to S.group
       • add q to S.queue
       • q retain swap positions A, B
     • for each on S.Q, compare against q on Q
       • if A, B are both same or both different, skip
       • position D is different, containing q.D
       • find q.D and retain swap positions D and q.D
       • add q to S group and S.Q

   • similarity graph -> string groups by DFS in Nlog(N)

In the case of low radix diversity, better to just (N² * L), but the (N * A) radix cost is irrelevant. So radix wins.
in general by reducing (N² * L) to (NsqrtN * A), though less advantageous for (L < A).

*/
