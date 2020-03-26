﻿using System;
using System.Collections.Generic;

using ShiftCo.ifmo_ca_lab_3.Evaluation.Interfaces;
using ShiftCo.ifmo_ca_lab_3.Evaluation.Interfaces.Markers;
using ShiftCo.ifmo_ca_lab_3.Evaluation.Patterns;
using ShiftCo.ifmo_ca_lab_3.Evaluation.Types;

using static ShiftCo.ifmo_ca_lab_3.Evaluation.Core.PatternMatcher;

namespace ShiftCo.ifmo_ca_lab_3.Evaluation.Core
{
    public static class Context
    {
        private static Dictionary<string, IPattern> _patterns = new Dictionary<string, IPattern>();
        private static List<(IElement, IElement)> _entries = new List<(IElement, IElement)>();

        public static void AddEntry(IElement lhs, IElement rhs)
        {
            _entries.Add((lhs, rhs));
        }

        public static IElement GetSubstitute(IElement element)
        {
            foreach (var rule in _entries)
            {
                var (lhs, rhs) = rule;
                if (TryMatch(element, lhs).IsSuccess)
                {
                    return GetRhs(lhs, rhs);
                }
            }
            return element;
        }

        private static IElement GetRhs(IElement lhs, IElement rhs)
        {
            _patterns = new Dictionary<string, IPattern>();
            PatternsSetUp(lhs);
            // Hardcode
            if (lhs is Expression expr &&
                expr.Operands.Count == 5 &&
                expr.Operands[0] is NullableSequencePattern seq1 &&
                expr.Operands[1] is IntegerPattern int1 &&
                expr.Operands[2] is NullableSequencePattern seq2 &&
                expr.Operands[3] is IntegerPattern int2 &&
                expr.Operands[4] is NullableSequencePattern seq3)
            {
                Integer val;
                switch (expr.Head)
                {
                    case "Sum":
                        val = new Integer(int1.Element.Value + int2.Element.Value);
                        break;

                    case "Mul":
                        val = new Integer(int1.Element.Value * int2.Element.Value);
                        break;

                    case "Pow":
                        val = new Integer((int)Math.Pow(int1.Element.Value, int2.Element.Value));
                        break;

                    default:
                        val = null;
                        break;
                }
                if (!(val is null))
                {
                    var exp = new Expression(expr.Head, new List<IElement>() { seq1, seq2, val, seq3 });
                    exp.Operands.RemoveAll(o => o is NullableSequencePattern n &&
                                                n.StoredElements.Count == 0);
                    return exp;
                }
            }
            return ApplyPatterns(rhs);
        }

        private static void PatternsSetUp(IElement lhs)
        {
            switch (lhs)
            {
                case IPattern p:
                    if (!_patterns.ContainsKey(p.Name.Value))
                    {
                        _patterns.Add(p.Name.Value, p);
                    }
                    break;

                case Expression e:
                    foreach (var o in e.Operands)
                    {
                        PatternsSetUp(o);
                    }
                    break;

                default:
                    break;
            }
        }

        private static IElement ApplyPatterns(IElement rhs)
        {
            IPattern pattern = null;
            switch (rhs)
            {
                case IPattern p when p.GetType() != _patterns[p.Name.Value].GetType():
                    throw new Exception("Pattern types does not matches");
                case IntegerPattern integer:
                    pattern = _patterns[integer.Name.Value];
                    return ((IntegerPattern)pattern).Element;

                case ElementPattern element:
                    pattern = _patterns[element.Name.Value];
                    return ((ElementPattern)pattern).Element;

                case Expression exp:
                    for (int i = 0; i < exp.Operands.Count; i++)
                    {
                        if (exp.Operands[i] is NullableSequencePattern seq)
                        {
                            pattern = _patterns[seq.Name.Value];
                            if (!(pattern is NullableSequencePattern))
                            {
                                throw new Exception("Pattern types does not matches");
                            }
                            if (((NullableSequencePattern)pattern).StoredElements.Count > 0)
                            {
                                exp.Operands.InsertRange(i, ((NullableSequencePattern)pattern).StoredElements);
                            }
                        }
                        else
                        {
                            exp.Operands[i] = ApplyPatterns(exp.Operands[i]);
                        }
                    }
                    exp.Operands.RemoveAll(o => o is NullableSequencePattern n &&
                                                n.StoredElements.Count == 0);
                    return exp;

                default:
                    return rhs;
            }
        }
    }
}
