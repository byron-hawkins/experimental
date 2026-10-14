CC="clang -Wno-macro-redefined -D__ANDROID_API__=31 --target=aarch64-linux-android30" AS="clang -c" make --trace hello-world-stream |& tee ../logs/make.log
basename="filestream"; git add Makefile ${basename}.s ${basename}_test.c .meta/config.json .meta/tests.toml vendor/*.*
printf 'Herzliche Grüße aus Deutschland!\x00'
clang++ -Wall -Wno-unused-private-field -o test -I./common filestream/main.cpp common/handles.s -g3
