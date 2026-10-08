from pathlib import Path

from conan import ConanFile
from conan.tools.cmake import CMake, CMakeToolchain, cmake_layout


def cmake_bracket_argument(value):
    for equals_count in range(10):
        equals = "=" * equals_count
        if f"]{equals}]" not in value:
            return f"[{equals}[{value}]{equals}]"
    raise ValueError("source folder cannot be represented as a CMake argument")


class ExperimentalConan(ConanFile):
    name = "experimental"
    version = "0.1"
    settings = "os", "arch", "compiler", "build_type"

    def layout(self):
        cmake_layout(self, build_folder="cmake-build")

    def generate(self):
        CMakeToolchain(self).generate()
        source_root = cmake_bracket_argument(str(Path(self.source_folder).resolve()))
        cmake_lists = f"""cmake_minimum_required(VERSION 3.20)
project(experimental LANGUAGES CXX)

set(EXPERIMENTAL_SOURCE_ROOT {source_root})
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

find_package(Python3 REQUIRED COMPONENTS Interpreter)
find_package(Threads REQUIRED)
file(GLOB_RECURSE CPP_SOURCES CONFIGURE_DEPENDS
     "${{EXPERIMENTAL_SOURCE_ROOT}}/src/*.cpp")
list(SORT CPP_SOURCES)

set(EXECUTABLE_INDEX 0)
foreach(SOURCE IN LISTS CPP_SOURCES)
  execute_process(
    COMMAND "${{Python3_EXECUTABLE}}"
            "${{EXPERIMENTAL_SOURCE_ROOT}}/cmake/has_main.py" "${{SOURCE}}"
    RESULT_VARIABLE HAS_MAIN
    OUTPUT_QUIET
    ERROR_VARIABLE MAIN_CHECK_ERROR)

  if("${{HAS_MAIN}}" STREQUAL "0")
    math(EXPR EXECUTABLE_INDEX "${{EXECUTABLE_INDEX}} + 1")
    file(RELATIVE_PATH SOURCE_RELATIVE
         "${{EXPERIMENTAL_SOURCE_ROOT}}/src" "${{SOURCE}}")
    get_filename_component(SOURCE_DIRECTORY "${{SOURCE_RELATIVE}}" DIRECTORY)
    get_filename_component(SOURCE_NAME "${{SOURCE_RELATIVE}}" NAME_WE)

    add_executable(experimental_executable_${{EXECUTABLE_INDEX}} "${{SOURCE}}")
    target_link_libraries(experimental_executable_${{EXECUTABLE_INDEX}}
                          PRIVATE Threads::Threads)
    set_target_properties(
      experimental_executable_${{EXECUTABLE_INDEX}}
      PROPERTIES
        OUTPUT_NAME "${{SOURCE_NAME}}"
        RUNTIME_OUTPUT_DIRECTORY
          "${{EXPERIMENTAL_SOURCE_ROOT}}/bin/${{SOURCE_DIRECTORY}}")

    foreach(CONFIGURATION DEBUG RELEASE RELWITHDEBINFO MINSIZEREL)
      set_target_properties(
        experimental_executable_${{EXECUTABLE_INDEX}}
        PROPERTIES
          RUNTIME_OUTPUT_DIRECTORY_${{CONFIGURATION}}
            "${{EXPERIMENTAL_SOURCE_ROOT}}/bin/${{SOURCE_DIRECTORY}}")
    endforeach()
  elseif(NOT "${{HAS_MAIN}}" STREQUAL "1")
    message(FATAL_ERROR "Failed to inspect ${{SOURCE}}: ${{MAIN_CHECK_ERROR}}")
  endif()
endforeach()
"""
        generated_cmake = Path(self.generators_folder) / "CMakeLists.txt"
        generated_cmake.write_text(cmake_lists, encoding="utf-8")

    def build(self):
        cmake = CMake(self)
        cmake.configure(build_script_folder=self.generators_folder)
        cmake.build()
