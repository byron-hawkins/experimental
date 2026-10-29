using System;
using System.Diagnostics;
using System.CommandLine;
using System.Collections.Concurrent;

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

class Test {
  public void evaluate(ConcurrentDictionary<StringRadix, Anagram> radixGroups) {
    Console.WriteLine($"• evaluating {radixGroups.Count()} radix groups");
    foreach (KeyValuePair<StringRadix, Anagram> entry in radixGroups) {
      Console.WriteLine($"  • evaluating radix group {entry.Key}");
      foreach (Group g in entry.Value.groups()) {
        if (!evaluate(g)) { Console.WriteLine($"    • Error swap coverage is incomplete on group {g.root()}"); }
        foreach (Group g2 in entry.Value.groups()) {
          if (!evaluate(g, g2)) { Console.WriteLine($"    • Error swap found between {g.root()} and {g2.root()}"); }
        }
      }
    }
  }

  private bool evaluate(Group g) {
    if (g.Count() == 1) return true;

    bool[] visited = new bool[g.Count()];
    List<string> s = new(g.members());

    List<int> q = new();
    visited[0] = true;
    q.Add(0);
    int coverage = 1;
    while (q.Count > 0) {
      int here = q[q.Count - 1];
      q.RemoveAt(q.Count - 1);
      for (int i = 0; i < s.Count; i++) {
        if (visited[i]) continue;
        if (swappable(s[here], s[i])) {
          if (++coverage == s.Count) return true;
          visited[i] = true;
          q.Add(i);
        }
      }
    }
    return false;
  }

  private bool evaluate(Group one, Group two) {
    if (one == two) return true;
    foreach (string s1 in one.members()) {
      foreach (string s2 in two.members()) {
        if (swappable(s1, s2)) return false;
      }
    }
    return true;
  }

  private bool swappable(string a, string b) {
    uint delta = 0;
    for (int i = 0; i < a.Length; i++) {
      if (a[i] != b[i]) delta++;
    }
    return delta == 0 || delta == 2;
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
    Option<bool> parallel = new("--parallelize", "-p");
    parallel.Description = "Parallelize the workload";
    parallel.DefaultValueFactory = parseResult => false;
    cli.Add(parallel);
    if (!cli.parse(args)) cli.query();

    string[] corpus =
        InputGenerator.Uniform.strings(cli.arguments.count, new InputGenerator.LowerCaseAlphabet(), new InputGenerator.FixedRandomStringLength(12));
    corpus = InputGenerator.StringProcessor.expand(corpus, new pod.Range<int>(3, 5), new pod.Range<int>(5, 8));

    if (cli.arguments.verbose) {
      Console.WriteLine($"• {cli.arguments.count} random strings:");
      foreach (string s in corpus) Console.WriteLine($"  • {s}");
    }

    ConcurrentDictionary<StringRadix, Anagram> radixGroups = new();

    bool parallelize = prallel.GetValue();
    if (parallelize) {
      Parallel.Radix.Split(radixGroups);
    } else {
      Sequential.Radix.Split(radixGroups);
    }

    if (cli.arguments.verbose) {
      Console.WriteLine("• anagrams:");
      foreach (KeyValuePair<StringRadix, Anagram> entry in radixGroups) { Console.WriteLine($"  • Anagram {entry.Key} of {entry.Value.Count()} strings"); }
    }

    foreach (KeyValuePair<StringRadix, Anagram> entry in radixGroups) entry.Value.decompose();

    if (cli.arguments.verbose) {
      Console.WriteLine("• groups:");
      foreach (KeyValuePair<StringRadix, Anagram> entry in radixGroups) {
        foreach (Group g in entry.Value.groups()) g.print();
      }
    }

    Test test = new();
    test.evaluate(radixGroups);

    // TODO: test it by groups:
    // • in group:
    //   • swappable by delta count
    //   • check graph connected
    // • cross group: no 2-delta
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
