using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CSharpMath.Structures;

/// <summary>
/// Funnels collection initializers to <see cref="Added"/>.
/// Implements <see cref="IEnumerable"/> but throws upon enumeration because it is there only to enable collection initializers.
/// </summary>
/// <example>
/// <code>new ProxyAdder&lt;int, string&gt;((key, value) => Console.WriteLine(value))
/// { { 1, 2, 3, "1 to 3" }, { Enumerable.Range(7, 10), i => i.ToString() } }</code>
/// </example>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = NotACollection)]
[SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = NotACollection)]
public class ProxyAdder<TKey, TValue> : IEnumerable {
    private const string NotACollection = "This is not a collection. It implements IEnumerable just to support collection initializers.";
    [Obsolete(NotACollection, true)]
    [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = NotACollection)]
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException(NotACollection);

    protected ProxyAdder(Action<TKey, TValue>? extraCommandToPerformWhenAdding = null) => Added += extraCommandToPerformWhenAdding;
    public event Action<TKey, TValue>? Added;
    public void Add(TKey key1, TValue value) => Added?.Invoke(key1, value);
    public void Add(TKey key1, TKey key2, TValue value) {
        Add(key1, value); Add(key2, value);
    }
    public void Add(TKey key1, TKey key2, TKey key3, TValue value) {
        Add(key1, value); Add(key2, value); Add(key3, value);
    }
    public void Add(TKey key1, TKey key2, TKey key3, TKey key4, TValue value) {
        Add(key1, value); Add(key2, value); Add(key3, value); Add(key4, value);
    }
    public void Add<TCollection>(TCollection keys, TValue value) where TCollection : IEnumerable<TKey> {
        foreach (var key in keys) Add(key, value);
    }
    public void Add<TCollection>(TCollection keys, Func<TKey, TValue> valueFunc) where TCollection : IEnumerable<TKey> {
        foreach (var key in keys) Add(key, valueFunc(key));
    }
}
/// <summary>Ensures that longer <see cref="string"/>s with same beginnings are listed first, to be matched first.</summary>
class DescendingStringComparer<TValue> : IComparer<(string NonCommand, TValue Value)> {
    public int Compare((string NonCommand, TValue Value) x, (string NonCommand, TValue Value) y) =>
        string.CompareOrdinal(y.NonCommand, x.NonCommand);
}

/// <summary>
///  A dictionary-based helper where the keys are classes of LaTeX <see cref="string"/>s, with special treatment
///  for commands (starting with "\"). The start of an inputted <see cref="Span{T}"/> with type argument
///  <see cref="char"/> is parsed, and an arbitrary object <typeparamref name="TValue"/> is returned,
///  along with the number of matching characters. Processing is based on dictionary lookup with fallback
///  to specified default functions for command and non-commands when lookup fails.
///  For non-commands, dictionary lookup finds the longest matching non-command.
/// </summary>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix",
    Justification = "This is conceptually a dictionary but has different lookup behavior")]
public class LaTeXCommandDictionary<TValue> : ProxyAdder<string, TValue>, IEnumerable<KeyValuePair<string, TValue>> {
    public delegate Result<(TValue Result, int SplitIndex)> DefaultDelegate(ReadOnlySpan<char> consume);

