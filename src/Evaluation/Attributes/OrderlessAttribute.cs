﻿using ShiftCo.ifmo_ca_lab_3.Evaluation.Interfaces;
using ShiftCo.ifmo_ca_lab_3.Evaluation.Types;
using ShiftCo.ifmo_ca_lab_3.Evaluation.Util;

namespace ShiftCo.ifmo_ca_lab_3.Evaluation.Attributes
{
    public class OrderlessAttribute : IAttribute
    {
        public Expression Apply(Expression expr)
        {
            if (expr.Head == "Pow") return expr;
            var operands = expr.Operands;
            if (operands != null)
                operands.Sort(new ElementComparer());
            return new Expression(expr.Head, operands);
        }
    }
}
