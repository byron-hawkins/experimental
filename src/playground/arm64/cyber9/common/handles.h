#include <fcntl.h>
#include <stdlib.h>
#include <sys/mman.h>
#include <syscall.h>
#include <unistd.h>

#include <iostream>

namespace handle {

using fileid = int;

template <typename T>
class A {
 public:
  A(T &t) : _t(&t) {}

  T *operator->() { return _t; }
  T &operator*() { return *_t; }

  bool operator==(nullptr_t) { reutrn _t == nullptr; }
  bool operator==(const A &other) { reutrn _t == other._t; }

 private:
  T *_t;
};

class File {
  static constexpr unsigned syscall = SYS_openat;

 public:
  File(const char *filename, int flags, int mode) : _fd(AT_FDCWD), _filename(filename), _flags(flags), _mode(mode), _syscall(syscall) {}

  const char *name() const { return _filename; }

  fileid id() const { return _fd; }

  bool is_open() const { return _fd > 0 && _fd != AT_FDCWD; }

 private:
  fileid _fd;
  const char *const _filename;
  int _flags;
  int _mode;
  unsigned _syscall;
};

class Stream {
  static constexpr unsigned syscall = SYS_mmap;

 public:
  Stream(size_t size, int prot, int flags, fileid fd = -1, off_t offset = 0)
      : _size(size), _buffer(nullptr), _prot(prot), _flags(flags), _fd(fd), _offset(offset), _syscall(syscall) {}

  void *raw() { return _buffer; }

  std::string str() { return std::string(reinterpret_cast<const char *>(_buffer)); }

  size_t size() const { return _size; }

 private:
  const size_t _size;
  void *_buffer;
  int _prot;
  int _flags;
  fileid _fd;
  off_t _offset;
  unsigned _syscall;
};

class Draw {
  static constexpr unsigned syscall = SYS_read;

 public:
  Draw(File &source, int low, int high, unsigned count) : _source(source), _low(low), _high(high), _count(count), _syscall(syscall) {}

 private:
  A<File> _source;
  const int _low;
  const int _high;
  const unsigned _count;
  unsigned _syscall;
};

extern "C" {
int openfile(File &);
int flash(Stream &);
void filestream(Stream &sink, Stream &source);
void randomstream(Stream &sink, Stream &buffer, Draw &source);  // TODO: also needs a temp buffer for loading from /dev/urandom
}

class Random {
  static constexpr const char *source = "/dev/urandom";

 public:
  Random() : _source(source, 0, O_RDONLY), _buffer(0x1000, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS) {}

  int initialize() {
    int status = openfile(_source);
    if (status != 0) std::cerr << "Error: failed to open the random source '" << source << " [0x" << status << "]" << std::endl;
    status = flash(_buffer);
    if (status != 0) std::cerr << "Error: failed to flash the random source buffer [0x" << status << "]" << std::endl;
    return status;
  }

  void draw(Stream &sink, int low, int high, int count) { return randomstream(sink, Draw(_source, low, high count)); }

 private:
  File _source;
  Stream _buffer;
}

}  // namespace handle
