using System;

namespace InputData {

  public struct Point<T> {
    T x;
    T y;

    public Point(T to_x, T to_y) {
      x = to_x;
      y = to_y;
    }
  };

}
