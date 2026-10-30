package main

import (
	"fmt"
 "math"
 "sync"
 "sync/atomic"
 "github.com/rishabhofficial/golang-standard-template-library/containers/vector"
 "github.com/rdleal/intervalst/interval"
)

type PriceSpan struct {
	base  int   // current selling price
	bound int   // end of the span covered by the enclosing item
}

func bounded(price PriceSpan) (bool) { return price.bound < math.MaxInt32 }

type Item struct {
	index  int32
	margin int
	price  PriceSpan

	best   [3]int

	in  *vector.Vector[Item]
  out *Item
}

var ItemIndex atomic.Int32

func NewItem(margin int, price int) (Item) {
	return Item { ItemIndex.Add(1), margin, PriceSpan { price, math.MaxInt32 }, [3]int{ margin, 0, 0 }, vector.NewVector[Item](), nil }
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
				fmt.Print("• truncate the price watch span for match ", match.price.base)
				if bounded(match.price) {
				  fmt.Print("-", match.price.bound)
				}
				fmt.Println(" at", i.price, "and bind interval")
			}
		} else {
			item := NewItem(i.margin, i.price)
			tree.Insert(item.price.base, item.price.bound, &item)
		}
	}
}

