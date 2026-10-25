using System;
using System.Diagnostics;

// class StringRadix {
//   public void
//
//       private uint buckets[26];
// }

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
  }
}
