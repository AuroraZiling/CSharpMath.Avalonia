using System;
using System.Collections.Generic;

namespace CSharpMath.Atom {
  using Atoms;
  //https://mirror.hmc.edu/ctan/macros/latex/contrib/unicode-math/unimath-symbols.pdf
  public static class LaTeXSettings {
    public static MathAtom Times => new BinaryOperator("×");
    public static MathAtom Divide => new BinaryOperator("÷");
    public static MathAtom Placeholder => new Placeholder("\u25A1");
    public static MathList PlaceholderList => new MathList { Placeholder };

    public static MathAtom? ForAscii(sbyte c) {
      if (c < 0) throw new ArgumentOutOfRangeException(nameof(c), c, "The character cannot be negative");
      var s = ((char)c).ToStringInvariant();
      if (char.IsControl((char)c) || char.IsWhiteSpace((char)c)) {
        return null; // skip spaces
      }
      if (c >= '0' && c <= '9') {
        return new Number(s);
      }
      if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) {
        return new Variable(s);
      }
      switch (c) {
        case (sbyte)'$':
        case (sbyte)'%':
        case (sbyte)'#':
        case (sbyte)'&':
        case (sbyte)'~':
        case (sbyte)'\'':
        case (sbyte)'^':
        case (sbyte)'_':
        case (sbyte)'{':
        case (sbyte)'}':
        case (sbyte)'\\': // All these are special characters we don't support.
          return null;
        case (sbyte)'(':
        case (sbyte)'[':
          return new Open(s);
        case (sbyte)')':
        case (sbyte)']':
          return new Close(s);
        case (sbyte)'!':
        case (sbyte)'?':
          return new Close(s, hasCorrespondingOpen: false);
        case (sbyte)',':
        case (sbyte)';':
          return new Punctuation(s);
        case (sbyte)'=':
        case (sbyte)'<':
        case (sbyte)'>':
          return new Relation(s);
        case (sbyte)':': // Colon is a ratio. Regular colon is \colon
          return new Relation("\u2236");
        case (sbyte)'-': // Use the math minus sign
          return new BinaryOperator("\u2212");
        case (sbyte)'+':
        case (sbyte)'*': // Star operator, not multiplication
          return new BinaryOperator(s);
        case (sbyte)'.':
          return new Number(s);
        case (sbyte)'"':
        case (sbyte)'/':
        case (sbyte)'@':
        case (sbyte)'`':
        case (sbyte)'|':
          return new Ordinary(s);
        default:
          throw new Structures.InvalidCodePathException
            ($"Ascii character {c} should have been accounted for.");
      }
    }

    public static Structures.AliasDictionary<string, Boundary> BoundaryDelimiters { get; } =
      new Structures.AliasDictionary<string, Boundary> {
        { ".", Boundary.Empty }, // . means no delimiter

        // Table 14: Delimiters
        { "(", new Boundary("(") },
        { ")", new Boundary(")") },
        { "uparrow", new Boundary("↑") },
        { "Uparrow", new Boundary("⇑") },
        { "[", new Boundary("[") },
        { "]", new Boundary("]") },
        { "downarrow", new Boundary("↓") },
        { "Downarrow", new Boundary("⇓") },
        { "{", "lbrace", new Boundary("{") },
        { "}", "rbrace", new Boundary("}") },
        { "updownarrow", new Boundary("↕") },
        { "Updownarrow", new Boundary("⇕") },
        { "lfloor", new Boundary("⌊") },
        { "rfloor", new Boundary("⌋") },
        { "lceil", new Boundary("⌈") },
        { "rceil", new Boundary("⌉") },
        { "<", "langle", new Boundary("〈") },
        { ">", "rangle", new Boundary("〉") },
        { "/", new Boundary("/") },
        { "\\", "backslash", new Boundary("\\") },
        { "|", "vert", new Boundary("|") },
        { "||", "Vert", new Boundary("‖") },

        // Table 15: Large Delimiters
        // { "lmoustache", new Boundary("⎰") }, // Glyph not in Latin Modern Math
        // { "rmoustache", new Boundary("⎱") }, // Glyph not in Latin Modern Math
        { "rgroup", new Boundary("⟯") },
        { "lgroup", new Boundary("⟮") },
        { "arrowvert", new Boundary("|") }, // unsure, copied from \vert
        { "Arrowvert", new Boundary("‖") }, // unsure, copied from \Vert
        { "bracevert", new Boundary("|") }, // unsure, copied from \vert

        // Table 19: AMS Delimiters
        { "ulcorner", new Boundary("⌜") },
        { "urcorner", new Boundary("⌝") },
        { "llcorner", new Boundary("⌞") },
        { "lrcorner", new Boundary("⌟") },
      };

