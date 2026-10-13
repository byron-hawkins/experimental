#include <condition_variable>
#include <iostream>
#include <thread>

class Gemini {
  class Twin {
  public:
    Twin(Gemini &g, unsigned id) : _g(g), _id(id), _waiting(false) {
      std::cout << "• construct twin " << _id << ": waiting? " << std::boolalpha
                << _waiting << std::flush << std::endl;
    }

    unsigned id() const { return _id; }

    void start() {
      std::unique_lock<std::mutex> access(_x);

      std::cout << "• twin " << _id << " starts and waits" << std::flush
                << std::endl;

      _waiting = true;
      _g._pilot.notify_one();
      if (_g._first == 0)
        _g._first = _id;
      _g._twin.wait(access, [&waiting = _waiting]() { return !waiting; });

      std::cout << "• twin " << _id << " finishes" << std::flush << std::endl;

      _g._pilot.notify_one();
    }

    void confirm() {
      std::unique_lock<std::mutex> access(_x);

      std::cout << "• has twin " << _id << " started? " << std::boolalpha
                << _waiting << std::flush << std::endl;

      if (!_waiting)
        _g._pilot.wait(access, [&waiting = _waiting]() { return waiting; });

      std::cout << "• confirmed: twin " << _id << " has started: (" << _waiting
                << ")" << std::flush << std::endl;
    }

    void finish() {
      std::unique_lock<std::mutex> access(_x);

      std::cout << "• wake twin " << _id << " to finish" << std::flush
                << std::endl;

      _waiting = false;
      _g._twin.notify_one();
    }

  private:
    Gemini &_g;
    unsigned _id;
    std::mutex _x;
    bool _waiting;
  };

public:
  Gemini() : _a(*this, 1), _b(*this, 2), _first(0) {}

  // • wait for both twins to start
  // • find which one waited first
  // • notify first on the other mutex

  void run() {
    std::cout << "• gemini starts two twins" << std::flush << std::endl;

    std::thread at(&Twin::start, std::ref(_a));
    std::thread bt(&Twin::start, std::ref(_b));

    _a.confirm();
    _b.confirm();

    if (_first == _a.id()) {
      _b.finish();
      _a.finish();
    } else {
      _a.finish();
      _b.finish();
    }

    at.join();
    bt.join();

    std::cout << "• exit" << std::flush << std::endl;
  }

private:
  std::condition_variable _twin;
  std::condition_variable _pilot;

  Twin _a;
  Twin _b;

  unsigned _first;
};

int main() {
  Gemini g;
  g.run();

  return 0;
}
