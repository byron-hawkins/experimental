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

namespace OutputData {

  class Comma {
    public Comma() { _skip = true; }

    public override string ToString() {
      if (_skip) {
        _skip = false;
        return "";
      } else {
        return ",";
      }
    }

    private bool _skip;
  }

}

namespace pod {

  public struct Range<I>
      where I : System.Numerics.IBinaryInteger<I> {
    public I min;
    public I max;

    public Range(I least, I sentinel) {
      min = least;
      max = sentinel;
    }
  }

}