    public static Structures.AliasDictionary<string, FontStyle> FontStyles { get; } =
      new Structures.AliasDictionary<string, FontStyle> {
        { "mathnormal", FontStyle.Default },
        { "mathrm", "rm", "text", FontStyle.Roman },
        { "mathbf", "bf", FontStyle.Bold },
        { "mathcal", "cal", FontStyle.Caligraphic },
        { "mathtt", FontStyle.Typewriter },
        { "mathit", "mit", FontStyle.Italic },
        { "mathsf", FontStyle.SansSerif },
        { "mathfrak", "frak", FontStyle.Fraktur },
        { "mathbb", FontStyle.Blackboard },
        { "mathbfit", "bm", FontStyle.BoldItalic },
      };

    public static MathAtom? AtomForCommand(string symbolName) =>
      Commands.TryGetValue(
        symbolName ?? throw new ArgumentNullException(nameof(symbolName)),
        out var symbol) ? symbol.Clone(false) : null;

    public static string? CommandForAtom(MathAtom atom) {
      var atomWithoutScripts = atom.Clone(false);
      atomWithoutScripts.Superscript.Clear();
      atomWithoutScripts.Subscript.Clear();
      if (atomWithoutScripts is IMathListContainer container)
        foreach (var list in container.InnerLists)
          list.Clear();
      return Commands.TryGetKey(atomWithoutScripts, out var name) ? name : null;
    }

