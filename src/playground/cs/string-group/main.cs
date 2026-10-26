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
      shift(ref h, (int) count);
    }
    if (zeros > 0) shift(ref h, zeros);
    return h;
  }

  private readonly int _hash;
  private readonly uint[] _buckets;
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

    ConcurrentDictionary<StringRadix, List<string>> radixGroups = new();
    foreach (var s in corpus) {
      StringRadix r = new(s);   // TODO: where/how to keep these?
      List<string> g = radixGroups.GetOrAdd(r, new List<string>());
      g.Add(s);
    }

    if (cli.arguments.verbose) {
      Console.WriteLine("• radix groups:");
      foreach (var g in radixGroups) {   // TODO: what type is this?
        Console.WriteLine($"  • group of {g.Value.Count()} strings:");
        foreach (string s in g.Value) Console.WriteLine($"    • {s}");
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


   • similarity graph -> string groups by DFS in Nlog(N)

In the case of low radix diversity, better to just (N² * L), but the (N * A) radix cost is irrelevant. So radix wins.
in general by reducing (N² * L) to (NsqrtN * A), though less advantageous for (L < A).

*/
