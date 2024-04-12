namespace MatrixLibrary;

public static class Matrix
{
    public static int[,] Pow(int[,] matrix, int degree)
    {
        var result = new int[matrix.GetLength(0), matrix.GetLength(1)];

        Array.Copy(matrix, result, matrix.Length);

        if (degree == 1)
            return result;

        for (var d = 2; d <= degree; d++)
        {
            var temp = new int[matrix.GetLength(0), matrix.GetLength(1)];

            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    var sum = 0;
                    for (var k = 0; k < matrix.GetLength(1); k++)
                    {
                        sum += matrix[i, k] * result[k, j];
                    }
                    temp[i, j] = sum;
                }
            }
            
            Array.Copy(temp, result, temp.Length);
        }

        return result;
    }

    private static int[,] Transpose(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        int[,] transposedMatrix = new int[columns, rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                transposedMatrix[j, i] = matrix[i, j];
            }
        }

        return transposedMatrix;
    }

    public static int[,] GetStronglyConnectivityMatrix(int[,] matrix)
    {
        int[,] newMatrix = new int[matrix.GetLength(1),matrix.GetLength(0)];
        int[,] transposeMatrix = Transpose(matrix);
        for (int i = 0;i<matrix.GetLength(1);i++)
        {
            for (int z = 0;z<matrix.GetLength(0);z++)
            {
                newMatrix[i, z] = matrix[i, z] * transposeMatrix[i, z];
            }
        }

        return newMatrix;
    }
}