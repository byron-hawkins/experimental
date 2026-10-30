package main

import (
	"fmt"
 "math"
 "sync"
 "sync/atomic"
 "github.com/rishabhofficial/golang-standard-template-library/containers/vector"
 "github.com/rdleal/intervalst/interval"
)

type Item struct {
	index  int32
	margin int
	price  int

	best   [3]int

	in  *vector.Vector[Item]
  out *Item
}

var ItemIndex atomic.Int32

func NewItem(margin int, price int) (Item) {
	return Item { ItemIndex.Add(1), margin, price, [3]int{ margin, 0, 0 }, vector.NewVector[Item](), nil }
}

type Matches struct {
	s       []string
  matched bool
}

func NewMatches(matches []string, matched bool) (Matches) {
	return Matches{ matches, matched }
}

var wait sync.WaitGroup

func compare (a, b int) int { return a - b }

func foo() {
	i := NewItem(4, 5)
	fmt.Println("i:", i)
}

func treetest() {
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
