# Building

The project uses Conan 2 to generate a CMake project and CMake to build it.
During `conan install`, the recipe writes a generated `CMakeLists.txt` into
Conan's build generators directory. It scans `.cpp` files under `src/` for
`main()` definitions, and creates C++17 executables under `bin/` mirroring
their source paths.

If Conan does not have a default profile yet, create one once with:

```sh
conan profile detect
```

```sh
conan install . --output-folder=build --build=missing
conan build . --output-folder=build
```

The generated CMake project is reusable after install. With the default
Release profile, configure it once with:

```sh
cmake -S build/cmake-build/Release/generators \
  -B build/cmake-build/Release \
  -DCMAKE_TOOLCHAIN_FILE=conan_toolchain.cmake \
  -DCMAKE_BUILD_TYPE=Release
```

Then rebuild as often as needed with:

```sh
cmake --build build/cmake-build/Release
```

The CMake `CONFIGURE_DEPENDS` source glob triggers reconfiguration when `.cpp`
files are added or removed, so source changes do not require another
`conan install`. Re-run install when the Conan profile, dependencies, or recipe
settings change.

In VS Code, run **Terminal → Run Task… → Conan: Generate CMake project** for
initial setup. It detects a default profile if needed, installs the Conan
configuration, and configures CMake. After that, use the default build task
**CMake: Build executables** ( **Ctrl+Shift+B** or **Cmd+Shift+B** on macOS)
for repeat builds without rerunning Conan. Run the setup task again if the
Conan profile, dependencies, or recipe settings change.

The project Python files can be syntax-checked locally using **Terminal → Run
Task… → Python: Check syntax**. This uses Python's built-in `py_compile`
module; no MCP service or extra package is required.

For example, `src/playground/cpp/thread/multitask.cpp` is built as
`bin/playground/cpp/thread/multitask` (with the platform's executable suffix,
if applicable).
