using Range = MonoTextBox.Positioning.Range;

namespace MonoTextBox.Positioning;


public class Line
{
    private const float LeftEdge = 0f;
        
    
    private readonly List<Range> _positions = new();

    
    public IReadOnlyList<Range> Positions => _positions;
    public bool Empty => Positions.Count == 0;
    public int Length => Positions.Count;
    public float RightEdge => Empty ? 0 : Positions[^1].EndPos;
    
    
    public Line()
    { }

    public Line(IEnumerable<Range> positions) : this(positions.ToList())
    { }
    
    public Line(List<Range> positions)
    {
        if (positions.Count <= 0)
            return;

        var addend = LeftEdge - positions[0].StartPos;
        
        if (addend == 0f)
        {
            _positions = positions;
            return;
        }

        for (var i = 0; i < positions.Count; i++) 
            positions[i] += addend;
        _positions = positions;
    }
    

    public void Append(Range range) 
        => _positions.Add(LineRange(range, this));

    public void Patch(int index, Range range)
    {
        if (_positions.Count <= index)
            _positions[index] = range;
        else
            Append(range);
    }


    public static Range LineRange(Range range, Line line)
    {
        var appended = range + line.RightEdge;
        var clamped = ClampLeft(appended);
        return clamped;
    }
    
    private static Range ClampLeft(Range range)
    {
        if (range.StartPos >= LeftEdge)
            return range;
        
        var addend = LeftEdge - range.StartPos;
        return range + addend;
    }
}