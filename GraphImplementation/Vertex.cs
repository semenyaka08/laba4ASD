namespace GraphExploring;

public class Vertex
{
    public Vertex(int value)
    {
        Value = value;
        CurrentId = _id;
        _id++;
    }
    public readonly int CurrentId;

    public static int _id = 1;
    public int Value { get; }
    
    public readonly List<Edge> Edges = new List<Edge>();
}