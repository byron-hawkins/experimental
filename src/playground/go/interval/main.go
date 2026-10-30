package main

import (
	"os"
	"fmt"
	"strconv"
)

func main() {
	if len(os.Args) != 2 {
		fmt.Println("• Missing argument: <item-count>")
		os.Exit(1)
	}
	itemcount, fail := strconv.Atoi(os.Args[1])
	if fail != nil {
		fmt.Println("• Error: failed to parse", os.Args[1], "as an integer:", fail)
		os.Exit(1)
	}

	input := generate(itemcount)
	for i := range min(len(input), 3) {
		fmt.Println(i, ":", input[i])
	}

	treewalk(input)
	// treetest()
}
