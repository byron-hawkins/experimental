package main

import (
	"golang.org/x/exp/constraints"
)

func  max[I constraints.Integer](a I, b I) (I) {
	if (a < b) {
		return b
	} else {
		return a
	}
}

