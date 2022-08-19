using System;
using System.Collections.Generic;

namespace ExpressionTrees.Stream.DB
{
    public partial class Document
    {
        public Document()
        {
            WordDocuments = new HashSet<WordDocument>();
            WordDocumentsPositions = new HashSet<WordDocumentsPosition>();
        }

        public int Id { get; set; }
        public string? Content { get; set; }

        public virtual ICollection<WordDocument> WordDocuments { get; set; }
        public virtual ICollection<WordDocumentsPosition> WordDocumentsPositions { get; set; }
    }
}
