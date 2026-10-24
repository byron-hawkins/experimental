using System;

namespace InputGenerator {

  class Uniform {
    public static int[] int32(uint count) {
      Console.WriteLine($"• generate {count} random 32-bit integers with equal distribution");

      Random r = new();
      int[] a = new int[count];
      for (int i = 0; i < count; i++) {
        a[i] = r.Next();
      }
      return a;
    }

    public static InputData.Point<int>[] int32x2d(int count) {
      Console.WriteLine($"• generate {count} random 32-bit integer 2D pointu with equal distribution");

      Random r = new();
      InputData.Point<int>[] a = new InputData.Point<int>[count];
      for (int i = 0; i < count; i++) {
        a[i] = new InputData.Point<int>(r.Next(), r.Next());
      }
      return a;
    }
  }

}
