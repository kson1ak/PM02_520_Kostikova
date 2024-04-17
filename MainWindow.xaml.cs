using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace Transport_problem_solver
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Проверка, является ли строка числом
        private bool IsNumeric(string input)
        {
            return int.TryParse(input, out _);
        }

        // Проверка, заполнено ли поле
        private bool IsFilled(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        // Проверка валидности всех введенных значений
        private bool ValidateInput(string[] inputs)
        {
            foreach (string input in inputs)
            {
                if (!IsFilled(input) || !IsNumeric(input))
                {
                    return false;
                }
            }
            return true;
        }

        private void SolveByNWCorner(object sender, RoutedEventArgs e)
        {
            string[] demandInputs = txtConsumerDemand.Text.Split(',');
            string[] supplyInputs = txtSupplierSupply.Text.Split(',');
            string[] costRows = txtCostMatrix.Text.Split(';');

            if (!ValidateInput(demandInputs) || !ValidateInput(supplyInputs) || costRows.Any(row => !ValidateInput(row.Split(','))))
            {
                if (!ValidateInput(demandInputs) || !ValidateInput(supplyInputs))
                {
                    MessageBox.Show("Введите числовые значения в поля спроса и предложения.");
                }
                else
                {
                    MessageBox.Show("Убедитесь, что все поля матрицы стоимостей заполнены числовыми значениями.");
                }
                return;
            }

            int[] demand = Array.ConvertAll(demandInputs, int.Parse);
            int[] supply = Array.ConvertAll(supplyInputs, int.Parse);

            int[,] costMatrix = new int[costRows.Length, demand.Length];
            for (int i = 0; i < costRows.Length; i++)
            {
                string[] costs = costRows[i].Split(',');
                for (int j = 0; j < costs.Length; j++)
                {
                    costMatrix[i, j] = int.Parse(costs[j]);
                }
            }

            var (allocation, totalCost) = NorthwestCornerMethod(supply, demand, costMatrix);
            DisplayResult(allocation, totalCost);
        }

        private void SolveByMinimumCost(object sender, RoutedEventArgs e)
        {
            string[] demandInputs = txtConsumerDemand.Text.Split(',');
            string[] supplyInputs = txtSupplierSupply.Text.Split(',');
            string[] costRows = txtCostMatrix.Text.Split(';');

            if (!ValidateInput(demandInputs) || !ValidateInput(supplyInputs) || costRows.Any(row => !ValidateInput(row.Split(','))))
            {
                if (!ValidateInput(demandInputs) || !ValidateInput(supplyInputs))
                {
                    MessageBox.Show("Введите числовые значения в поля спроса и предложения.");
                }
                else
                {
                    MessageBox.Show("Убедитесь, что все поля матрицы стоимостей заполнены числовыми значениями.");
                }
                return;
            }

            int[] demand = Array.ConvertAll(demandInputs, int.Parse);
            int[] supply = Array.ConvertAll(supplyInputs, int.Parse);

            int[][] costMatrix = new int[costRows.Length][];
            for (int i = 0; i < costRows.Length; i++)
            {
                costMatrix[i] = costRows[i].Split(',').Select(int.Parse).ToArray();
            }

            var (allocation, totalCost) = MinimumCostMethod(supply, demand, costMatrix);
            DisplayResult(allocation, totalCost);
        }

        private (int[][], int) NorthwestCornerMethod(int[] supply, int[] demand, int[,] costs)
        {
            int rows = supply.Length;
            int cols = demand.Length;
            int[][] allocation = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                allocation[i] = new int[cols];
            }

            int[] supplyCopy = (int[])supply.Clone();
            int[] demandCopy = (int[])demand.Clone();
            int totalCost = 0;

            int row = 0, col = 0;
            while (row < rows && col < cols)
            {
                int x = Math.Min(supplyCopy[row], demandCopy[col]);
                allocation[row][col] = x;
                supplyCopy[row] -= x;
                demandCopy[col] -= x;
                totalCost += x * costs[row, col];

                if (supplyCopy[row] == 0)
                {
                    row++;
                }
                else
                {
                    col++;
                }
            }

            return (allocation, totalCost);
        }

        static (int[][], int) MinimumCostMethod(int[] supply, int[] demand, int[][] costs)
        {
            int[][] allocation = new int[supply.Length][];
            for (int i = 0; i < supply.Length; i++)
            {
                allocation[i] = new int[demand.Length];
            }

            int[] supplyCopy = supply.ToArray();
            int[] demandCopy = demand.ToArray();
            int totalCost = 0;

            while (true)
            {
                int minCost = int.MaxValue;
                int minRow = -1, minCol = -1;

                for (int row = 0; row < supply.Length; row++)
                {
                    for (int col = 0; col < demand.Length; col++)
                    {
                        if (supplyCopy[row] > 0 && demandCopy[col] > 0)
                        {
                            if (costs[row][col] < minCost)
                            {
                                minCost = costs[row][col];
                                minRow = row;
                                minCol = col;
                            }
                        }
                    }
                }

                if (minRow == -1 || minCol == -1)
                {
                    break;
                }

                int x = Math.Min(supplyCopy[minRow], demandCopy[minCol]);
                allocation[minRow][minCol] = x;
                supplyCopy[minRow] -= x;
                demandCopy[minCol] -= x;
                totalCost += x * minCost;
            }

            return (allocation, totalCost);
        }

        private void DisplayResult(int[][] allocation, int totalCost)
        {
            StringBuilder resultBuilder = new StringBuilder();

            // Добавляем строку с распределением
            resultBuilder.AppendLine("Опорный план:");
            for (int i = 0; i < allocation.Length; i++)
            {
                for (int j = 0; j < allocation[i].Length; j++)
                {
                    resultBuilder.Append(allocation[i][j]);
                    resultBuilder.Append("\t");
                }
                resultBuilder.AppendLine();
            }

            // Добавляем строку с общей стоимостью
            resultBuilder.AppendLine($"Общая стоимость: {totalCost}");

            txtSolution.Text = resultBuilder.ToString();
        }

        private void ClearFields(object sender, RoutedEventArgs e)
        {
            txtSupplierSupply.Clear();
            txtConsumerDemand.Clear();
            txtCostMatrix.Clear();
        }

        private void ClearSolution(object sender, RoutedEventArgs e)
        {
            txtSolution.Clear();
        }
    }
}