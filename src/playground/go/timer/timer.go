package main

import (
	"fmt"
 "math/rand"
 "time"
 "sync"
)

var wait sync.WaitGroup

func compare (a, b int) int { return a - b }

func main() {
	rand.Seed(time.Now().Unix())

	n := make([]int, rand.Intn(8) + 2)
	for i := range len(n) {
		n[i] = rand.Intn(100)
	}
	for i := range len(n) {
		fmt.Println(i, ":", n[i])
	}

	fmt.Println("  ----")

	for index, i := range n {
		wait.Add(1)
		go output(index, i)
	}

	wait.Wait()
}

func output(index int, i int) {
	defer wait.Done()
	time.Sleep(time.Duration(rand.Intn(1000)) * time.Millisecond)
	fmt.Println(index, ":", i)
}

