#include <fcntl.h>
#include <stdlib.h>
#include <sys/mman.h>
#include <syscall.h>
#include <unistd.h>

#include <iostream>

namespace handle {

class File {
 public:
  File(const char *filename, int flags, int mode) : _fd(AT_FDCWD), _filename(filename), _flags(flags), _mode(mode) {}

  const char *name() const { return _filename; }

  int handle() const { return _fd; }

  bool is_open() const { return _fd > 0 && _fd != AT_FDCWD; }

 private:
  static constexpr unsigned _syscall = SYS_openat;

  int _fd;
  const char *const _filename;
  int _flags;
  int _mode;
};

class Stream {
 public:
  Stream(size_t size, int prot, int flags, int fd = -1, off_t offset = 0)
      : _size(size), _buffer(nullptr), _prot(prot), _flags(flags), _fd(fd), _offset(offset) {}

  void *raw() { return _buffer; }

  std::string str() { return std::string(reinterpret_cast<const char *>(_buffer)); }

  size_t size() const { return _size; }

 private:
  static constexpr unsigned _syscall = SYS_mmap;

  const size_t _size;
  void *_buffer;
  int _prot;
  int _flags;
  int _fd;
  off_t _offset;
};

extern "C" {
int openfile(File &);
int flash(Stream &);
void filestream(Stream &sink, Stream &source);
}

}  // namespace handle
