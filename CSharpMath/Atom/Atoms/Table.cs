using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Atom.Atoms;

///<summary>A table. Not part of TeX.</summary>
public sealed class Table(string? environment, List<List<MathList>>? cells = null)
    : MathAtom(string.Empty), IMathListContainer {
    public Table() : this(null) { }
    /// <summary>Deep copy, finalized or not.</summary>
    public new Table Clone(bool finalize) => (Table)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Table(Environment) {
        InterColumnSpacing = InterColumnSpacing,
        InterRowAdditionalSpacing = InterRowAdditionalSpacing,
        Environment = Environment,
        Alignments = Alignments.ToList(),
        Cells = [
            ..Cells.Select(list =>
                new List<MathList>(list.Select(sublist => sublist.Clone(finalize))))
        ]
    };
    public override bool ScriptsAllowed => false;
    public List<ColumnAlignment> Alignments { get; private set; } = [];
    /// <summary>Cells[i][j] == cell at ith row and jth column</summary>
    public List<List<MathList>> Cells { get; private set; } = cells ?? [];

    IEnumerable<MathList> IMathListContainer.InnerLists => Cells.SelectMany(row => row);
    /// <summary>Space between columns in mu units.</summary>
    public float InterColumnSpacing { get; set; }
    /// <summary>
    /// Additional spacing between rows in jots (one jot is 0.3 times font size).
    /// </summary>
    public float InterRowAdditionalSpacing { get; set; }
    /// <summary>The name of the environment that this table denotes</summary>
    public string? Environment { get; set; } = environment;

    /// <summary>Number of rows</summary>
    public int NRows => Cells.Count;
    /// <summary>Number of columns</summary>
    public int NColumns => NRows == 0 ? 0 : Cells.Max(row => row.Count);
    public void SetCell(MathList list, int iRow, int iColumn) {
        while (Cells.Count <= iRow) Cells.Add(new List<MathList>());
        while (Cells[iRow].Count <= iColumn) Cells[iRow].Add(new MathList());
        Cells[iRow][iColumn] = list;
    }
    public void SetAlignment(ColumnAlignment alignment, int columnIndex) {
        while (Alignments.Count <= columnIndex) Alignments.Add(ColumnAlignment.Center);
        Alignments[columnIndex] = alignment;
    }
    public ColumnAlignment GetAlignment(int columnIndex) =>
        Alignments.Count <= columnIndex ? ColumnAlignment.Center : Alignments[columnIndex];
    public bool EqualsTable(Table otherTable) =>
        EqualsAtom(otherTable) &&
        NRows == otherTable.NRows &&
        Cells.SequenceEqual(otherTable.Cells, EqualityComparer<List<MathList>>.Default) &&
        Alignments.SequenceEqual(otherTable.Alignments);
    public override bool Equals(object? obj) => obj is Table t && EqualsTable(t);
    public override int GetHashCode() =>
        (base.GetHashCode(), Cells, Alignments).GetHashCode();
}