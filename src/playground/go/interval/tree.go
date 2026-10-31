package main

import (
	"fmt"
 "math"
 "sync/atomic"
 "github.com/rishabhofficial/golang-standard-template-library/containers/vector"
 "github.com/rdleal/intervalst/interval"
)

type PriceSpan struct {
	base  int   // current selling price
	bound int   // end of the span covered by the enclosing item
}

func (price PriceSpan) String() (string) {
	if price.Bounded() {
		return fmt.Sprintf("[%v - %v)", price.base, price.bound)
	} else {
		return fmt.Sprintf("[%v -)", price.base)
	}
}

func (price *PriceSpan) Bounded() (bool) { return price.bound < math.MaxInt32 }

type Item struct {
	index  int32
	margin int
	price  PriceSpan

	best   [3]int

	in  *vector.Vector[*Item]
	out *vector.Vector[*Item]
}

var ItemIndex atomic.Int32

func NewItem(margin int, price int) (Item) {
	return Item {
		ItemIndex.Add(1),
		margin,
		PriceSpan { price, math.MaxInt32 },
		[3]int{ margin, 0, 0 },
		vector.NewVector[*Item](),
		vector.NewVector[*Item](),
	}
}

func (item *Item) String() string {
	return fmt.Sprintf("%v @%v <%v x %v>", item.price, item.margin, item.in.Size(), item.out.Size())
}

func (item *Item) AllIn(yield func(item *Item) bool) {
	for i := range item.in.Size() {
		if !yield(item.in.At(i)) {
			return
		}
	}
}

func (from *Item) Link(to *Item) {
	from.out.PushBack(to)
	to.in.PushBack(from)
}

func compare (a, b int) int { return a - b }

type PriceSpanSet struct {
  tree *interval.SearchTree[*Item,int]
}


func (set *PriceSpanSet) Print() {
	fmt.Println("• PriceSpanSet:")
	set.tree.InOrderTraverse(func (base int, bound int, item *Item) error {
		fmt.Println("• ", item)
  	return nil
  })
}

func NewPriceSpanSet() (PriceSpanSet) {
	return PriceSpanSet { interval.NewSearchTree[*Item,int](compare) }
}

func (set *PriceSpanSet) TreeWalk(input []Input) {
	fmt.Println("• walk", len(input), "inputs")

	for _, i := range input {
		item := NewItem(i.margin, i.price)

		set.TreeStep(&item)

		e := set.tree.Insert(item.price.base, item.price.bound, &item)
		if e != nil {
			panic(e)
		}
	}

	set.Print()
}

func (set *PriceSpanSet) TreeStep(item *Item) {
	fmt.Println("• TreeStep ", item)

	matches, matched := set.tree.AllIntersections(item.price.base, item.price.base)
	if matched {
		for _, match := range matches {
			fmt.Println("  • truncate the price interval for match ", match, "at", item.price.base, "and rebind")

			if item.price.base < match.price.bound {
				set.tree.Delete(match.price.base, match.price.bound)
			  match.price.bound = item.price.base
				e := set.tree.Insert(match.price.base, match.price.bound, match)
				if e != nil {
					panic(e)
				}
				match.Link(item)
			} else {
				for predecessor := range match.AllIn {
					predecessor.Link(item)
				}
			}
		}
	}
}
