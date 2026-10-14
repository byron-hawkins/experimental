#include <math.h>

#include <iomanip>
#include <iostream>

int main() {
  std::cout << std::fixed << std::setprecision(3);

  for (unsigned i = 1; i < 40; i++) {
    double d = sqrt(static_cast<double>(2 * i * i));
    std::cout << "• " << i << ") " << d << " [" << (2 * i) << "]" << std::endl;
  }

  return 0;
}
