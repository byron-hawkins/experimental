#include <iostream>

int main(int argc, char* argv[]) {
  std::cin.tie(0);
  std::cout.tie(0);
  std::ios_base::sync_with_stdio(0);

  int tc;
  std::cin >> tc;
  while (tc--) {
    unsigned n, k;
    std::cin >> n >> k;
    std::cout << (2 * (k - 1)) + (1 << (n - k + 1)) << std::endl;
  }

  return 0;
}