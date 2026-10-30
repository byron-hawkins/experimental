package main

import (
 "time"
	"math/rand"
)

func init() {
	rand.Seed(time.Now().Unix())
}

type Input struct {
	margin int
	price  int
}

func generate(count int) ([]Input) {
	input := make([]Input, count)
	for i := range len(input) {
		input[i] = Input { rand.Intn(1000), rand.Intn(1000) }
	}
	return input
}


