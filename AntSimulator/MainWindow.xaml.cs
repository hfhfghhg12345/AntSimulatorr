
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AntSimulator
{
    public partial class MainWindow : Window
    {
        // Размер одной клетки в пикселях
        private const int CellSize = 24;

        // Игровое поле: false — белая клетка, true — чёрная
        private bool[,] cells = new bool[20, 30];

        // Размер игрового поля
        private int rows = 20;
        private int cols = 30;

        // Координаты муравья
        private int antX;
        private int antY;

        // Направление: 0 — вверх, 1 — вправо,
        // 2 — вниз, 3 — влево
        private int direction = 0;

        // Количество шагов
        private int steps = 0;

        // Таймер автоматического движения
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(120);
            timer.Tick += Timer_Tick;

            GenerateField();
        }

        // Создание игрового поля
        private void GenerateField()
        {
            timer.Stop();

            cells = new bool[rows, cols];

            // Муравей появляется в центре
            antX = cols / 2;
            antY = rows / 2;

            // Начальное направление — вверх
            direction = 0;

            steps = 0;

            DrawField();
            UpdateStatus();
        }

        // Кнопка генерации поля
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WidthBox.Text, out int width) ||
                !int.TryParse(HeightBox.Text, out int height) ||
                width < 5 || width > 60 ||
                height < 5 || height > 60)
            {
                MessageBox.Show(
                    "Введите ширину и высоту от 5 до 60 клеток.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            cols = width;
            rows = height;

            GenerateField();
        }

        // Запуск симуляции
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        // Пауза
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        // Один шаг
        private void Step_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            MakeStep();
        }

        // Событие таймера
        private void Timer_Tick(object? sender, EventArgs e)
        {
            MakeStep();
        }

        // Выполнение одного шага муравья

        private void MakeStep()
        {
            // Запоминаем цвет клетки, на которой находится муравей
            bool isBlack = cells[antY, antX];

            // Выбираем правило в зависимости от цвета клетки
            int rule = isBlack
                ? BlackRuleBox.SelectedIndex
                : WhiteRuleBox.SelectedIndex;

            switch (rule)
            {
                case 0:
                    direction = (direction + 1) % 4;
                    break;

                case 1:
                    direction = (direction + 3) % 4;
                    break;

                case 2:
                    break;

                case 3:
                    break;
            }

            // Меняем цвет клетки после выполнения правила
            cells[antY, antX] = !cells[antY, antX];

            // Перемещаем муравья
            MoveAnt();

            steps++;

            DrawField();
            UpdateStatus();
        }


        // Перемещение муравья
        private void MoveAnt()
        {
            switch (direction)
            {
                case 0:
                    antY--;
                    break;

                case 1:
                    antX++;
                    break;

                case 2:
                    antY++;
                    break;

                case 3:
                    antX--;
                    break;
            }

            // Если муравей вышел за границы поля,
            // он появляется с противоположной стороны
            antX = (antX + cols) % cols;
            antY = (antY + rows) % rows;
        }

        // Отрисовка игрового поля
        private void DrawField()
        {
            GameCanvas.Children.Clear();

            GameCanvas.Width = cols * CellSize;
            GameCanvas.Height = rows * CellSize;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Rectangle cell = new Rectangle
                    {
                        Width = CellSize - 1,
                        Height = CellSize - 1,

                        Fill = cells[y, x]
                            ? Brushes.Black
                            : Brushes.White,

                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1
                    };

                    Canvas.SetLeft(cell, x * CellSize);
                    Canvas.SetTop(cell, y * CellSize);

                    GameCanvas.Children.Add(cell);
                }
            }

            DrawAnt();
        }

        // Отрисовка муравья
        private void DrawAnt()
        {
            Ellipse ant = new Ellipse
            {
                Width = 16,
                Height = 16,
                Fill = Brushes.Crimson,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 1.5
            };

            Canvas.SetLeft(ant, antX * CellSize + 4);
            Canvas.SetTop(ant, antY * CellSize + 4);

            GameCanvas.Children.Add(ant);

            // Стрелка показывает направление движения
            Polygon arrow = new Polygon
            {
                Fill = Brushes.Yellow,
                Points = GetDirectionPoints()
            };

            Canvas.SetLeft(arrow, antX * CellSize);
            Canvas.SetTop(arrow, antY * CellSize);

            GameCanvas.Children.Add(arrow);
        }

        // Координаты стрелки в зависимости от направления
        private PointCollection GetDirectionPoints()
        {
            switch (direction)
            {
                case 0:
                    return new PointCollection
                    {
                        new Point(12, 2),
                        new Point(7, 10),
                        new Point(17, 10)
                    };

                case 1:
                    return new PointCollection
                    {
                        new Point(22, 12),
                        new Point(14, 7),
                        new Point(14, 17)
                    };

                case 2:
                    return new PointCollection
                    {
                        new Point(12, 22),
                        new Point(7, 14),
                        new Point(17, 14)
                    };

                default:
                    return new PointCollection
                    {
                        new Point(2, 12),
                        new Point(10, 7),
                        new Point(10, 17)
                    };
            }
        }

        // Обновление счётчика шагов
        private void UpdateStatus()
        {
            StatusText.Text = $"Шагов: {steps}";
        }
    }
}
