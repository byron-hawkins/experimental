using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;

class StringRadix {
  public StringRadix(string s) {
    _buckets = new uint[size];
    foreach (char c in s) _buckets[(c - a)]++;

    _hash = hash();
  }

  public static bool operator<(StringRadix left, StringRadix right) {
    for (uint i = 0; i < size; i++) {
      if (left._buckets[i] < right._buckets[i]) return true;
      if (left._buckets[i] > right._buckets[i]) return false;
    }
    return false;
  }

  public static bool operator>(StringRadix left, StringRadix right) {
    for (uint i = 0; i < size; i++) {
      if (left._buckets[i] > right._buckets[i]) return true;
      if (left._buckets[i] < right._buckets[i]) return false;
    }
    return false;
  }

  public static bool operator ==(StringRadix left, StringRadix right) {
    for (uint i = 0; i < size; i++)
      if (left._buckets[i] != right._buckets[i]) return false;
    return true;
  }

  public static bool operator !=(StringRadix left, StringRadix right) {
    for (uint i = 0; i < size; i++)
      if (left._buckets[i] != right._buckets[i]) return true;
    return false;
  }

  public override bool Equals(object? o) { return (o is StringRadix r) ? (this == r) : false; }
  public override int GetHashCode() { return _hash; }
  public override string ToString() {
    OutputData.Comma comma = new();
    StringBuilder s = new();
    s.Append("[");
    int c = a;
    foreach (uint i in _buckets) {
      c++;
      if (i == 0) continue;
      s.Append(comma);
      if (i > 1) s.Append(i);
      s.Append((char)c);
    }
    s.Append("]");
    return s.ToString();
  }

  private static readonly int size = 26;
  private static readonly int a = (int)'a';

  private int hash() {
    void shift(ref int h, int i) { h = (((h << 5) + h) ^ i); }

    int h = 0;
    int zeros = 0;
    foreach (uint count in _buckets) {
      if (count == 0) {
        zeros++;
        continue;
      }
      if (zeros > 0) {
        shift(ref h, zeros);
        zeros = 0;
      }
      shift(ref h, (int)count);
    }
    if (zeros > 0) shift(ref h, zeros);
    return h;
  }

  private readonly int _hash;
  private readonly uint[] _buckets;
}

struct Swap {
  public string root;
  public string self;
  public int place1;  // places are not ordered
  public int place2;

  // TODO: possible to object-initialize instead?
  public Swap(string a, string b, int one, int two) {
    root = a;
    self = b;
    place1 = one;
    place2 = two;
  }
}

class Group {
  public Group(string root) {
    _root = root;
    _members = new();
    _members.Add(_root);
  }

  public string root() { return _root; }
  public int Count() { return _members.Count(); }
  public void Add(string s) { _members.Add(s); }

  public IEnumerable<string> members() { return _members; }

  public void print() {
    Console.WriteLine($"  • group '{_root}' with {_members.Count()} members:");
    foreach (string s in _members) Console.WriteLine($"    • {s}");
  }

  private string _root;
  private HashSet<string> _members;
}

class Anagram {
  public Anagram() {
    _g = new();
    _q = new();
  }

  public int Count() { return _q.Count(); }
  public void Add(string s) { _q.AddLast(s); }

  public IEnumerable<Group> groups() { return _g; }

  public void decompose() {
    bool stepped = false;
    List<Swap> swaps = new();
    while (_q.First is LinkedListNode<string> top) {
      Group g = new(top.Value);
      _g.Add(g);
      if (top.Next is LinkedListNode<string> walk) {
        LinkedListNode<string>? remove = null;
        _q.RemoveFirst();
        do {
          stepped = false;
          Swap? swap = findSwap(top.Value, walk.Value);
          if (swap is Swap member) {
            g.Add(member.self);
            swaps.Add(member);
            remove = walk;
          }
          if (walk.Next is LinkedListNode<string> step) {
            walk = step;
            stepped = true;
          }
          if (remove is LinkedListNode<string>) {
            _q.Remove(remove);
            remove = null;
          }
        } while (stepped);

        // drain swaps against _q
        for (int i = 0; i < swaps.Count(); i++) {
          if (_q.First is null) break;
          walk = _q.First;
          do {
            stepped = false;
            Swap? swap = findSwap(swaps[i], walk.Value);
            if (swap is Swap member) {
              g.Add(member.self);
              swaps.Add(member);
              remove = walk;
            }
            if (walk.Next is LinkedListNode<string> step) {
              walk = step;
              stepped = true;
            }
            // TODO: if removal can be done with a dispose, can simply break after failed step
            if (remove is LinkedListNode<string>) {
              _q.Remove(remove);
              remove = null;
            }
          } while (stepped);
        }
        swaps.Clear();
      } else {
        break;  // singleton group already added
      }
    }
  }

  private Swap? findSwap(string root, string s) {
    List<int> delta = new();
    for (int i = 0; i < root.Length; i++) {
      if (root[i] != s[i]) {
        if (delta.Count() == 2) return null;
        delta.Add(i);
      }
    }
    if (delta.Count() != 2) return null;
    return new Swap(root, s, delta[0], delta[1]);
  }

  private Swap? findSwap(Swap member, string s) {
    int inside, outside;
    if (member.self[member.place1] != s[member.place1]) {
      if (member.self[member.place2] != s[member.place2]) return null;  // if swappable, already found
      outside = s.IndexOf(member.self[member.place1]);
      if (member.self[outside] != s[member.place1]) return null;  // not swappable: 3+ rotation
      inside = member.place1;
    } else if (member.self[member.place2] != s[member.place2]) {
      outside = s.IndexOf(member.self[member.place2]);
      if (member.self[outside] != s[member.place2]) return null;  // if swappable, already found
      inside = member.place2;
    } else {
      return null;  // if swappable, already found
    }

    for (int i = 0; i < s.Length; i++) {
      if (i == inside || i == outside) continue;
      if (member.self[i] != s[i]) return null;
    }
    return new Swap(member.self, s, inside, outside);
  }

  private List<Group> _g;
  private LinkedList<string> _q;
}
