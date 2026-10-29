package main

import (
	"fmt"
 "math"
 "math/rand"
 "time"
 "sync"
 "github.com/rishabhofficial/golang-standard-template-library/containers/vector"
 "github.com/rdleal/intervalst/interval"
)

type Item struct {
	index  int
	margin int
	price  int

  in  *Item
	out *vector.Vector[Item]
}

type Matches struct {
	s       []string
  matched bool
}

var wait sync.WaitGroup

func compare (a, b int) int { return a - b }

func foo() {
	i := Item { 0, 8, 3, nil, vector.NewVector[Item]() }
	fmt.Println("i:", i)
}

func main() {
	rand.Seed(time.Now().Unix())

	foo()

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

	tree := interval.NewSearchTree[string,int](compare)
	tree.Insert(3, 5, "foo")
	tree.Insert(4, 8, "bar")
	tree.Insert(6, math.MaxInt, "zot")

	c := make(chan Matches)

	for i := range 3 {
		wait.Add(1)
		go search(c, tree, i + 4)
	}

	for i := range 3 {
		matches := <- c
	  fmt.Println("•  received matches for ", i)
	  if (matches.matched) {
	  	for i := range len(matches.s) {
	  		fmt.Println("  • matched tree value ", matches.s[i])
	  	}
	  } else {
	  	fmt.Println("  • no matches")
	  }
	}

	wait.Wait()
}

func output(index int, i int) {
	defer wait.Done()
	time.Sleep(time.Duration(rand.Intn(1000)) * time.Millisecond)
	fmt.Println(index, ":", i)
}

func NewMatches(matches []string, matched bool) (Matches) {
	return Matches{ matches, matched }
}

func search(c chan Matches, tree *interval.SearchTree[string,int], i int) {
	defer wait.Done()

	fmt.Println("•  search tree for ", i)
	matches := NewMatches(tree.AllIntersections(i, i))
	if (matches.matched) {
		for i := range len(matches.s) {
			fmt.Println("  • matched tree value ", matches.s[i])
		}
	} else {
		fmt.Println("  • no matches")
	}

	c <- matches
}
