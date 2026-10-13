#include <algorithm>
#include <iostream>
#include <vector>

class Foo {
 public:
  Foo(unsigned &i) : _i(i) {}

  unsigned i() const { return _i; }

 private:
  unsigned &_i;
};

int main() {
  unsigned i = 0;

  std::vector<Foo> v;
  v.emplace_back(i);
  // std::sort(v.begin(), v.end());  // this fights with the reference member

  std::cout << "• v contains " << v.size() << " elements: " << std::endl;
  for (Foo &f : v) std::cout << "• " << f.i() << std::endl;

  return 0;
}
