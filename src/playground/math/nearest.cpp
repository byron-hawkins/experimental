#include <iostream>
#include <random>
#include <vector>

template <typename T>
class handle {
 public:
  handle() : _t(nullptr) {}
  handle(T &t) : _t(&t) {};
  handle(nullptr_t) : _t(nullptr) {}

  T *operator->() { return _t; }
  T &operator*() { return *_t; }

 private:
  T *_t;
};

using Point = std::vector<int>;
using PointSet = std::vector<Point>;
using Position = typename PointSet::const_iterator;

class Pair {
  static handle<PointSet> points;

 public:
  Pair(Position a, Position b) : _a(a), _b(b) {}

  static void initialize(handle<PointSet> p) { points = p; }

  unsigned taxi() { return abs(_a->at(0) - _b->at(0)) + abs(_a->at(1) - _b->at(1)); }

  unsigned bound() {
    unsigned d = taxi();
    return (d + (d >> 1));
  }

  Position a() { return _a; }
  Position b() { return _b; }

  std::vector<int> indexes() {
    return {static_cast<int>(std::distance(_a, points->cbegin())), static_cast<int>(std::distance(_b, points->cbegin()))};
  }

 private:
  Position _a;
  Position _b;
};

handle<PointSet> Pair::points;

void nearest(PointSet &input) {
  Pair::initialize(input);

  for (Position i = input.cbegin(); i < (input.cend() - 1); i++) {
    for (Position j = (i + 1); j < input.cend(); j++) {
      Pair p(i, j);
      std::cout << "• [" << p.a()->at(0) << ", " << p.a()->at(1) << "] x [" << p.b()->at(0) << ", " << p.b()->at(1)
                << "]: " << "texi = " << p.taxi() << ", bound = " << p.bound() << std::endl;
    }
  }
}

int main() {
  PointSet p;
  for (int i = 0; i < 10; i++) {
    for (int j = 0; j < 10; j++) {
      p.emplace_back(Point({i, j}));
    }
  }

  nearest(p);

  return 0;
}
