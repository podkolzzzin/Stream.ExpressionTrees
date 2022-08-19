using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace ExpressionTrees.Stream.DB
{
    public partial class IndexedDataset
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public NpgsqlTsVector? Ts { get; set; }
    }
}
