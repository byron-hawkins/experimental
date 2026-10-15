#include <atomic>
#include <condition_variable>
#include <iostream>
#include <limits>
#include <mutex>
#include <random>
#include <thread>
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

struct All {
  static handle<PointSet> instance() {
    static PointSet singleton;
    return singleton;
  }
};

class Pair {
 public:
  Pair(Position a, Position b) : _a(a), _b(b) {}

  unsigned taxi() const { return abs(_a->at(0) - _b->at(0)) + abs(_a->at(1) - _b->at(1)); }

  unsigned bound() const {
    unsigned d = taxi();
    return (d + (d >> 1));
  }

  Position a() const { return _a; }
  Position b() const { return _b; }

  std::vector<int> indexes() {
    return {static_cast<int>(std::distance(_a, All::instance()->cbegin())), static_cast<int>(std::distance(_b, All::instance()->cbegin()))};
  }

  friend std::ostream &operator<<(std::ostream &o, const Pair &p) {
    return o << "[" << p.a()->at(0) << ", " << p.a()->at(1) << "] x [" << p.b()->at(0) << ", " << p.b()->at(1)
             << "]: " << "texi = " << p.taxi() << ", bound = " << p.bound();
  }

 private:
  Position _a;
  Position _b;
};

class Tasks {
  using StripeCandidateSet = std::vector<Pair>;
  using CandidateSet = std::vector<std::unique_ptr<StripeCandidateSet>>;

  static constexpr unsigned max = 8;

  void stripe() {
    std::cout << "• task starts, now have " << _running << std::endl;

    while (true) {
      Position begin = next();
      if (begin == All::instance()->end()) return;

      std::unique_ptr<StripeCandidateSet> candidates = std::make_unique<StripeCandidateSet>();
      for (Position step = begin + 1; step < All::instance()->end(); step++) {
        Pair p(begin, step);
        // std::cout << "• " << p << std::endl;

        unsigned best = _bound.load();
        if (p.taxi() < best) candidates->emplace_back(p);
        while (p.bound() < best) _bound.compare_exchange_strong(best, p.bound());
      }
      commit(std::move(candidates));
    }
  }

 public:
  Tasks() : _bound(std::numeric_limits<unsigned>::max()), _running(0), _next(All::instance()->begin()) {}

  void start() {
    std::unique_lock<std::mutex> access(_x);

    std::vector<std::thread> threads;
    for (unsigned i = 0; i < max; i++) {
      _running++;
      threads.push_back(std::thread(&Tasks::stripe, this));
    }

    _c.wait(access, [&running = _running]() { return running == 0; });

    for (std::thread &t : threads) t.join();
  }

  void report() {
    unsigned total = 0;

    std::cout << "• check " << _candidates.size() << " candidate stripes" << std::endl;

    for (std::unique_ptr<StripeCandidateSet> &stripe : _candidates) {
      total += stripe->size();
      for (const Pair &p : *stripe) std::cout << "  • " << p << std::endl;
    }

    std::cout << "• found " << total << " total conadidates" << std::endl;
  }

 private:
  Position next() {
    std::unique_lock<std::mutex> access(_x);

    if (_next != All::instance()->end()) _next++;
    if (_next == All::instance()->end()) {
      if (--_running == 0) _c.notify_one();

      std::cout << "• task completes, now have " << _running << std::endl;
    }
    return _next;
  }

  void commit(std::unique_ptr<StripeCandidateSet> stripe) {
    std::lock_guard<std::mutex> access(_x);

    _candidates.push_back(std::move(stripe));
  }

  std::mutex _x;
  std::condition_variable _c;
  std::atomic<unsigned> _bound;
  unsigned _running;

  CandidateSet _candidates;
  Position _next;
};

void nearest() {
  Tasks t;
  t.start();
  t.report();
}

void generate() {
  std::random_device r;
  std::mt19937 g(r());
  std::uniform_int_distribution d(-1000, 1000);

  for (unsigned i = 0; i < 1000; i++) {
    All::instance()->emplace_back(Point({d(g), d(g)}));
  }
}

int main() {
  generate();
  nearest();

  return 0;
}
