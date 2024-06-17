using System.Collections.Generic;

namespace Match3Game.Data
{
    public class FieldLine
    {
        public List<FieldCell> Cells = new List<FieldCell>();

        public FieldLine(List<FieldCell> cells)
        {
            Cells.AddRange(cells);
        }
    }
}