#include <atmoic>
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

  class Task {
   public:
    Task(Tasks &tasks) : _tasks(tasks) {}

    void start() {
      while (true) {
        Position begin = _tasks.next();
        if (begin == All::instance()->end()) return;

        std::unique_ptr<StripeCandidateSet> candidates = std::make_unique<StripeCandidateSet>();
        for (Position i = begin; i < All::instance()->end() - 1; i++) {
          for (Position j = i + 1; j < All::instance()->end(); j++) {
            Pair p(i, j);
            std::cout << "• " << p << std::endl;

            // TODO: access pattern for _tasks.bound
            if (p.taxi() < _tasks.bound) candidates->emplace_back(p);
            if (p.bound() < _tasks.bound) {
              std::cout << "---- bound " << p.bound() << " ----" << std::endl;
              _tasks.bound = p.bound();
            }
          }
        }

        _tasks.commit(std::move(candidates));
      }
    }

   private:
    Tasks &_tasks;
  };

 public:
  Tasks() : _bound(std::numeric_limits<unsigned>::max()), _next(All::instance()->begin()) {}

  void start() {
    std::vector<std::thread> threads;
    std::vector<Task> tasks;

    for (unsigned i = 0; i < max; i++) {
      tasks.push_back(Task(*this));
      threads.push_back(std::thread(&Task::start, std::ref(tasks.back()));
    }

    // TODO: wait until _next reaches the end

    for (std::thread &t : threads) t.join();
  }

  void report() {
    std::cout << "• check " << _candidates.size() << " candidate stripes" << std::endl;
    for (std::unique_ptr<StripeCandidateSet> &strip : _candidates) {
      for (const Pair &p : *candidates) std::cout << "  • " << p << std::endl;
    }
  }

 private:
  Position next() {
    std::unique_lock<std::mutex> access(_x);

    if (_next == All::instance()->end())
      return _next;
    else
      return _next++;
  }

  void commit(std::unique_ptr<StripeCandidateSet> candidates) {
    std::lock_guard<std::mutex> access(_x);

    _candidates.push_back(std::move(candidate));
  }

  std::mutex _x;
  std::atomic<unsigned> _bound;

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

  for (unsigned i = 0; i < 100; i++) {
    All::instance()->emplace_back(Point({d(g), d(g)}));
  }
}

int main() {
  generate();
  nearest();

  return 0;
}
