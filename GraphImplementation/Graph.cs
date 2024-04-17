namespace GraphExploring;

public class Graph
{
    public Graph(bool isDirected)
    {
        IsDirected = isDirected;
        Vertex._id = 1;
    }

    public double K { private get; set; } = 1.0 - 2 * 0.01 - 1 * 0.01 - 0.3;

    public bool isCondensation = false;
    
    public readonly bool IsDirected;
    
    public List<Vertex> Vertices = new List<Vertex>();
    
    public List<Edge> Edges = new List<Edge>();
    
    public int[,] GetMatrix()
    {
        var matrix = new int[Vertices.Count, Vertices.Count];

        foreach (var edge in Edges)
        {
            matrix[edge.From.CurrentId - 1, edge.To.CurrentId - 1] = 1;

            if (!IsDirected) matrix[edge.To.CurrentId - 1, edge.From.CurrentId - 1] = 1;
        }
        
        return matrix;
    }
    
    public void AddVertex(int value)
    {
        var newVertex = new Vertex(value);
        Vertices.Add(newVertex);
    }

    public void AddEdge(Vertex v1, Vertex v2)
    {
        if (Edges.Any(edge => edge.From == v1 && edge.To == v2))
            return;
        
        var firstEdge = new Edge(v1, v2);
        Edges.Add(firstEdge);
        v1.Edges.Add(firstEdge);

        if (!IsDirected)
        {
            if(v1 == v2) return;
            var secondEdge = new Edge(v2, v1);
            Edges.Add(secondEdge);
            v2.Edges.Add(secondEdge);
        }
    }

    private void MatrixGenerationGraph(int[,] matrix)
    {
        for (var i = 0;i<matrix.GetLength(0);i++)
        {
            for (var z =0;z<matrix.GetLength(1);z++)
            {
                if (matrix[i,z] == 1)
                {
                    AddEdge(Vertices.First(p=>p.CurrentId == i+1), Vertices.First(p=>p.CurrentId == z+1));
                }
            }
        }
    }

    private int[,] MatrixForDirected()
    {
        var variant = 3421;
        
        var n = Vertices.Count; 
        var adjacencyMatrix = new int[n, n];
        var rnd = new Random(variant);
        
        var k = K;
        
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                adjacencyMatrix[i, j] = Round( (rnd.NextDouble() * 2.0) * k);
            }
        }

        return adjacencyMatrix;
    }

    private int[,] MatrixForUnDirected(int[,] adjacencyMatrix)
    {
        var n = 12;
        var undirectedAdjMatrix = new int[n, n]; 

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                if (adjacencyMatrix[i, j] == 1 )
                {
                    undirectedAdjMatrix[i, j] = 1;
                    undirectedAdjMatrix[j, i] = 1;
                }
            }
        }

        return undirectedAdjMatrix;
    }

    public void GenerateMatrix()
    {
        if (IsDirected) MatrixGenerationGraph(MatrixForDirected());
        else MatrixGenerationGraph(MatrixForUnDirected(MatrixForDirected()));
    }

    static int Round(double num)
    {
        if (num < 1) return 0;
        return 1;
    }
}