    public LaTeXCommandDictionary(DefaultDelegate defaultParser,
        DefaultDelegate defaultParserForCommands, Action<string, TValue>? extraCommandToPerformWhenAdding = null) : base(extraCommandToPerformWhenAdding) {
        this._defaultParser = defaultParser;
        this._defaultParserForCommands = defaultParserForCommands;
        Added += (key, value) => {
            if (key.AsSpan().StartsWithInvariant(@"\"))
                if (SplitCommand(key.AsSpan()) != key.Length - 1)
                    _commands.Add(key, value);
                else throw new ArgumentException("Key is unreachable: " + key, nameof(key));
            else _nonCommands.Add((key, value));
        };
    }

    private readonly DefaultDelegate _defaultParser;
    private readonly DefaultDelegate _defaultParserForCommands;

    private readonly SortedSet<(string NonCommand, TValue Value)> _nonCommands = new(new DescendingStringComparer<TValue>());
    private readonly Dictionary<string, TValue> _commands = new();

    public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() =>
        _nonCommands.Select(t => new KeyValuePair<string, TValue>(t.NonCommand, t.Value))
            .Concat(_commands).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Finds the number of characters corresponding to a LaTeX command at the beginning of <see cref="char"/>s.</summary>
    static int SplitCommand(ReadOnlySpan<char> chars) {
        System.Diagnostics.Debug.Assert(chars[0] == '\\');
        var splitIndex = 1;
        if (splitIndex >= chars.Length) return splitIndex;
        if (IsEnglishAlphabetOrAt(chars[splitIndex])) {
            do splitIndex++; while (splitIndex < chars.Length && IsEnglishAlphabetOrAt(chars[splitIndex]));
            if (splitIndex >= chars.Length) return splitIndex;
            switch (chars[splitIndex]) {
                case '*':
                case '=':
                case '\'':
                    splitIndex++;
                    break;
            }
        } else splitIndex++;
        return splitIndex;

        // Note on '@': https://stackoverflow.com/questions/29217603/extracting-all-latex-commands-from-a-latex-code-file#comment47075515_29218404
        static bool IsEnglishAlphabetOrAt(char c) => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '@';
    }
    /// <summary>Tries to find a command at the beginning of <see cref="char"/>s, returning the
    /// <typeparamref name="TValue"/> corresponding to the command Key, and the length of the command.</summary>
    public Result<(TValue Result, int SplitIndex)> TryLookup(ReadOnlySpan<char> chars) {
        Result<(TValue Result, int SplitIndex)> TryLookupCommand(ReadOnlySpan<char> characters) {
            var splitIndex = SplitCommand(characters);
            var lookup = characters.Slice(0, splitIndex);
            while (splitIndex < characters.Length && char.IsWhiteSpace(characters[splitIndex]))
                splitIndex++;
            return _commands.TryGetValue(lookup.ToString(), out var result)
                ? Result.Ok((result, splitIndex))
                : _defaultParserForCommands(lookup);
        }
        Result<(TValue Result, int SplitIndex)> TryLookupNonCommand(ReadOnlySpan<char> characters) {
            foreach (var (nonCommand, value) in _nonCommands) {
                if (characters.StartsWith(nonCommand.AsSpan(), StringComparison.Ordinal)) {
                    return Result.Ok((value, nonCommand.Length));
                }
            }
            return _defaultParser(characters);
        }

        if (chars.IsEmpty) throw new ArgumentException("There are no characters to read.", nameof(chars));
        return chars.StartsWithInvariant(@"\") ? TryLookupCommand(chars) : TryLookupNonCommand(chars);
    }
}

// Taken from https://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary/255638#255638
/// <summary>
/// Represents a many to one relationship between <typeparamref name="TFirst"/>s and <typeparamref name="TSecond"/>s,
/// allowing fast lookup of the first <typeparamref name="TFirst"/> corresponding to any <typeparamref name="TSecond"/>,
/// in addition to the usual lookup of a <typeparamref name="TSecond"/>s by a <typeparamref name="TFirst"/>.
/// </summary>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = IDictionaryNoLongerImplemented)]
[SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = IDictionaryNoLongerImplemented)]
public class AliasBiDictionary<TFirst, TSecond> : ProxyAdder<TFirst, TSecond> where TFirst : notnull where TSecond : notnull {
    const string IDictionaryNoLongerImplemented = "This is two dictionaries in one so a single IReadOnlyDictionary interface isn't appropriate. Instead both are provided.";
    public AliasBiDictionary(Action<TFirst, TSecond>? extraCommandToPerformWhenAdding = null) : base(extraCommandToPerformWhenAdding) =>
        Added += (first, second) => {
            switch (_firstToSecond.ContainsKey(first), _secondToFirst.ContainsKey(second)) {
                case (true, _):
                    // There cannot be multiple TSeconds linked to the same TFirst
                    throw new Exception($"Key already exists in {nameof(AliasBiDictionary<TFirst, TSecond>)}.");
                case (false, true):
                    _firstToSecond.Add(first, second);
                    break;
                case (false, false):
                    _firstToSecond.Add(first, second);
                    _secondToFirst.Add(second, first);
                    break;
            }
        };

    private readonly Dictionary<TFirst, TSecond> _firstToSecond = new();
    private readonly Dictionary<TSecond, TFirst> _secondToFirst = new();
    public Dictionary<TFirst, TSecond>.Enumerator GetEnumerator() =>
        _firstToSecond.GetEnumerator();
    public bool RemoveByFirst(TFirst first) {
        bool exists = _firstToSecond.TryGetValue(first, out var svalue);
        if (!exists) return exists;
        _firstToSecond.Remove(first);
        // if first is currently mapped to from svalue,
        // then try to reconnect svalue to another TFirst mapping to it;
        // otherwise delete the svalue record in secondToFirst
        if (!_secondToFirst[svalue].Equals(first)) return exists;
        TFirst[] otherFirsts =
            _firstToSecond
                .Where(kvp => EqualityComparer<TSecond>.Default.Equals(kvp.Value,svalue))
                .Select(kvp => kvp.Key).ToArray();
        if (otherFirsts.IsEmpty())
            _secondToFirst.Remove(svalue);
        else _secondToFirst[svalue] = otherFirsts[0];
        return exists;
    }
    public bool RemoveBySecond(TSecond second) {
        bool exists = _secondToFirst.TryGetValue(second, out _);
        if (!exists) return exists;
        _secondToFirst.Remove(second);
        // Remove all TFirsts pointing to second
        var firsts =
            _firstToSecond
                .Where(kvp => EqualityComparer<TSecond>.Default.Equals(kvp.Value,second))
                .Select(kvp => kvp.Key).ToArray();
        foreach (var first in firsts)
            _firstToSecond.Remove(first);
        return exists;
    }
    public IReadOnlyDictionary<TFirst, TSecond> FirstToSecond => _firstToSecond;
    public IReadOnlyDictionary<TSecond, TFirst> SecondToFirst => _secondToFirst;
}