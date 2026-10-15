#include <iostream>
#include <limits>
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

  unsigned taxi() const { return abs(_a->at(0) - _b->at(0)) + abs(_a->at(1) - _b->at(1)); }

  unsigned bound() const {
    unsigned d = taxi();
    return (d + (d >> 1));
  }

  Position a() const { return _a; }
  Position b() const { return _b; }

  std::vector<int> indexes() {
    return {static_cast<int>(std::distance(_a, points->cbegin())), static_cast<int>(std::distance(_b, points->cbegin()))};
  }

  friend std::ostream &operator<<(std::ostream &o, const Pair &p) {
    return o << "[" << p.a()->at(0) << ", " << p.a()->at(1) << "] x [" << p.b()->at(0) << ", " << p.b()->at(1)
             << "]: " << "texi = " << p.taxi() << ", bound = " << p.bound();
  }

 private:
  Position _a;
  Position _b;
};

handle<PointSet> Pair::points;

void nearest(PointSet &input) {
  Pair::initialize(input);

  unsigned bound = std::numeric_limits<unsigned>::max();
  std::vector<Pair> candidates;

  for (Position i = input.cbegin(); i < (input.cend() - 1); i++) {
    for (Position j = (i + 1); j < input.cend(); j++) {
      Pair p(i, j);
      std::cout << "• " << p << std::endl;

      if (p.taxi() < bound) candidates.emplace_back(p);
      if (p.bound() < bound) {
        std::cout << "---- bound " << p.bound() << " ----" << std::endl;
        bound = p.bound();
      }
    }
  }

  std::cout << "• check " << candidates.size() << " candidates" << std::endl;
  for (const Pair &p : candidates) std::cout << "  • " << p << std::endl;
}

void generate(PointSet &p) {
  std::random_device r;
  std::mt19937 g(r());
  std::uniform_int_distribution d(-1000, 1000);

  for (unsigned i = 0; i < 100; i++) {
    p.emplace_back(Point({d(g), d(g)}));
  }
}

int main() {
  PointSet p;
  generate(p);
  nearest(p);

  return 0;
}
