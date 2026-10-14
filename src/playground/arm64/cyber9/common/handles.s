.text

.global filestream
.global openfile
.global flash

/**** open a file ****/
openfile:
/* call SYS_openat with the configuration in x0 */
  mov   x6,   x0              /* retain the configuration in x6 */
  ldrsw x0,   [x6]            /* x0: fd */
  ldr   x1,   [x6, #0x8]      /* x1: filename */
  ldr   w2,   [x6, #0x10]     /* x3: flags */
  ldr   w3,   [x6, #0x14]     /* x2: mode */
  ldr   w8,   [x6, #0x18]     /* x5: SYS_mmap */
  svc   0x80
  str   w0, [x6]                /* make fd available to the caller */
  cmp   x0, #0
  b.lt openfile_error
  mov   w0, #0
  ret

openfile_error:
  mov w0, #-1
  ret

/**** mmap: flash memory ****/
flash:
/* call SYS_mmap with the configuration in x0 */
  mov x6, x0                /* retain the configuration in x6 */
  mov x0, #0                /* x0: address = 0 */
  ldr x1, [x6]              /* x1: length */
  ldr w2,   [x6, #0x10]     /* x2: prot */
  ldr w3,   [x6, #0x14]     /* x3: flags */
  ldrsw x4, [x6, #0x18]     /* x4: fd = -1 */
  ldr x5,   [x6, #0x20]     /* x5: offset = 0 */
  ldr w8,   [x6, #0x28]     /* x5: SYS_mmap */
  svc 0x80
  str x0, [x6, #8]          /* make the mapping available to the caller */
  cmp x0, #0
  b.le allocate_error
  mov  w0, #0
  ret

allocate_error:
  mov x0, #-1
  ret

/**** simd copy to stream ****/
filestream:
/*
 * x0: output stream handle
 * x1: input stream handle
 */

  ldr  x0, [x0, #8]   /* extract the buffer from the stream */
  ldr  x1, [x1, #8]   /* extract the buffer from the stream */

  /* find stride entry point */
  ands x3, x1, #0xf /* #0xfffffffffffffff0 */
  b.eq stride   /* already aligned */
  neg  x3, x3
  add  x3, x3, #0x20

step:
  /* copy one character */
  ldrb w2, [x1]
  strb W2, [x0]

  /* check null-terminator */
  cmp w2, #0
  b.eq quit

  /* step source and sink */
  add x1, x1, #1
  add x0, x0, #1

  /* check stride entry point */
  subs x3, x3, 1
  b.eq stride

  b step   /* loop */

stride:
  ld1   {v0.16B}, [x1]
  /* copy v0.16B to stream */
  st1   {v0.16B}, [x0]
  add   x1, x1, #0x10
  add   x0, x0, #0x10
  /* check null-terminator */
  uminv b1, v0.16b
  umov  w3, v1.b[0]
  cmp   x3, #0
  b.ne stride   /* loop */

quit:
  ret

