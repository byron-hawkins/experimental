#include <atomic>
#include <chrono>
#include <condition_variable>
#include <iostream>
#include <mutex>
#include <thread>
#include <vector>

using namespace std::chrono_literals;

class Adder {
 public:
  Adder(std::mutex &m, std::condition_variable &c)
      : _m(m), _c(c), _started(false), _t() {}
  Adder(const Adder &source)
      : _m(source._m), _c(source._c), _started(false), _t() {}

  void start() { _t = std::thread(&Adder::run, this); }

  void run() {
    std::unique_lock<std::mutex> group(_m);

    _started = true;
    for (unsigned i = 0; i < 10; i++)
      std::cout << "Adder on thread " << _t.get_id() << std::endl;

    _c.wait(group);
    std::cout << "• done on " << _t.get_id() << std::endl;
  }

  bool started() {
    std::unique_lock<std::mutex> group(_m);
    return _started;
  }

  void join() { _t.join(); }

 private:
  std::mutex &_m;
  std::condition_variable &_c;
  bool _started;

  std::thread _t;
};

class Adders {
 public:
  Adders() : _adders(5, Adder(_m, _c)) {}

  void start() {
    for (Adder &adder : _adders) adder.start();

    for (Adder &adder : _adders) {
      while (!adder.started()) std::this_thread::sleep_for(1ms);

      std::unique_lock<std::mutex> notify(_m);
      _c.notify_all();
    }

    for (Adder &adder : _adders) adder.join();
  }

 private:
  std::mutex _m;
  std::condition_variable _c;

  std::vector<Adder> _adders;
};

int main() {
  Adders adders;
  adders.start();

  return 0;
}
