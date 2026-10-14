#include <fcntl.h>
#include <stdlib.h>
#include <sys/mman.h>
#include <syscall.h>
#include <unistd.h>

#include <iostream>

namespace handle {

class File {
  static constexpr unsigned syscall = SYS_openat;

 public:
  File(const char *filename, int flags, int mode) : _fd(AT_FDCWD), _filename(filename), _flags(flags), _mode(mode), _syscall(syscall) {}

  const char *name() const { return _filename; }

  int handle() const { return _fd; }

  bool is_open() const { return _fd > 0 && _fd != AT_FDCWD; }

 private:
  int _fd;
  const char *const _filename;
  int _flags;
  int _mode;
  unsigned _syscall;
};

class Stream {
  static constexpr unsigned syscall = SYS_mmap;

 public:
  Stream(size_t size, int prot, int flags, int fd = -1, off_t offset = 0)
      : _size(size), _buffer(nullptr), _prot(prot), _flags(flags), _fd(fd), _offset(offset), _syscall(syscall) {}

  void *raw() { return _buffer; }

  std::string str() { return std::string(reinterpret_cast<const char *>(_buffer)); }

  size_t size() const { return _size; }

 private:
  const size_t _size;
  void *_buffer;
  int _prot;
  int _flags;
  int _fd;
  off_t _offset;
  unsigned _syscall;
};

extern "C" {
int openfile(File &);
int flash(Stream &);
void filestream(Stream &sink, Stream &source);
}

}  // namespace handle
