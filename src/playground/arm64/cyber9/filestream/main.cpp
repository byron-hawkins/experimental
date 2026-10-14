#include "handles.h"

#define MESSAGE "Herzliche Grüße aus Deutschland!"

using namespace handle;

class Test {
 public:
  Test() : _stream(0x1000, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS) {}

  void setUp(void) {
    int status = flash(_stream);
    if (status != 0) std::cerr << "Error: failed to flash the output stream [0x" << status << "]" << std::endl;
  }

  void tearDown(void) { munmap(_stream.raw(), _stream.size()); }

  void test_message(void) {
    File file("./testdata/message.txt", 0, O_RDONLY);
    int status = openfile(file);
    if (status != 0) std::cerr << "Error: failed to open the file at '" << file.name() << "' [0x" << status << "]" << std::endl;

    Stream fstream(0x1000, PROT_READ | PROT_WRITE, MAP_PRIVATE, file.handle());
    status = flash(fstream);
    if (status != 0) std::cerr << "Error: failed to flash the input stream [0x" << status << "]" << std::endl;
    filestream(_stream, fstream);

    if (_stream.str() == MESSAGE) {
      std::cout << "test_message: ok" << std::endl;
    } else {
      std::cerr << "Error: streamed buffer does not match the original:\n• original: " << MESSAGE << "\n• streamed " << _stream.str()
                << std::endl;
    }
  }

 private:
  Stream _stream;
};

int main(void) {
  Test t;
  t.setUp();
  t.test_message();
  t.tearDown();

  return 0;
}
