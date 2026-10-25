using System;

// TODO: dislike spurious signedness for integer types

namespace InputGenerator {

  interface IRandomStringLength {
    int next();
  }

  interface IRandomStringAlphabet {
    int count();
    char get(int i);
  }

  class FixedRandomStringLength : IRandomStringLength {
    public FixedRandomStringLength(int length) { _length = length; }

    public int next() { return _length; }

    private int _length;
  }

  class LowerCaseAlphabet : IRandomStringAlphabet {
    public int count() { return 26; }
    public char get(int i) { return (char) (a + i); }

    private static readonly int a = (int) 'a';
  }

  class Uniform {
    public static int[] int32(uint count, int? min = null, int? max = null) {
      Console.WriteLine($"• generate {count} random 32-bit integers with equal distribution");

      Random r = new();
      int[] a = new int[count];
      if (max is int sentinel) {
        if (min is int least) {
          for (int i = 0; i < count; i++) { a[i] = r.Next(least, sentinel); }
        } else {
          for (int i = 0; i < count; i++) { a[i] = r.Next(sentinel); }
        }
      } else {
        for (int i = 0; i < count; i++) { a[i] = r.Next(); }
      }
      return a;
    }

    public static InputData.Point<int>[] int32x2d(int count) {
      Console.WriteLine($"• generate {count} random 32-bit integer 2D pointu with equal distribution");

      Random r = new();
      InputData.Point<int>[] a = new InputData.Point<int>[count];
      for (int i = 0; i < count; i++) { a[i] = new InputData.Point<int>(r.Next(), r.Next()); }
      return a;
    }

    public static string[] strings(uint count, IRandomStringAlphabet ab, IRandomStringLength length) {
      Random r = new();
      string[] s = new string[count];
      for (uint i = 0; i < count; i++) {
        char[] c = Enumerable.Range(0, length.next()).Select(_ => ab.get(r.Next(ab.count()))).ToArray();
        s[i] = new string(c);
      }
      return s;
    }
  }
}
