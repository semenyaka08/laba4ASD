using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphExploring;
using Matrix = MatrixLibrary.Matrix;


namespace VisualGraph;

public partial class MainWindow 
{
    private readonly Graph _graph = new Graph(true);

    private List<List<int>> _result;
    
    public MainWindow()
    {
        InitializeComponent();
        _graph.AddVertex(1);
        _graph.AddVertex(2);
        _graph.AddVertex(3);
        _graph.AddVertex(4);
        _graph.AddVertex(5);
        _graph.AddVertex(6);
        _graph.AddVertex(7);
        _graph.AddVertex(8);
        _graph.AddVertex(9);
        _graph.AddVertex(10);
        _graph.AddVertex(11);
        _graph.AddVertex(12);
        
        var button = new Button
        {
            Content = "Намалювати новий" + Environment.NewLine + "        орграф",
            Width = 115,
            Height = 50,
        };
        button.Click += Button_Click;
        
        Canvas.SetLeft(button, 50);
        Canvas.SetTop(button, 50);
        Canvas.Children.Add(button);
        
        DrawGraph(_graph);
        DisplayMatrix(_graph);
        Calculations();
    }
    
    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        Canvas.Children.RemoveRange(0,Canvas.Children.Count);
        _graph.K = 1 - 2 * 0.005 - 1 * 0.005 - 0.27;
        await Task.Delay(500);
        DrawGraph(_graph);
        Console.WriteLine("\n\n\n                 **Новий орграф** \n\n\n  ");
        DisplayMatrix(_graph);
        Console.WriteLine("\n\n ");
        foreach (var vertex in _graph.Vertices)
        {
            Console.WriteLine($"Півстепінь виходу для Vertex № {vertex.CurrentId} = {vertex.Edges.Count}");
            Console.WriteLine($"Півстепінь заходу для Vertex № {vertex.CurrentId} = {_graph.Edges.Count(p => p.To == vertex)}");
        }
        
        var squareMatrix = Matrix.Pow(_graph.GetMatrix(), 2);
        
        var cubeMatrix = Matrix.Pow(_graph.GetMatrix(), 3);
        
        List<LinkedList<int>> doublePaths = GetEnds(squareMatrix);
        List<LinkedList<int>> triplePaths = GetEnds(cubeMatrix);
        
        foreach (var vertexes in doublePaths)
        {
            for (var i = 0; i < _graph.GetMatrix().GetLength(1); i++)
            {
                if (_graph.GetMatrix()[vertexes.First.Value - 1, i] == 0 ||
                    _graph.GetMatrix()[i, vertexes.Last.Value - 1] != 1 || vertexes.Contains(i + 1)) continue;
                vertexes.AddAfter(vertexes.First, i + 1);
                break;
            }
        }
        
        foreach (var vertexes in triplePaths)
        {
            for (var i = 0; i < _graph.GetMatrix().GetLength(1); i++)
            {
                if (_graph.GetMatrix()[vertexes.First.Value - 1, i] == 0) continue;
                for (var j = 0; j < _graph.GetMatrix().GetLength(1); j++)
                {
                    if (_graph.GetMatrix()[i, j] != 0 && _graph.GetMatrix()[j, vertexes.Last.Value - 1] != 0 && 
                        !vertexes.Contains(i + 1) && !vertexes.Contains(j + 1) && i != j)
                    {
                        vertexes.AddAfter(vertexes.First, i + 1);
                        vertexes.AddAfter(vertexes.First.Next, j + 1);
                        break;
                    }
                }
            }
        }
        
        ExcessRemoving(triplePaths);
        ExcessRemoving(doublePaths);
        
        Console.WriteLine("      **шляхи довжиною 2**    ");
        
        foreach (var item in doublePaths)
        {
            Console.WriteLine(item.First.Value + " - " + item.First.Next.Value + " - " + item.Last.Value);
        }
        Console.WriteLine("      **шляхи довжиною 3**    ");
        
        foreach (var item in triplePaths)
        {
            Console.WriteLine(item.First.Value + " - " + item.First.Next.Value + " - " + item.Last.Previous.Value + " - " + item.Last.Value);
        }

        Console.WriteLine("\n          **Матриця досяжності** \n  ");
        
        var reachableMatrix = GetReachableMatrix();
        
        for (int i = 0;i<reachableMatrix.GetLength(1);i++)
        {
            for (int z = 0;z<reachableMatrix.GetLength(0);z++)
            {
                Console.Write($"{reachableMatrix[i,z]}, ");
            }
            Console.WriteLine();
        }

