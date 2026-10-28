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
  // TODO: possible to object-initialize instead?
  public Swap(string s, int one, int two) {
    _self = s;
    _place1 = one;
    _place2 = two;
  }

  public string Get() { return _self; }

  public Swap? FindSwap(string s) {
    int inside, outside;
    if (_self[_place1] != s[_place1]) {
      if (_self[_place2] != s[_place2]) return null;  // if swappable, already found
      outside = s.IndexOf(_self[_place1]);
      if (_self[outside] != s[_place1]) return null;  // not swappable: 3+ rotation
      inside = _place1;
    } else if (_self[_place2] != s[_place2]) {
      outside = s.IndexOf(_self[_place2]);
      if (_self[outside] != s[_place2]) return null;  // if swappable, already found
      inside = _place2;
    } else {
      return null;  // if swappable, already found
    }

    for (int i = 0; i < s.Length; i++) {
      if (i == inside || i == outside) continue;
      if (_self[i] != s[i]) return null;
    }
    return new Swap(s, inside, outside);
  }

  private string _self;
  private int _place1;  // places are not ordered
  private int _place2;
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

class Delta {
  public Delta() {
    _pair = new int[2];
    _s = "";
  }

  public Swap? MakeSwap() {
    if (_index == 0) return null;
    return new Swap(_s, _pair[0], _pair[1]);
  }

  public bool FindSwap(string a, string b) {
    _s = b;
    _index = 0;
    for (int i = 0; i < a.Length; i++) {
      if (a[i] != b[i]) {
        if (!Add(i)) return false;
      }
    }

    switch (_index) {
      case 0:
      case 2: return true;
      default: return false;
    }
  }

  private bool Add(int i) {
    if (_index < 2) {
      _pair[_index++] = i;
      return true;
    }
    return false;
  }

  private int _index;
  private int[] _pair;
  private string _s;
}

class Anagram {
  class Removal : IDisposable {
    public Removal(LinkedList<string> q) {
      _q = q;
      _n = null;
    }

    public void Load(LinkedListNode<string> n) { _n = n; }
    public void Dispose() {
      if (_n != null) _q.Remove(_n);
    }

    private LinkedList<string> _q;
    private LinkedListNode<string>? _n;
  }

  public Anagram() {
    _g = new();
    _q = new();
  }

  public int Count() { return _q.Count(); }
  public void Add(string s) { _q.AddLast(s); }

  public IEnumerable<Group> groups() { return _g; }

  public void decompose() {
    Delta delta = new();
    List<Swap> swaps = new();
    while (_q.First is LinkedListNode<string> top) {
      Group g = new(top.Value);
      _g.Add(g);
      if (top.Next is LinkedListNode<string> walk) {
        _q.RemoveFirst();
        while (true) {
          using (Removal remove = new(_q)) {
            if (delta.FindSwap(top.Value, walk.Value)) {
              g.Add(walk.Value);
              remove.Load(walk);
              if (delta.MakeSwap() is Swap member) swaps.Add(member);
            }
            if (walk.Next is LinkedListNode<string> step) walk = step;
            else
              break;
          }
        }

        for (int i = 0; i < swaps.Count(); i++) {
          if (_q.First is null) break;
          walk = _q.First;
          while (true) {
            using (Removal remove = new(_q)) {
              Swap? swap = swaps[i].FindSwap(walk.Value);
              if (swap is Swap member) {
                g.Add(member.Get());
                swaps.Add(member);
                remove.Load(walk);
              }
              if (walk.Next is LinkedListNode<string> step) walk = step;
              else
                break;
            }
          }
        }
        swaps.Clear();
      } else {
        break;  // singleton group already added
      }
    }
  }

  private List<Group> _g;
  private LinkedList<string> _q;
}
