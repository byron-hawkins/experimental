#include <array>
#include <condition_variable>
#include <iostream>
#include <mutex>
#include <thread>
#include <vector>

enum class Go { pilot, sink };

class FourStamps {
  class Sink {
   public:
    Sink(FourStamps& stamps) : _go(Go::sink), _stamps(stamps) {}

    void start() {
      std::unique_lock<std::mutex> access(_stamps._m);

      while (true) {
        _go = Go::pilot;
        _c.notify_one();
        _c.wait(access, [&who = _go]() { return who == Go::sink; });

        if (_stamps._i == _stamps._count) break;

        std::cout << _stamps._i << std::endl;
      }

      _go = Go::pilot;
      _c.notify_one();
    }

    void pump(const std::string& label = "") {
      std::unique_lock<std::mutex> access(_stamps._m);

      std::cout << label << ": ";

      if (_go != Go::pilot)
        _c.wait(access, [&who = _go]() { return who == Go::pilot; });

      _go = Go::sink;
      _c.notify_one();
      _c.wait(access, [&who = _go]() { return who == Go::pilot; });
    }

   private:
    Go _go;

    FourStamps& _stamps;
    std::condition_variable _c;
  };

 public:
  FourStamps(unsigned count)
      : _count(count),
        _i(0),
        _by3(0),
        _by5(0),
        _sinks{*this, *this, *this, *this} {}

  void print() {
    std::vector<std::thread> threads;
    for (Sink& sink : _sinks)
      threads.push_back(std::thread(&Sink::start, std::ref(sink)));

    for (; _i < _count; _i++) {
      if (_by3 == 0) {
        _by3 = 3;
        if (_by5 == 0) {
          _by5 = 5;
          _sinks.at(0).pump("3x5");
        } else {
          _sinks.at(1).pump("3");
        }
      } else {
        if (_by5 == 0) {
          _by5 = 5;
          _sinks.at(2).pump("5");
        } else {
          _sinks.at(3).pump("-");
        }
      }
      _by3--;
      _by5--;
    }

    for (Sink& sink : _sinks) sink.pump();
    for (std::thread& thread : threads) thread.join();
  }

 private:
  unsigned _count;
  unsigned _i;
  unsigned _by3;
  unsigned _by5;

  std::mutex _m;
  std::array<Sink, 4> _sinks;
};

int main() {
  FourStamps stamps(10000);
  stamps.print();

  return 0;
}