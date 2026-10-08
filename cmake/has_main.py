import re
import sys
from pathlib import Path


def remove_comments_and_literals(source):
    output = []
    index = 0

    while index < len(source):
        raw_string = re.match(
            r'(?:u8|u|U|L)?R"([^ ()\\\t\r\n]*)\(', source[index:]
        )
        if raw_string:
            delimiter = raw_string.group(1)
            end_marker = ")" + delimiter + '"'
            end = source.find(end_marker, index + raw_string.end())
            if end == -1:
                raise ValueError("unterminated raw string literal")
            output.append(" ")
            index = end + len(end_marker)
            continue

        if source.startswith("//", index):
            end = source.find("\n", index)
            index = len(source) if end == -1 else end
            output.append(" ")
        elif source.startswith("/*", index):
            end = source.find("*/", index + 2)
            if end == -1:
                raise ValueError("unterminated block comment")
            output.append(" ")
            index = end + 2
        elif source[index] in ('"', "'"):
            quote = source[index]
            index += 1
            while index < len(source):
                if source[index] == "\\":
                    index += 2
                elif source[index] == quote:
                    index += 1
                    break
                else:
                    index += 1
            else:
                raise ValueError("unterminated string or character literal")
            output.append(" ")
        else:
            output.append(source[index])
            index += 1

    return "".join(output)


def has_main_definition(source):
    code = remove_comments_and_literals(source)
    for match in re.finditer(r"\bmain\s*\(", code):
        depth = 1
        index = match.end()
        while index < len(code) and depth:
            if code[index] == "(":
                depth += 1
            elif code[index] == ")":
                depth -= 1
            index += 1

        if depth:
            continue

        suffix = code[index:]
        suffix = re.sub(r"^\s*noexcept(?:\s*\([^)]*\))?", "", suffix)
        suffix = re.sub(r"^\s*(?:\[\[.*?\]\]\s*)*", "", suffix, flags=re.DOTALL)
        if re.match(r"\s*\{", suffix):
            return True

    return False


def main():
    if len(sys.argv) != 2:
        print("usage: has_main.py <source.cpp>", file=sys.stderr)
        return 2

    try:
        source = Path(sys.argv[1]).read_text(encoding="utf-8")
        return 0 if has_main_definition(source) else 1
    except (OSError, UnicodeError, ValueError) as error:
        print(f"could not inspect {sys.argv[1]}: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