        Console.WriteLine("\n          **Матриця сильної зв'язності** \n  ");
        var strongConnectivitymatrix = Matrix.GetStronglyConnectivityMatrix(reachableMatrix);
        
        for (int i = 0;i<reachableMatrix.GetLength(1);i++)
        {
            for (int z = 0;z<reachableMatrix.GetLength(0);z++)
            {
                Console.Write($"{strongConnectivitymatrix[i,z]}, ");
            }
            Console.WriteLine();
        }
        
        Console.WriteLine("\n          **Компоненти сильної зв'язності** \n  ");
        _result = FindEqualRows(reachableMatrix);
        
        for (int i = 0; i < _result.Count; i++)
        {
            Console.Write("Компонента №" + (i + 1) + ": ");
            for (int j = 0; j < _result[i].Count; j++)
            {
                Console.Write(_result[i][j] + ", ");
            }
            Console.WriteLine();
        }
        
        var button2 = new Button
        {
            Content = "Намалювати граф " + Environment.NewLine + "    конденсації",
            Width = 115,
            Height = 50,
        };
        button2.Click += Button_Click2;
        Canvas.SetLeft(button2, 50);
        Canvas.SetTop(button2, 50);
        Canvas.Children.Add(button2);
    }

    private async void Button_Click2(object sender, RoutedEventArgs e)
    {
        await Task.Delay(500);
        Canvas.Children.RemoveRange(0,Canvas.Children.Count);
        Graph condensationGraph = new Graph(true){isCondensation = true};
        for (int i = 0;i<_result.Count;i++)
        {
            condensationGraph.AddVertex(i+1);
            if(i>0) condensationGraph.AddEdge(condensationGraph.Vertices.First(p=>p.Value == i), condensationGraph.Vertices.First(p=>p.Value == i+1));
        }
        
        DrawGraph(condensationGraph);
    }
    
    static List<List<int>> FindEqualRows(int[,] matrix)
    {
        List<List<int>> equalRows = new List<List<int>>();
        HashSet<string> visited = new HashSet<string>();

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            List<int> currentEqualRows = new List<int>();

            if (visited.Contains(i.ToString()))
                continue;

            currentEqualRows.Add(i + 1);

            for (int j = i + 1; j < matrix.GetLength(0); j++)
            {
                bool isEqual = true;

                for (int k = 0; k < matrix.GetLength(1); k++)
                {
                    if (matrix[i, k] != matrix[j, k])
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                {
                    currentEqualRows.Add(j + 1);
                    visited.Add(j.ToString());
                }
            }

            if (currentEqualRows.Count > 1)
                equalRows.Add(currentEqualRows);
        }

        return equalRows;
    }
    
    private int[,] GetReachableMatrix()
    {
        var matrix1 = _graph.GetMatrix();
        var matrix2 = Matrix.Pow(matrix1, 2);
        var matrix3 = Matrix.Pow(matrix1, 3);
        var matrix4 = Matrix.Pow(matrix1, 4);
        var matrix5 = Matrix.Pow(matrix1, 5);
        var matrix6 = Matrix.Pow(matrix1, 6);
        var matrix7 = Matrix.Pow(matrix1, 7);
        var matrix8 = Matrix.Pow(matrix1, 8);
        var matrix9 = Matrix.Pow(matrix1, 9);
        var matrix10 = Matrix.Pow(matrix1, 10);
        var matrix11 = Matrix.Pow(matrix1, 11);
        
        List<int[,]> matrices = new List<int[,]>
        {
            matrix1, matrix2, matrix3, matrix4, matrix5, matrix6, matrix7, matrix8, matrix9, matrix10, matrix11,
        };
        
        int[,] reachabilityMatrix = new int[matrix1.GetLength(1), matrix1.GetLength(1)];
        
        for (int i = 0;i<reachabilityMatrix.GetLength(1);i++)
        {
            for (int z = 0;z<reachabilityMatrix.GetLength(0);z++)
            {
                for (int k = 0;k<matrices.Count;k++)
                {
                    if (matrices[k][i,z] != 0)
                    {
                        reachabilityMatrix[i, z] = 1;
                        break;
                    }
                }
            }
        }

        return reachabilityMatrix;
    }
    
    private static void ExcessRemoving(List<LinkedList<int>> list)
    {
        for (var i = 1;i<list.Count;i++)
        {
            for (var z = 0;z<i;z++)
            {
                if (list[i].SequenceEqual(list[z]))
                {
                    list.RemoveAt(i);
                    z--;
                }
            }
        }
        list.RemoveAll(subList => subList.Count != 3 && subList.Count != 4);
    }

    private static List<LinkedList<int>> GetEnds(int[,] matrix)
    {
        List<LinkedList<int>> doublePaths = new List<LinkedList<int>>();
        for (var i = 0;i<matrix.GetLength(0);i++)
        {
            for (var j = 0;j<matrix.GetLength(1);j++)
            {
                if (matrix[i, j] == 0) continue;
                for (var z = 1; z <= matrix[i, j]; z++)
                {
                    var linkedList = new LinkedList<int>();
                    linkedList.AddLast(i + 1);
                    linkedList.AddLast(j + 1);
                    doublePaths.Add(linkedList);
                }
            }
        }

        return doublePaths;
    }
    
    private void DisplayMatrix(Graph graph)
    {
        var matrix = graph.GetMatrix();
        
        for (var i = 0; i < matrix.GetLength(0); i++)
        {
            for (var j = 0; j < matrix.GetLength(1); j++)
            {
                Console.Write(matrix[i, j] + "  ");
            }
            Console.WriteLine();
        }
    }
    
    private void ArrangeVerticesInCircle(double centreX,double centreY,int radius, Dictionary<Vertex, Coordinates> dictionary,Graph graph)
    {
        string? k = null;
        if (graph.isCondensation) k = "K";
        
        var angleIncrement = 360 / graph.Vertices.Count;
        for (var i = 0; i < graph.Vertices.Count; i++)
        {
            double angle = i * angleIncrement;
            var angleRad = angle * Math.PI / 180; 

            var x = centreX + radius * Math.Cos(angleRad);
            var y = centreY + radius * Math.Sin(angleRad);

            var coordinates = new Coordinates(x, y);
            dictionary.Add(graph.Vertices.First(p=>p.CurrentId == i+1), coordinates);
            DrawVertex(coordinates, graph.Vertices.First(p=>p.CurrentId == i+1), k);
        }
    }
    
    private void DrawVertex(Coordinates coordinates, Vertex vertex, string k)
    {
        var ellipse = new Ellipse
        {
            Width = 30,
            Height = 30,
            Fill = Brushes.LightBlue,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
        
        Canvas.SetLeft(ellipse, coordinates.X - 15);
        Canvas.SetTop(ellipse, coordinates.Y - 15);
        Canvas.Children.Add(ellipse);
        
        var textBlock = new TextBlock
        {
            Text = k + vertex.Value.ToString(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Canvas.Children.Add(textBlock);
        
        Canvas.SetLeft(textBlock, (coordinates.X + ellipse.Width / 2 - textBlock.ActualWidth / 2)-19);
        Canvas.SetTop(textBlock, (coordinates.Y + ellipse.Height / 2 - textBlock.ActualHeight / 2)-24);
    }

    private void DrawEdge(Edge edge, Dictionary<Vertex, Coordinates> dictionary)
    {
        if (edge.From.CurrentId == edge.To.CurrentId)
        {
            var ellipse = new Ellipse()
            {
                Width = 15,
                Height = 15,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            
            const double koef = 322.5 / 300;
        
            Canvas.SetLeft(ellipse, (683 +(dictionary[edge.To].X-683)*koef)-7.5);
            Canvas.SetTop(ellipse, (352 + (dictionary[edge.To].Y-352)*koef)-7.5);
            Canvas.Children.Add(ellipse);
            return;
        }
        
        var lineLength = Math.Sqrt(Math.Pow((dictionary[edge.To].X - dictionary[edge.From].X), 2) +
                                   Math.Pow(dictionary[edge.To].Y - dictionary[edge.From].Y, 2));
        var k = 15 / lineLength;

        
        if (_graph.IsDirected && _graph.Edges.Any( z => z.To == edge.From && z.From == edge.To))
        {
                var path = new Path();
                path.Stroke = Brushes.Black;
                path.StrokeThickness = 2;
                var startPoint = new Point(dictionary[edge.From].X + k*(dictionary[edge.To].X - dictionary[edge.From].X), dictionary[edge.From].Y + k*(dictionary[edge.To].Y - dictionary[edge.From].Y));
                var endPoint = new Point(dictionary[edge.To].X + k*(dictionary[edge.From].X - dictionary[edge.To].X), dictionary[edge.To].Y + k*(dictionary[edge.From].Y - dictionary[edge.To].Y));
                var center = new Point((startPoint.X + endPoint.X)/2, (startPoint.Y + endPoint.Y)/2);
                var dx = endPoint.X - startPoint.X;
                var dy = startPoint.Y - endPoint.Y;
                var length = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                var xGrow = dx / length;
                var yGrow = dy / length;
                
                var middlePoint = new Point(center.X - (50 * yGrow), center.Y - (50*xGrow));
                
                var pathGeometry = new PathGeometry();
                
                var pathFigure = new PathFigure();
                
                pathFigure.StartPoint = startPoint; 
            
                var bezierSegment = new BezierSegment(
                    startPoint,
                    middlePoint,
                    endPoint,
                    true);
                
                pathFigure.Segments.Add(bezierSegment);
                pathGeometry.Figures.Add(pathFigure);
                path.Data = pathGeometry;
                Canvas.Children.Add(path);
        }

        else
        {
            var line = new Line
            {
                X1 = dictionary[edge.From].X + k * (dictionary[edge.To].X - dictionary[edge.From].X),
                Y1 = dictionary[edge.From].Y + k * (dictionary[edge.To].Y - dictionary[edge.From].Y),
                X2 = dictionary[edge.To].X + k * (dictionary[edge.From].X - dictionary[edge.To].X),
                Y2 = dictionary[edge.To].Y + k * (dictionary[edge.From].Y - dictionary[edge.To].Y),
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            Canvas.Children.Add(line);
        }

        if (!_graph.IsDirected) return;
        {
            var y2 = dictionary[edge.To].Y + k * (dictionary[edge.From].Y - dictionary[edge.To].Y);
            var y1 = dictionary[edge.From].Y + k * (dictionary[edge.To].Y - dictionary[edge.From].Y);
            var x1 = dictionary[edge.From].X + k * (dictionary[edge.To].X - dictionary[edge.From].X);
            var x2 = dictionary[edge.To].X + k * (dictionary[edge.From].X - dictionary[edge.To].X);
            
            var angle = Math.Atan2(y2 - y1, x2 - x1) * (180 / Math.PI);
            if (_graph.IsDirected && _graph.Edges.Any(z => z.To == edge.From && z.From == edge.To))
            {
                angle -= 15;
            }
            
            var arrowhead = new Polygon
            {
                Points = new PointCollection { new Point(x2, y2), new Point(x2 - 10, y2 - 5), new Point(x2 - 10, y2 + 5) }, // Треугольник
                Fill = Brushes.Black, 
                StrokeThickness = 0,
                RenderTransform = new RotateTransform(angle, x2, y2)
            };

            Canvas.Children.Add(arrowhead);
        }
    }

    private bool IsRegular()
    {
        for (var i = 1;i<_graph.Vertices.Count;i++)
        {
            if (_graph.Vertices.First(p => p.CurrentId == i).Edges.Count != _graph.Vertices.First(p=>p.CurrentId == i+1).Edges.Count)
            {
                return false;
            }
        }

        return true;
    }

    private void DrawGraph(Graph graph)
    {
        if (!graph.isCondensation)
        {
            graph.GenerateMatrix();
        }
        
        var dictionary = new Dictionary<Vertex, Coordinates>();
        
        ArrangeVerticesInCircle(683, 352, 300, dictionary, graph);
        
        foreach (var edge in graph.Vertices.SelectMany(vertex => vertex.Edges))
        {
            DrawEdge(edge, dictionary);
        }
    }
    
    private void Calculations()
    {
        foreach (var vertex in _graph.Vertices)
        {
            var edgesNum = vertex.Edges.Count;
            if (_graph.IsDirected)
            {
                edgesNum += _graph.Edges.Count(p=>p.To == vertex && p.From != p.To);
            }
            Console.WriteLine($"Вершина № {vertex.CurrentId} має степінь {edgesNum}");
        }

        if (_graph.IsDirected)
        {
            foreach (var vertex in _graph.Vertices)
            {
                Console.WriteLine($"Півстепінь виходу для Vertex № {vertex.CurrentId} = {vertex.Edges.Count}");
                Console.WriteLine($"Півстепінь заходу для Vertex № {vertex.CurrentId} = {_graph.Edges.Count(p => p.To == vertex)}");
            }
        }

        Console.WriteLine(IsRegular()
            ? $"Степінь однорідності графа = {_graph.Vertices[0].Edges.Count}"
            : "Граф є не однорідним");

        var reclinedVertexes = _graph.Vertices.Where(p => p.Edges.Count is 0 or 1);
        foreach (var rec in reclinedVertexes)
        {
            Console.WriteLine(rec.Edges.Count == 0
                ? $"Вершина № {rec.CurrentId} є висячою"
                : $"Вершина № {rec.CurrentId} є тупиковою");
        }
    }
}

public readonly struct Coordinates(double x, double y)
{
    public double X { get; } = x;

    public double Y { get; } = y;
}