    public static Structures.AliasDictionary<string, MathAtom> Commands { get; } =
      new Structures.AliasDictionary<string, MathAtom> {
        // Custom additions
        { "because", new Ordinary("\u2235") }, //not in iosMath
        { "therefore", new Ordinary("\u2234") }, //not in iosMath
        { "diameter", new Ordinary("\u2300") }, // not in iosMath
        { "degree", new Ordinary("°") },
        { "iiint", new LargeOperator("∭", false) },
        { "iiiint", new LargeOperator("⨌", false) },
        { "oiiint", new LargeOperator("∰", false) },
        { "intclockwise", new LargeOperator("∱", false) },
        { "awint", new LargeOperator("⨑", false) },
        { "varointclockwise", new LargeOperator("∲", false) },
        { "ointctrclockwise", new LargeOperator("∳", false) },
        { "iint", new LargeOperator("∬", false) },
        { "oiint", new LargeOperator("∯", false) },
        { "bigbot", new LargeOperator("⟘", null) },
        { "bigtop", new LargeOperator("⟙", null) },
        { "bigcupdot", new LargeOperator("⨃", null) },
        { "bigsqcap", new LargeOperator("⨅", null) },
        { "bigtimes", new LargeOperator("⨉", null) },
        { "arsinh", new LargeOperator("arsinh", false, true) },
        { "arcosh", new LargeOperator("arcosh", false, true) },
        { "artanh", new LargeOperator("artanh", false, true) },
        { "arccot", new LargeOperator("arccot", false, true) },
        { "arcoth", new LargeOperator("arcoth", false, true) },
        { "arcsec", new LargeOperator("arcsec", false, true) },
        { "sech", new LargeOperator("sech", false, true) },
        { "arsech", new LargeOperator("arsech", false, true) },
        { "arccsc", new LargeOperator("arccsc", false, true) },
        { "csch", new LargeOperator("csch", false, true) },
        { "arcsch", new LargeOperator("arcsch", false, true) },
        // Use escape sequence for combining characters
        { "overbar", new Accent("\u0305") },
        { "ovhook", new Accent("\u0309") },
        { "ocirc", new Accent("\u030A") },
        { "leftharpoonaccent", new Accent("\u20D0") },
        { "rightharpoonaccent", new Accent("\u20D1") },
        { "vertoverlay", new Accent("\u20D2") },
        { "dddot", new Accent("\u20DB") },
        { "ddddot", new Accent("\u20DC") },
        { "widebridgeabove", new Accent("\u20E9") },
        { "asteraccent", new Accent("\u20F0") },
        { "threeunderdot", new Accent("\u20E8") },

        // Standard TeX
        { " ", new Ordinary(" ") },
        { "lceil", new Open("⌈") },
        { "rceil", new Close("⌉") },
        { "lfloor", new Open("⌊") },
        { "rfloor", new Close("⌋") },
        { "langle", new Open("〈") },
        { "rangle", new Close("〉") },
        { "lgroup", new Open("⟮") },
        { "rgroup", new Close("⟯") },
        { ",", new Space(Structures.Space.ShortSpace) },
        { ":", ">", new Space(Structures.Space.MediumSpace) },
        { ";", new Space(Structures.Space.LongSpace) },
        { "!", new Space(-Structures.Space.ShortSpace) },
        { "enspace", new Space(Structures.Space.EmWidth / 2) },
        { "quad", new Space(Structures.Space.EmWidth) },
        { "qquad", new Space(Structures.Space.EmWidth * 2) },
        { "displaystyle", new Style(LineStyle.Display) },
        { "textstyle", new Style(LineStyle.Text) },
        { "scriptstyle", new Style(LineStyle.Script) },
        { "scriptscriptstyle",  new Style(LineStyle.ScriptScript) },

        // LaTeX Symbol List: https://rpi.edu/dept/arc/training/latex/LaTeX_symbols.pdf
        // (Included in the same folder as this file)
        // Shorter list: https://www.andy-roberts.net/res/writing/latex/symbols.pdf

        // Command <-> Unicode: https://www.johndcook.com/unicode_latex.html
        // Unicode char lookup: https://unicode-table.com/en/search/
        // Reference LaTeX output for glyph: https://www.codecogs.com/latex/eqneditor.php
        // Look at what glyphs are in a font: https://github.com/fontforge/fontforge

        // Following tables are from the LaTeX Symbol List
        // Table 1: Escapable “Special” Characters
        { "$", new Ordinary("$") },
        { "%", new Ordinary("%") },
        { "_", new Ordinary("_") },
        { "}", "rbrace", new Close("}") },
        { "&", new Ordinary("&") },
        { "#", new Ordinary("#") },
        { "{", "lbrace", new Open("{") },

        // Table 2: LaTeX2ε Commands Deﬁned to Work in Both Math and Text Mode
        // $ is defined in Table 1
        { "P", new Ordinary("¶") },
        { "S", new Ordinary("§") },
        // _ is defined in Table 1
        { "copyright", new Ordinary("©") },
        { "dag", new Ordinary("†") },
        { "ddag", new Ordinary("‡") },
        { "dots", new Ordinary("…") },
        { "pounds", new Ordinary("£") },
        // { is defined in Table 1
        // } is defined in Table 1

        // Table 3: Non-ASCII Letters (Excluding Accented Letters)
        { "aa", new Ordinary("å") },
        { "AA", "angstrom", new Ordinary("Å") },
        { "AE", new Ordinary("Æ") },
        { "ae", new Ordinary("æ") },
        { "DH", new Ordinary("Ð") },
        { "dh", new Ordinary("ð") },
        { "DJ", new Ordinary("Đ") },
        //{ "dj", new Ordinary("đ") }, // Glyph not in Latin Modern Math
        { "L", new Ordinary("Ł") },
        { "l", new Ordinary("ł") },
        { "NG", new Ordinary("Ŋ") },
        { "ng", new Ordinary("ŋ") },
        { "o", new Ordinary("ø") },
        { "O", new Ordinary("Ø") },
        { "OE", new Ordinary("Œ") },
        { "oe", new Ordinary("œ") },
        { "ss", new Ordinary("ß") },
        { "SS", new Ordinary("SS") },
        { "TH", new Ordinary("Þ") },
        { "th", new Ordinary("þ") },

        // Table 4: Greek Letters
        { "alpha", new Variable("α") },
        { "beta", new Variable("β") },
        { "gamma", new Variable("γ") },
        { "delta", new Variable("δ") },
        { "epsilon", new Variable("ϵ") },
        { "varepsilon", new Variable("ε") },
        { "zeta", new Variable("ζ") },
        { "eta", new Variable("η") },
        { "theta", new Variable("θ") },
        { "vartheta", new Variable("ϑ") },
        { "iota", new Variable("ι") },
        { "kappa", new Variable("κ") },
        { "lambda", new Variable("λ") },
        { "mu", new Variable("µ") },
        { "nu", new Variable("ν") },
        { "xi", new Variable("ξ") },
        { "omicron", new Variable("ο") },
        { "pi", new Variable("π") },
        { "varpi", new Variable("ϖ") },
        { "rho", new Variable("ρ") },
        { "varrho", new Variable("ϱ") },
        { "sigma", new Variable("σ") },
        { "varsigma", new Variable("ς") },
        { "tau", new Variable("τ") },
        { "upsilon", new Variable("υ") },
        { "phi", new Variable("φ") },
        { "varphi", new Variable("ϕ") },
        { "chi", new Variable("χ") },
        { "psi", new Variable("ψ") },
        { "omega", new Variable("ω") },

        { "Gamma", new Variable("Γ") },
        { "Delta", new Variable("∆") },
        { "Theta", new Variable("Θ") },
        { "Lambda", new Variable("Λ") },
        { "Xi", new Variable("Ξ") },
        { "Pi", new Variable("Π") },
        { "Sigma", new Variable("Σ") },
        { "Upsilon", new Variable("Υ") },
        { "Phi", new Variable("Φ") },
        { "Psi", new Variable("Ψ") },
        { "Omega", new Variable("Ω") },
        // (The remaining Greek majuscules can be produced with ordinary Latin letters.
        // The symbol “M”, for instance, is used for both an uppercase “m” and an uppercase “µ”.

        // Table 5: Punctuation Marks Not Found in OT
        // [Skip text mode commands]

        // Table 6: Predeﬁned LaTeX2ε Text-Mode Commands
        // [Skip text mode commands]

        // Table 7: Binary Operation Symbols
        { "pm", new BinaryOperator("±") },
        { "mp", new BinaryOperator("∓") },
        { "times", Times },
        { "div", Divide },
        { "ast", new BinaryOperator("∗") },
        { "star" , new BinaryOperator("⋆") },
        { "circ" , new BinaryOperator("◦") },
        { "bullet", new BinaryOperator("•") },
        { "cdot" , new BinaryOperator("·") },
        // +
        { "cap", new BinaryOperator("∩") },
        { "cup", new BinaryOperator("∪") },
        { "uplus", new BinaryOperator("⊎") },
        { "sqcap", new BinaryOperator("⊓") },
        { "sqcup", new BinaryOperator("⊔") },
        { "vee", "lor", new BinaryOperator("∨") },
        { "wedge", "land", new BinaryOperator("∧") },
        { "setminus", new BinaryOperator("∖") },
        { "wr", new BinaryOperator("≀") },
        // -
        { "diamond", new BinaryOperator("⋄") },
        { "bigtriangleup", new BinaryOperator("△") },
        { "bigtriangledown", new BinaryOperator("▽") },
        { "triangleleft", new BinaryOperator("◁") }, // Latin Modern Math doesn't have ◃
        { "triangleright", new BinaryOperator("▷") }, // Latin Modern Math doesn't have ▹
        { "lhd", new BinaryOperator("⊲") },
        { "rhd", new BinaryOperator("⊳") },
        { "unlhd", new BinaryOperator("⊴") },
        { "unrhd", new BinaryOperator("⊵") },
        { "oplus", new BinaryOperator("⊕") },
        { "ominus", new BinaryOperator("⊖") },
        { "otimes", new BinaryOperator("⊗") },
        { "oslash", new BinaryOperator("⊘") },
        { "odot", new BinaryOperator("⊙") },
        { "bigcirc", new BinaryOperator("◯") },
        { "dagger", new BinaryOperator("†") },
        { "ddagger", new BinaryOperator("‡") },
        { "amalg", new BinaryOperator("⨿") },

        // Table 8: Relation Symbols
        { "leq", "le", new Relation("≤") },
        { "geq", "ge", new Relation("≥") },
        { "equiv", new Relation("≡") },
        { "models", new Relation("⊧") },
        { "prec", new Relation("≺") },
        { "succ", new Relation("≻") },
        { "sim", new Relation("∼") },
        { "perp", new Relation("⟂") },
        { "preceq", new Relation("⪯") },
        { "succeq", new Relation("⪰") },
        { "simeq", new Relation("≃") },
        { "mid", new Relation("∣") },
        { "ll", new Relation("≪") },
        { "gg", new Relation("≫") },
        { "asymp", new Relation("≍") },
        { "parallel", new Relation("∥") },
        { "subset", new Relation("⊂") },
        { "supset", new Relation("⊃") },
        { "approx", new Relation("≈") },
        { "bowtie", new Relation("⋈") },
        { "subseteq", new Relation("⊆") },
        { "supseteq", new Relation("⊇") },
        { "cong", new Relation("≅") },
        // Latin Modern Math doesn't have ⨝ so we copy the one from \bowtie
        { "Join", new Relation("⋈") }, // Capital J is intentional
        { "sqsubset", new Relation("⊏") },
        { "sqsupset", new Relation("⊐") },
        { "neq", "ne", new Relation("≠") },
        { "smile", new Relation("⌣") },
        { "sqsubseteq", new Relation("⊑") },
        { "sqsupseteq", new Relation("⊒") },
        { "doteq", new Relation("≐") },
        { "frown", new Relation("⌢") },
        { "in", new Relation("∈") },
        { "ni", new Relation("∋") },
        { "notin", new Relation("∉") },
        { "propto", new Relation("∝") },
        // =
        { "vdash", new Relation("⊢") },
        { "dashv", new Relation("⊣") },
        // <
        // >
        // :
        
        // Table 9: Punctuation Symbols
        // ,
        // ;
        { "colon", new Punctuation(":") }, // \colon is different from : which is a relation
        { "ldotp", new Punctuation(".") }, // Aka the full stop or decimal dot
        { "cdotp", new Punctuation("·") },

        // Table 10: Arrow Symbols 
        { "leftarrow", "gets", new Relation("←") },
        { "longleftarrow", new Relation("⟵") },
        { "uparrow", new Relation("↑") },
        { "Leftarrow", new Relation("⇐") },
        { "Longleftarrow", new Relation("⟸") },
        { "Uparrow", new Relation("⇑") },
        { "rightarrow", "to", new Relation("→") },
        { "longrightarrow", new Relation("⟶") },
        { "downarrow", new Relation("↓") },
        { "Rightarrow", new Relation("⇒") },
        { "Longrightarrow", new Relation("⟹") },
        { "Downarrow", new Relation("⇓") },
        { "leftrightarrow", new Relation("↔") },
        { "Leftrightarrow", new Relation("⇔") },
        { "updownarrow", new Relation("↕") },
        { "longleftrightarrow", new Relation("⟷") },
        { "Longleftrightarrow", "iff", new Relation("⟺") },
        { "Updownarrow", new Relation("⇕") },
        { "mapsto", new Relation("↦") },
        { "longmapsto", new Relation("⟼") },
        { "nearrow", new Relation("↗") },
        { "hookleftarrow", new Relation("↩") },
        { "hookrightarrow", new Relation("↪") },
        { "searrow", new Relation("↘") },
        { "leftharpoonup", new Relation("↼") },
        { "rightharpoonup", new Relation("⇀") },
        { "swarrow", new Relation("↙") },
        { "leftharpoondown", new Relation("↽") },
        { "rightharpoondown", new Relation("⇁") },
        { "nwarrow", new Relation("↖") },
        { "rightleftharpoons", new Relation("⇌") },
        { "leadsto", new Relation("⇝") }, // unsure, copied from \rightsquigarrow

        // Table 11: Miscellaneous Symbols
        { "ldots", new Ordinary("…") },
        { "aleph", new Ordinary("ℵ") },
        { "hbar", new Ordinary("ℏ") },
        { "imath", new Ordinary("𝚤") },
        { "jmath", new Ordinary("𝚥") },
        { "ell", new Ordinary("ℓ") },
        { "wp", new Ordinary("℘") },
        { "Re", new Ordinary("ℜ") },
        { "Im", new Ordinary("ℑ") },
        { "mho", new Ordinary("℧") },
        { "cdots", new Ordinary("⋯") },
        // \prime is removed because Unicode has no matching character
        { "emptyset", new Ordinary("∅") },
        { "nabla", new Ordinary("∇") },
        { "surd", new Ordinary("√") },
        { "top", new Ordinary("⊤") },
        { "bot", new Ordinary("⊥") },
        { "|", "Vert", new Ordinary("‖") },
        { "angle", new Ordinary("∠") },
        // .
        { "vdots", new Ordinary("⋮") },
        { "forall", new Ordinary("∀") },
        { "exists", new Ordinary("∃") },
        { "neg", "lnot", new Ordinary("¬") },
        { "flat", new Ordinary("♭") },
        { "natural", new Ordinary("♮") },
        { "sharp", new Ordinary("♯") },
        { "backslash", new Ordinary("\\") },
        { "partial", new Ordinary("𝜕") },
        { "vert", new Ordinary("|") },
        { "ddots", new Ordinary("⋱") },
        { "infty", new Ordinary("∞") },
        { "Box", new Ordinary("□") }, // unsure, copied from \square
        { "Diamond", new Ordinary("♢") }, // Unsure, copied from \diamondsuit
        { "triangle", new Ordinary("△") },
        { "clubsuit", new Ordinary("♣") },
        { "diamondsuit", new Ordinary("♢") },
        { "heartsuit", new Ordinary("♡") },
        { "spadesuit", new Ordinary("♠") },

        // Table 12: Variable-sized Symbols 
        { "sum", new LargeOperator("∑", null) },
        { "prod", new LargeOperator("∏", null) },
        { "coprod", new LargeOperator("∐", null) },
        { "int", new LargeOperator("∫", false) },
        { "oint", new LargeOperator("∮", false) },
        { "bigcap", new LargeOperator("⋂", null) },
        { "bigcup", new LargeOperator("⋃", null) },
        { "bigsqcup", new LargeOperator("⨆", null) },
        { "bigvee", new LargeOperator("⋁", null) },
        { "bigwedge", new LargeOperator("⋀", null) },
        { "bigodot", new LargeOperator("⨀", null) },
        { "bigoplus", new LargeOperator("⨁", null) },
        { "bigotimes", new LargeOperator("⨂", null) },
        { "biguplus", new LargeOperator("⨄", null) },

        // Table 13: Log-like Symbols 
        { "arccos", new LargeOperator("arccos", false, true) },
        { "arcsin", new LargeOperator("arcsin", false, true) },
        { "arctan", new LargeOperator("arctan", false, true) },
        { "arg", new LargeOperator("arg", false, true) },
        { "cos", new LargeOperator("cos", false, true) },
        { "cosh", new LargeOperator("cosh", false, true) },
        { "cot", new LargeOperator("cot", false, true) },
        { "coth", new LargeOperator("coth", false, true) },
        { "csc", new LargeOperator("csc", false, true) },
        { "deg", new LargeOperator("deg", false, true) },
        { "det", new LargeOperator("det", null) },
        { "dim", new LargeOperator("dim", false, true) },
        { "exp", new LargeOperator("exp", false, true) },
        { "gcd", new LargeOperator("gcd", null) },
        { "hom", new LargeOperator("hom", false, true) },
        { "inf", new LargeOperator("inf", null) },
        { "ker", new LargeOperator("ker", false, true) },
        { "lg", new LargeOperator("lg", false, true) },
        { "lim", new LargeOperator("lim", null) },
        { "liminf", new LargeOperator("lim inf", null) },
        { "limsup", new LargeOperator("lim sup", null) },
        { "ln", new LargeOperator("ln", false, true) },
        { "log", new LargeOperator("log", false, true) },
        { "max", new LargeOperator("max", null) },
        { "min", new LargeOperator("min", null) },
        { "Pr", new LargeOperator("Pr", null) },
        { "sec", new LargeOperator("sec", false, true) },
        { "sin", new LargeOperator("sin", false, true) },
        { "sinh", new LargeOperator("sinh", false, true) },
        { "sup", new LargeOperator("sup", null) },
        { "tan", new LargeOperator("tan", false, true) },
        { "tanh", new LargeOperator("tanh", false, true) },

        // Table 14: Delimiters
        // Table 15: Large Delimiters
        // [See BoundaryDelimiters dictionary above]

        // Table 16: Math-Mode Accents
        // Use escape sequence for combining characters
        { "hat", new Accent("\u0302") },  // In our implementation hat and widehat behave the same.
        { "acute", new Accent("\u0301") },
        { "bar", new Accent("\u0304") },
        { "dot", new Accent("\u0307") },
        { "breve", new Accent("\u0306") },
        { "check", new Accent("\u030C") },
        { "grave", new Accent("\u0300") },
        { "vec", new Accent("\u20D7") },
        { "ddot", new Accent("\u0308") },
        { "tilde", new Accent("\u0303") }, // In our implementation tilde and widetilde behave the same.

        // Table 17: Some Other Constructions
        { "widehat", new Accent("\u0302") },
        { "widetilde", new Accent("\u0303") },
#warning implement \overleftarrow, \overrightarrow, \overbrace, \underbrace
        // \overleftarrow{}
        // \overrightarrow{}
        // \overline{}
        // \underline{}
        // \overbrace{}
        // \underbrace{}
        // \sqrt{}
        // \sqrt[]{}
        // '
        // \frac{}{}

        // Table 18: textcomp Symbols
        // [Skip text mode commands]

        // Table 19: AMS Delimiters
        // [See BoundaryDelimiters dictionary above]

        // Table 20: AMS Arrows
        //{ "dashrightarrow", new Relation("⇢") }, // Glyph not in Latin Modern Math
        //{ "dashleftarrow", new Relation("⇠") }, // Glyph not in Latin Modern Math
        { "leftleftarrows", new Relation("⇇") },
        { "leftrightarrows", new Relation("⇆") },
        { "Lleftarrow", new Relation("⇚") },
        { "twoheadleftarrow", new Relation("↞") },
        { "leftarrowtail", new Relation("↢") },
        { "looparrowleft", new Relation("↫") },
        { "leftrightharpoons", new Relation("⇋") },
        { "curvearrowleft", new Relation("↶") },
        { "circlearrowleft", new Relation("↺") },
        { "Lsh", new Relation("↰") },
        { "upuparrows", new Relation("⇈") },
        { "upharpoonleft", new Relation("↿") },
        { "downharpoonleft", new Relation("⇃") },
        { "multimap", new Relation("⊸") },
        { "leftrightsquigarrow", new Relation("↭") },
        { "rightrightarrows", new Relation("⇉") },
        { "rightleftarrows", new Relation("⇄") },
        // Duplicate entry in LaTeX Symbol list: \rightrightarrows
        // Duplicate entry in LaTeX Symbol list: \rightleftarrows
        { "twoheadrightarrow", new Relation("↠") },
        { "rightarrowtail", new Relation("↣") },
        { "looparrowright", new Relation("↬") },
        // \rightleftharpoons defined in Table 10
        { "curvearrowright", new Relation("↷") },
        { "circlearrowright", new Relation("↻") },
        { "Rsh", new Relation("↱") },
        { "downdownarrows", new Relation("⇊") },
        { "upharpoonright", new Relation("↾") },
        { "downharpoonright", new Relation("⇂") },
        { "rightsquigarrow", new Relation("⇝") },

        // Table 21: AMS Negated Arrows
        { "nleftarrow", new Relation("↚") },
        { "nrightarrow", new Relation("↛") },
        { "nLeftarrow", new Relation("⇍") },
        { "nRightarrow", new Relation("⇏") },
        { "nleftrightarrow", new Relation("↮") },
        { "nLeftrightarrow", new Relation("⇎") },

        // Table 22: AMS Greek 
        // { "digamma", new Variable("ϝ") }, // Glyph not in Latin Modern Math
        { "varkappa", new Variable("ϰ") },

        // Table 23: AMS Hebrew
        { "beth", new Ordinary("ℶ") },
        { "daleth", new Ordinary("ℸ") },
        { "gimel", new Ordinary("ℷ") },

        // Table 24: AMS Miscellaneous
        // \hbar defined in Table 11
        { "hslash", new Ordinary("ℏ") }, // Same as \hbar
        { "vartriangle", new Ordinary("△") }, // ▵ not in Latin Modern Math // ▵ is actually a triangle, not an inverted v as displayed in Visual Studio
        { "triangledown", new Ordinary("▽") }, // ▿ not in Latin Modern Math
        { "square", Placeholder },
        { "lozenge", new Ordinary("◊") },
        // { "circledS", new Ordinary("Ⓢ") }, // Glyph not in Latin Modern Math
        // \angle defined in Table 11
        { "measuredangle", new Ordinary("∡") },
        { "nexists", new Ordinary("∄") },
        // \mho defined in Table 11
        // { "Finv", new Ordinary("Ⅎ") }, // Glyph not in Latin Modern Math
        // { "Game", new Ordinary("⅁") }, // Glyph not in Latin Modern Math
        { "Bbbk", new Ordinary("𝐤") },
        { "backprime", new Ordinary("‵") },
        { "varnothing", new Ordinary("∅") }, // Same as \emptyset
        { "blacktriangle", new Ordinary("▲") }, // ▴ not in Latin Modern Math
        { "blacktriangledown", new Ordinary("▼") }, // ▾ not in Latin Modern Math
        { "blacksquare", new Ordinary("▪") },
        { "blacklozenge", new Ordinary("♦") }, // ⧫ not in Latin Modern Math
        { "bigstar", new Ordinary("⋆") }, // ★ not in Latin Modern Math
        { "sphericalangle", new Ordinary("∢") },
        { "complement", new Ordinary("∁") },
        { "eth", new Ordinary("ð") }, // Same as \dh
        { "diagup", new Ordinary("/") }, // ╱ not in Latin Modern Math
        { "diagdown", new Ordinary("\\") }, // ╲ not in Latin Modern Math

        // Table 25: AMS Commands Deﬁned to Work in Both Math and Text Mode
        { "checkmark", new Ordinary("✓") },
        { "circledR", new Ordinary("®") },
        { "maltese", new Ordinary("✠") },

        // Table 26: AMS Binary Operators
        { "dotplus", new BinaryOperator("∔") },
        { "smallsetminus", new BinaryOperator("∖") },
        { "Cap", new BinaryOperator("⋒") },
        { "Cup", new BinaryOperator("⋓") },
        { "barwedge", new BinaryOperator("⌅") },
        { "veebar", new BinaryOperator("⊻") },
        // { "doublebarwedge", new BinaryOperator("⩞") }, //Glyph not in Latin Modern Math
        { "boxminus", new BinaryOperator("⊟") },
        { "boxtimes", new BinaryOperator("⊠") },
        { "boxdot", new BinaryOperator("⊡") },
        { "boxplus", new BinaryOperator("⊞") },
        { "divideontimes", new BinaryOperator("⋇") },
        { "ltimes", new BinaryOperator("⋉") },
        { "rtimes", new BinaryOperator("⋊") },
        { "leftthreetimes", new BinaryOperator("⋋") },
        { "rightthreetimes", new BinaryOperator("⋌") },
        { "curlywedge", new BinaryOperator("⋏") },
        { "curlyvee", new BinaryOperator("⋎") },
        { "circleddash", new BinaryOperator("⊝") },
        { "circledast", new BinaryOperator("⊛") },
        { "circledcirc", new BinaryOperator("⊚") },
        { "centerdot", new BinaryOperator("·") },
        { "intercal", new BinaryOperator("⊺") },
      };
  }
}
