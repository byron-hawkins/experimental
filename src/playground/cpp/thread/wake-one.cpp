#include <array>
#include <condition_variable>
#include <iostream>
#include <mutex>
#include <thread>
#include <vector>

using namespace std::chrono_literals;

static bool done = false;

enum class Proceed { sleepy, pilot };

class Sleepy {
 public:
  Sleepy(unsigned index, std::mutex& m)
      : _proceed(Proceed::sleepy), _index(index), _m(m) {}
  Sleepy(Sleepy&& source)
      : _proceed(Proceed::sleepy), _index(source._index), _m(source._m), _c() {}

  void start() {
    std::unique_lock<std::mutex> access(_m);

    while (true) {
      release_pilot();
      _c.wait(access, [&who = _proceed]() { return who == Proceed::sleepy; });

      if (done) break;

      std::cout << "Sleepy thread " << _index << " has awoken" << std::endl;
    }

    release_pilot();
  }

  void wake() {
    std::unique_lock<std::mutex> access(_m);
    if (_proceed != Proceed::pilot) {
      _c.wait(access, [&who = _proceed]() { return who == Proceed::pilot; });
    }

    _c.notify_one();
    _proceed = Proceed::sleepy;
    _c.wait(access, [&who = _proceed]() { return who == Proceed::pilot; });
  }

 private:
  void release_pilot() {
    _proceed = Proceed::pilot;
    _c.notify_one();
  }

  volatile Proceed _proceed;
  const unsigned _index;

  std::mutex& _m;
  std::condition_variable _c;
};

int main() {
  std::mutex m;

  std::vector<Sleepy> sleepers;
  std::vector<std::thread> threads;
  for (unsigned i = 0; i < 4; i++) sleepers.emplace_back(i, m);

  for (Sleepy& s : sleepers)
    threads.push_back(std::thread(&Sleepy::start, std::ref(s)));

  for (Sleepy& s : sleepers) {
    s.wake();
  }

  done = true;

  for (Sleepy& s : sleepers) {
    s.wake();
  }

  for (std::thread& t : threads) {
    t.join();
  }

  std::cout << "Hello, World!\n";
  return 0;
}