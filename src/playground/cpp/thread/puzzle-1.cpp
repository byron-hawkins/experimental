#include <iostream>

static constexpr unsigned testspan = 10000;

template <unsigned Factor, unsigned Skip>
class OneFactor {
 public:
  OneFactor() : _next(Factor), _tick(Skip) {}

  void advance() {
    unsigned step = 1;
    _tick--;
    if (_tick == 0) {
      step = 2;
      _tick = Skip;
    }
    _next += (Factor * step);
  }

 private:
  unsigned _next;
  unsigned _tick;
};

int main() {
  std::cout << "Hello, World!\n";
  return 0;
}
