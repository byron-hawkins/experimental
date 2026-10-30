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
	s       []Item
  matched bool
}

func NewMatches(matches []Item, matched bool) (Matches) {
	return Matches{ matches, matched }
}

var wait sync.WaitGroup

func compare (a, b int) int { return a - b }

func foo() {
	i := NewItem(4, 5)
	fmt.Println("i:", i)
}

func treewalk(input []Input) {
	fmt.Println("• walk", len(input), "inputs")

	tree := interval.NewSearchTree[*Item,int](compare)

	for _, i := range input {
		matches, matched := tree.AllIntersections(i.price, i.price)
		if matched {
			for _, match := range matches {
				fmt.Println("• truncate match ", match, "and bind interval")
			}
		} else {
			item := NewItem(i.margin, i.price)
			tree.Insert(i.price, math.MaxInt32, &item)
		}
	}
}

