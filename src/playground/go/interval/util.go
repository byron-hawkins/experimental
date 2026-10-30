package main

import (
	"golang.org/x/exp/constraints"
)

func  min[I constraints.Integer](a I, b I) (I) {
	if (a < b) {
		return a
	} else {
		return b
	}
}

func  max[I constraints.Integer](a I, b I) (I) {
	if (a < b) {
		return b
	} else {
		return a
	}
}

