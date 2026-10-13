#include <sys/mman.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdlib.h>
#include <syscall.h>
#include <errno.h>

#define MESSAGE "Herzliche Grüße aus Deutschland!"

typedef struct _file_t {
    int fd;
    const char *const filename;
    int flags;
    int mode;
    unsigned syscall;
} file_t;

static void file_configure(file_t *file, int flags, int mode) {
    file->fd = AT_FDCWD;
    file->flags = flags;
    file->mode = mode;
    file->syscall = SYS_openat;
}

typedef struct _stream_t {
    const size_t size;
    void *buffer;
    int prot;
    int flags;
    int fd;
    off_t offset;
    unsigned syscall;
} stream_t;

static void filestream_configure(stream_t *s, int prot, int flags, int fd) {
    s->buffer = NULL;
    s->prot = prot;
    s->flags = flags;
    s->fd = fd;
    s->offset = 0;
    s->syscall = SYS_mmap;
}

static void stream_configure(stream_t *s, int prot, int flags) {
    filestream_configure(s, prot, flags, -1);
}

static stream_t stream = { 0x1000, 0, 0, 0, 0, 0, 0 };

extern int openfile(file_t *);
extern int flash(stream_t *);
extern void filestream(stream_t *sink, stream_t *source);

void setUp(void) {
    stream_configure(&stream, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS);
    int status = flash(&stream);
    if (status != 0)
        fprintf(stderr, "Error: failed to flash the output stream [0x%x]: %s\n", status, strerror(errno));
}

void tearDown(void) {
    munmap(stream.buffer, stream.size);
}

void test_message(void) {
    stream_t fstream = { 0x1000, 0, 0, 0, 0, 0, 0 };
    file_t file = { 0, "./testdata/message.txt", 0, 0, 0 };   // TODO: simplify somehow

    file_configure(&file, 0, O_RDONLY);
    int status = openfile(&file);
    if (status != 0)
        fprintf(stderr, "Error: failed to open the file at '%s' [0x%x]: %s\n", file.filename, status, strerror(errno));

    filestream_configure(&fstream, PROT_READ | PROT_WRITE, MAP_PRIVATE, file.fd);
    status = flash(&fstream);
    if (status != 0)
        fprintf(stderr, "Error: failed to flash the input stream [0x%x]: %s\n", status, strerror(errno));
    filestream(&stream, fstream.buffer);

    if (strncmp(MESSAGE, (const char *) stream.buffer, sizeof(MESSAGE)) != 0) {
        fprintf(stderr, "Error: streamed buffer does not match the original:\n• original: %s\n• streamed %s\n",
                MESSAGE, (const char *) stream.buffer);
    } else {
        printf("• test_message: ok\n");
    }
}

int main(void) {
    setUp();
    test_message();
    tearDown();

    return 0;
}
