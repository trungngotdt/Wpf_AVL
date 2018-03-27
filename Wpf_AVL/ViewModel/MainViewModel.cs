using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Wpf_AVL.Model;

namespace Wpf_AVL.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        private double widthGridBST;
        private int nodeAdd;
        private int numFind;
        private int nodeDelete;
        private AVLTree<int> tree;


        private readonly int VerticalMarging = 100;
        private readonly int HorizontalMarging = 50;
        private double heightGridBST;
        private ICommand btnAddNodeClickCommand;
        //private ICommand bSTGridSizeChanged;
        private ICommand btnFindNodeClickCommand;
        private ICommand btnDeleteNodeClickCommand;
        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }
            set
            {
                Set(ref _welcomeTitle, value);
            }
        }

        public int NumBeDelete { get => nodeDelete; set => nodeDelete = value; }
        public int NodeAdd { get => numFind; set => numFind = value; }
        public int NodeFind { get => nodeAdd; set => nodeAdd = value; }
        public double WidthGridBST { get => widthGridBST; set => widthGridBST = value; }
        public double HeightGridBST { get => heightGridBST; set => heightGridBST = value; }
        public ICommand BtnAddNodeClickCommand
        {
            get
            {
                return btnAddNodeClickCommand = new RelayCommand<object>((p) =>
                {
                    if ((NodeAdd == null) || (p as Grid).Children.OfType<Button>().Where(pa => pa.Content.Equals($"Btn{NodeAdd.ToString()}")).ToList().Count != 0)
                    {
                        MessageBox.Show("Test");
                        return;
                    }
                    AddButtonGridAsync(p as Grid);
                });
            }
        }
        public ICommand BtnFindNodeClickCommand { get => btnFindNodeClickCommand; set => btnFindNodeClickCommand = value; }
        public ICommand BtnDeleteNodeClickCommand
        {
            get
            {
                return btnDeleteNodeClickCommand = new RelayCommand<Grid>((p) =>
                {
                    if ((NumBeDelete == null) || (p as Grid).Children.OfType<Button>().Where(pa => pa.Name.Equals($"Btn{NumBeDelete.ToString()}")).ToList().Count == 0)
                    {
                        MessageBox.Show($"We can't delete {NumBeDelete.ToString()}");
                        return;
                    }
                    DeleteNodeInGridAsync(p, NumBeDelete);
                });
            }
        }
        public AVLTree<int> Tree { get => tree; set => tree = value; }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            tree = new AVLTree<int>();
            HeightGridBST = 705;
            WidthGridBST = 1138.3333333333335;
        }


        #region Add a node to grid

        #region Draw line

        /// <summary>
        /// Draw a line from the button to the other button
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="isRightLeaf"></param>
        /// <param name="name"></param>
        private void DrawLine(Grid grid, double x1, double x2, double y1, double y2, bool isRightLeaf, string name)
        {
            Line l = new Line
            {
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2.0,
                Name = name,
                X1 = x1 + (isRightLeaf ? 50 : 0),
                X2 = x1 + (isRightLeaf ? 50 : 0), //x2 + (isRightLeaf ? 0 : 50),
                Y1 = y1 + 25, //btn1Point.Y + a.ActualHeight / 2;
                Y2 = y1 + 25
            };
            AnimationGrowLine(x2 + (isRightLeaf ? 0 : 50), y2 + 25, TimeSpan.FromSeconds(1), l);
            grid.Children.Add(l);
        }



        /// <summary>
        /// Grow a line to x2,y2
        /// </summary>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="time">The time the line will be grow</param>
        /// <param name="line"></param>
        public void AnimationGrowLine(double x2, double y2, TimeSpan time, Line line)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation animationX2 = new DoubleAnimation(x2, time);
            DoubleAnimation animationY2 = new DoubleAnimation(y2, time);
            Storyboard.SetTarget(animationX2, line);
            Storyboard.SetTargetProperty(animationX2, new PropertyPath("X2"));
            Storyboard.SetTarget(animationY2, line);
            Storyboard.SetTargetProperty(animationY2, new PropertyPath("Y2"));
            sb.Children.Add(animationX2);
            sb.Children.Add(animationY2);
            sb.Begin();
            /*line.BeginAnimation(Line.X2Property, animationX2);
            line.BeginAnimation(Line.Y2Property, animationY2);*/
        }
        #endregion


        /// <summary>
        /// To calculate new position and button will be added to Grid
        /// </summary>
        /// <param name="p">this is a grid which will be add a button</param>
        private async void AddButtonGridAsync(UIElement p)
        {
            double x = 0;
            double y = 0;
            if ((p as Grid).Children.OfType<Button>().Count<Button>() == 0)
            {
                Tree.Insert(new Node<int>(NodeAdd, (p as Grid).ActualWidth / 2, VerticalMarging));
                AddNode(p as Grid, NodeAdd, new Thickness((p as Grid).ActualWidth / 2, VerticalMarging, 0, 0));
                return;
            }
            if (Tree.Contains(new Node<int>(NodeAdd)))
            {
                return;
            }
            var node = new Node<int>(NodeAdd, x, y);

            Tree.InsertNoRotation(node);
            var checkExitsParent = Tree.FindParent(new Node<int>(NodeAdd));
            if (checkExitsParent.Item1 != null)
            {
                var oldX = checkExitsParent.Item1.X;
                var oldY = checkExitsParent.Item1.Y;
                if (checkExitsParent.Item2 == -1)
                {
                    x = oldX - (p as Grid).ActualWidth / Math.Pow(2, ((oldY + VerticalMarging) / VerticalMarging));
                    y = oldY + VerticalMarging;
                    AddNode(p as Grid, NodeAdd, new Thickness(x, y, 0, 0));
                }
                else if (checkExitsParent.Item2 == 1)
                {
                    x = oldX + (p as Grid).ActualWidth / Math.Pow(2, ((oldY + VerticalMarging) / VerticalMarging));
                    y = oldY + VerticalMarging;
                    AddNode(p as Grid, NodeAdd, new Thickness(x, y, 0, 0));
                }
                node.X = x;
                node.Y = y;
                if (x + 50 >= WidthGridBST || x - 50 <= 0)
                {
                    ResizeGrid();
                    await ReLayoutAllButtonAsync(p as Grid);
                }
                DrawLine(p as Grid, checkExitsParent.Item1.X, node.X, checkExitsParent.Item1.Y, node.Y, checkExitsParent.Item2 > 0, $"{"Btn" + checkExitsParent.Item1.Data.ToString() + "Btn" + node.Data.ToString() }");

                Tree.root = await FindAndRotationAsync(Tree.root, new Node<int>(NodeAdd, x, y), (p as Grid));
            }

        }

        public async Task<Node<int>> FindAndRotationAsync(Node<int> x, Node<int> key, UIElement p)
        {
            if (x.Data.Equals(key.Data))
            {
                Node<int> node = new Node<int>(key.Data, key.X, key.Y);
                return node;
            }

            int cmp = key.CompareTo(x);
            if (cmp < 0)
                x.Left = await FindAndRotationAsync(x.Left, key, (p as Grid));
            else if (cmp > 0)
                x.Right = await FindAndRotationAsync(x.Right, key, (p as Grid));
            x = await BalanceAsync(x, (p as Grid));

            return x;
        }

        #region Balance
        /// <summary>
        /// Keeping tree's balance
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private async Task<Node<int>> BalanceAsync(Node<int> x, UIElement p)
        {
            if (CheckBalance(x) < -1)
            {
                if (CheckBalance(x.Right) > 0)
                {
                    (p as Grid).Children.Remove((p as Grid).Children.OfType<Line>().FirstOrDefault(l => Regex.Split(l.Name, "Btn")[2] == x.Right.Left.Data.ToString()));
                    x.Right = await RotateRightAsync(x.Right, (p as Grid));
                }
                x = await RotateLeftAsync(x, (p as Grid));
            }
            else if (CheckBalance(x) > 1)
            {
                if (CheckBalance(x.Left) < 0)
                {
                    (p as Grid).Children.Remove((p as Grid).Children.OfType<Line>().FirstOrDefault(l => Regex.Split(l.Name, "Btn")[2] == x.Left.Right.Data.ToString()));
                    x.Left = await RotateLeftAsync(x.Left, (p as Grid));
                }
                x = await RotateRightAsync(x, (p as Grid));
            }
            return x;
        }

        /// <summary>
        /// Checking the tree is balance or nor
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private int CheckBalance(Node<int> x)
        {
            return Tree.Height(x.Left) - Tree.Height(x.Right);
        }

        private async Task<Node<int>> RotateLeftAsync(Node<int> x, UIElement p)
        {
            Node<int> y = x.Right;
            x.Right = y.Left;
            y.Left = x;
            y.X = x.X;
            y.Y = x.Y;
            Node<int> father = null;//Tree.FindParent(y)?.Item1;
            if (y.Left != null)
            {

                var colectionLine = (p as Grid).Children.OfType<Line>().ToArray();
                for (int i = 0; i < colectionLine.Length; i++)
                {

                    if (y.Left.Data.ToString() == Regex.Split(colectionLine[i].Name, "Btn")[2])
                    {
                        (p as Grid).Children.Remove(colectionLine[i]);
                        colectionLine[i].Name = $"Btn{Regex.Split(colectionLine[i].Name, "Btn")[1]}Btn{y.Data}";
                        (p as Grid).Children.Add(colectionLine[i]);
                    }
                    else if (y.Data.ToString() == Regex.Split(colectionLine[i].Name, "Btn")[2])
                    {
                        (p as Grid).Children.Remove(colectionLine[i]);
                    }
                }
            }
            await RelayoutAfterRotateAsync(y, p, father);
            return y;

        }

        private async Task<Node<int>> RotateRightAsync(Node<int> x, UIElement p)
        {
            Node<int> y = x.Left;
            x.Left = y.Right;
            y.Right = x;
            y.X = x.X;
            y.Y = x.Y;

            Node<int> father = null;//Tree.FindParent(y)?.Item1;
            var colectionLine = (p as Grid).Children.OfType<Line>().ToArray();
            if (y.Right != null)
            {
                for (int i = 0; i < colectionLine.Length; i++)
                {
                    if (y.Right.Data.ToString() == Regex.Split(colectionLine[i].Name, "Btn")[2])
                    {
                        (p as Grid).Children.Remove(colectionLine[i]);
                        colectionLine[i].Name = $"Btn{Regex.Split(colectionLine[i].Name, "Btn")[1]}Btn{y.Data}";
                        (p as Grid).Children.Add(colectionLine[i]);
                    }
                    else if (y.Data.ToString() == Regex.Split(colectionLine[i].Name, "Btn")[2])
                    {
                        (p as Grid).Children.Remove(colectionLine[i]);
                    }
                }
            }
            await RelayoutAfterRotateAsync(y, p, father);
            return y;

        }

        private async Task RelayoutAfterRotateAsync(Node<int> node, UIElement p, Node<int> father = null, bool isRight = false)
        {
            if (node == null)
            {
                return;
            }
            if (node.Left != null)
            {
                node.Left.Y = node.Y + VerticalMarging;
                node.Left.X = node.X - (p as Grid).ActualWidth / Math.Pow(2, ((node.Y + VerticalMarging) / VerticalMarging));
            }
            if (node.Right != null)
            {
                node.Right.Y = node.Y + VerticalMarging;
                node.Right.X = node.X + (p as Grid).ActualWidth / Math.Pow(2, ((node.Y + VerticalMarging) / VerticalMarging));
            }
            if (father != null)
            {
                var colectionLine = (p as Grid).Children.OfType<Line>().ToArray();
                List<Task> list = new List<Task>();
                for (int i = 0; i < colectionLine.Length; i++)
                {
                    int j = i;
                    var task = Task.Factory.StartNew(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (node.Data.ToString() == Regex.Split(colectionLine[j].Name, "Btn")[2])
                            {
                                (p as Grid).Children.Remove(colectionLine[j]);
                                //break;
                            }
                        });
                    });
                    list.Add(task);
                }
                await Task.WhenAll(list);
                //(p as Grid).Children.Remove(FindLineInGrid(p as Grid, $"Btn{father.Data}Btn{node.Data}"));
                DrawLine(p as Grid, father.X, node.X, father.Y, node.Y, isRight, $"Btn{father.Data}Btn{node.Data}");
            }
            AnimationButtonMovetTo(node.X, node.Y, (p as Grid).Children.OfType<Button>().Where(b => b.Name == $"Btn{node.Data}").FirstOrDefault());
            Task taskLeft = Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await RelayoutAfterRotateAsync(node.Left, p, node);
                });
            });
            Task taskRight = Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await RelayoutAfterRotateAsync(node.Right, p, node, true);
                });
            });
            await Task.WhenAll(new Task[] { taskLeft, taskRight });
        }
        #endregion

        /// <summary>
        /// Add a node to Grid with location
        /// </summary>
        /// <param name="gridPanel"></param>
        /// <param name="data"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        private bool AddNode(UIElement gridPanel, int data, Thickness thickness)
        {
            try
            {
                var grid = gridPanel as Grid;
                Button button = new Button() { Name = "Btn" + data.ToString(), Content = data.ToString() };

                button.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                button.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Top);
                button.Margin = thickness;// new Thickness(1138.3 / 2, 20, 0, 0);
                button.Width = 50;
                button.Height = 50;
                RoundButton(button);
                grid.Children.Add(button);
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return false;
            }
        }

        #region Relayout for button

        /// <summary>
        /// Re-layout all buttons from grid
        /// </summary>
        /// <param name="grid"></param>
        private async Task ReLayoutAllButtonAsync(Grid grid)
        {
            List<Task> listTask = new List<Task>();
            grid.Children.OfType<Button>().ToList().ForEach(p =>
            {
                var task1 = Task.Factory.StartNew(() =>
                 {
                     Application.Current.Dispatcher.Invoke(() =>
                     {
                         var Left = p.Margin.Left * 2;
                         Tree.FindNode(new Node<int>(int.Parse(p.Content.ToString()))).X = Left;
                         p.Margin = new Thickness(Left, p.Margin.Top, p.Margin.Right, p.Margin.Bottom);
                     });
                 });
                listTask.Add(task1);
            });
            grid.Children.OfType<Line>().ToList().ForEach(p =>
            {
                Task task2 = Task.Factory.StartNew(() =>
                 {
                     Application.Current.Dispatcher.Invoke(() =>
                     {
                         var X2 = p.X2;
                         var Y2 = p.Y2;
                         var name = p.Name;//Type : Btn'firstnum'Btn'lastnum',Example :Btn1Btn2
                                           //var firstn = name.IndexOf("n");
                                           //var lastn = name.LastIndexOf("n");
                                           //var firstb = name.LastIndexOf("B");
                                  var number1 = Regex.Split(name, "Btn")[1]; //name.Substring(firstn + 1, firstb - firstn - 1);//Get first number
                                                                             //var numStr = name.Length - lastn - 1;
                                  var number2 = Regex.Split(name, "Btn")[2];//name.Substring(lastn + 1, numStr);//Get last number
                                  p.BeginAnimation(Line.X2Property, null);//Animation be removed
                                  if (int.Parse(number2) > int.Parse(number1))//Right
                                  {
                             p.X1 = p.X1 * 2 - 50;
                             p.X2 = X2 * 2;
                         }
                         else//Left
                                  {
                             p.X2 = X2 * 2 - 50;
                             p.X1 = p.X1 * 2;
                         }
                         p.BeginAnimation(Line.Y2Property, null);//Animation be removed
                                  p.Y2 = Y2;
                     });
                 });
                listTask.Add(task2);
            });
            await Task.WhenAll(listTask);

        }
        /// <summary>
        /// Resize the grid (<seealso cref="HeightGridBST"/>+100; <seealso cref="WidthGridBST"/>*2)
        /// </summary>
        private void ResizeGrid()
        {
            WidthGridBST = WidthGridBST * 2;
            HeightGridBST = HeightGridBST + 100;
            RaisePropertyChanged("WidthGridBST");
            RaisePropertyChanged("HeightGridBST");
        }

        #endregion

        /// <summary>
        /// Draw the round button
        /// </summary>
        /// <param name="button"></param>
        public void RoundButton(Button button)
        {
            ControlTemplate circleButtonTemplate = new ControlTemplate(typeof(Button));

            // Create the circle
            FrameworkElementFactory circle = new FrameworkElementFactory(typeof(Ellipse));
            circle.SetValue(Ellipse.FillProperty, Brushes.LightGreen);
            circle.SetValue(Ellipse.StrokeProperty, Brushes.Black);
            circle.SetValue(Ellipse.StrokeThicknessProperty, 1.0);
            circle.SetValue(Ellipse.FillProperty, Brushes.WhiteSmoke);

            // Create the ContentPresenter to show the Button.Content
            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(Button.ContentProperty));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Create the Grid to hold both of the elements
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
            grid.AppendChild(circle);
            grid.AppendChild(presenter);

            // Set the Grid as the ControlTemplate.VisualTree
            circleButtonTemplate.VisualTree = grid;

            // Set the ControlTemplate as the Button.Template
            button.Template = circleButtonTemplate;
        }
        #endregion

        #region Find a Node (button) in grid

        /// <summary>
        /// Function for btnFindNodeClickCommand
        /// It will find a node-button in grid and draw a circle around button 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="grid"></param>
        private void FindNodeInGrid(Node<int> node, UIElement grid)
        {
            var nodeBeFind = Tree.FindNode(node);
            if (nodeBeFind == null)
            {
                return;
            }
            var button = (grid as Grid).Children.OfType<Button>().ToList()
                .Where(p => p.Content.Equals(nodeBeFind.Data.ToString()))
                .FirstOrDefault();
            if (button == null)
            {
                return;
            }
            var point = button.TranslatePoint(new Point(0, 0), grid as Grid);
            CreateCircleAsync(point, grid);

        }

        /// <summary>
        /// Create a circle in 3 seconds
        /// </summary>
        /// <param name="point"></param>
        private async void CreateCircleAsync(Point point, UIElement grid)
        {
            Ellipse ellipse;
            await Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ellipse = new Ellipse()
                    {
                        Stroke = new SolidColorBrush(Colors.Red),
                        Width = 55,
                        Height = 55,
                        StrokeThickness = 1.0
                    };
                    (grid as Grid).Children.Add(ellipse);

                });

                //.OfType<Ellipse>()
            });
            await Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await Task.Delay(3000);
                    (grid as Grid).Children.Remove((grid as Grid).Children.OfType<Ellipse>().FirstOrDefault());
                });
            });
        }
        #endregion

        #region Delete a node (button)

        private async void DeleteNodeInGridAsync(Grid grid, int nodeDelete)
        {

            if (nodeDelete == null)
            {
                return;
            }

            var tup = FindButtonInGrid(grid, nodeDelete);
            //Task.Factory.ContinueWhenAll(tup.Result.Item1.ToArray(), p => { });
            await tup.ContinueWith(p =>
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    var nodeDe = Tree.FindNode(new Node<int>(nodeDelete));
                    if (nodeDe.Left != null && nodeDe.Right != null)
                    {
                        var successor = Tree.GetMin(nodeDe.Right);
                        var nodeSucc = Tree.FindNode(nodeDe, successor);
                        var buttonSucc = grid.Children.OfType<Button>().Where(s => s.Content.Equals(nodeSucc.Data.ToString())).FirstOrDefault();
                        AnimationButtonMovetTo(nodeDe.X, nodeDe.Y, buttonSucc);//to move a successor to new position (the button will be deleted)
                        Node<int> nodeDelPar = new Node<int>();
                        Task taskFindParent = Task.Factory.StartNew(() => { nodeDelPar = Tree.FindParent(new Node<int>(nodeDelete)).Item1; });
                        Task task = Task.Factory.StartNew(() =>
                        {
                            Application.Current.Dispatcher.Invoke(() => { UpdateButtonAfterDeleteAsync(grid, nodeSucc.Data); });
                        });
                        grid.Children.Remove(p.Result.Item2);
                        await Task.Factory.ContinueWhenAll(new Task[] { task, taskFindParent }, t =>
                        {
                            Application.Current.Dispatcher.Invoke(async () =>
                            {
                                Tree.root = await RemoveAsync(Tree.root, NumBeDelete, grid);
                            });
                            //Tree.Remove(new Node<int>(NumBeDelete));
                        });
                        grid.Children.OfType<Line>().Where(l => l.Name.Contains($"{"Btn" + nodeDelete.ToString()}")).ToList().ForEach((item) =>
                        {
                            item.Name = item.Name.Replace($"{"Btn" + nodeDelete.ToString()}", $"{"Btn" + nodeSucc.Data.ToString()}");
                        });

                    }
                    else
                    {
                        Task task = Task.Factory.StartNew(() =>
                        {
                            Application.Current.Dispatcher.Invoke(() => { UpdateButtonAfterDeleteAsync(grid, nodeDelete); });
                        });
                        await Task.Factory.ContinueWhenAll(new Task[] { task }, t =>
                        {
                            if (Tree.FindParent(new Node<int>(nodeDelete)).Item1 == null && nodeDe.Right == null && nodeDe.Left == null)
                            {
                                Tree = null;
                            }
                            else
                            {
                                Application.Current.Dispatcher.Invoke(async () =>
                                {
                                    Tree.root = await RemoveAsync(Tree.root, NumBeDelete, grid);
                                });
                            }
                        }); grid.Children.Remove(p.Result.Item2);
                    }
                    //AnimationButtonMovetTo(20, 20, p.Result.Item2);
                });
            });
        }

        /// <summary>
        /// Remove a element with minimum value in AVL
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private Node<int> RemoveMin(Node<int> x)
        {
            if (x.Left == null)
                return x.Right;
            x.Left = RemoveMin(x.Left);
            return x;
        }

        /// <summary>
        /// Remove a element in AVL -paramater is a object <seealso cref="Node{T}"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<Node<int>> RemoveAsync(Node<int> x, int key, UIElement p)
        {
            if (x == null) return null;
            int cmp = key.CompareTo(x.Data);
            if (cmp < 0)
                x.Left = await RemoveAsync(x.Left, key, p);
            else if (cmp > 0)
                x.Right = await RemoveAsync(x.Right, key, p);
            else
            {
                if (x.Right == null)
                    return x.Left;
                if (x.Left == null)
                    return x.Right;
                Node<int> t = x;
                x.Data = GetMin(t.Right).Data;
                x.Right = RemoveMin(t.Right);
                x.Left = t.Left;
            }
            x = await BalanceAsync(x, p);
            return x;
        }

        /// <summary>
        ///Return a minimum value in node
        /// </summary>
        /// <returns></returns>
        public Node<int> GetMin(Node<int> node)
        {
            var temp = node;
            if (node == null)
            {
                return node;
            }
            while (true)
            {
                if (temp.Left == null)
                {
                    return temp;
                }
                else if (temp.Left != null)
                {
                    temp = temp.Left;
                }
            }
        }

        /// <summary>
        /// The line be found in grid by name
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Line FindLineInGrid(Grid grid, string name)
        {
            var line = grid.Children.OfType<Line>().Where(p => p.Name.Equals(name)).FirstOrDefault();
            return line;
        }

        /// <summary>
        /// Find the Button in grid  - async
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="match">name of the button</param>
        /// <returns></returns>
        async Task<Tuple<List<Task>, Button>> FindButtonInGrid(Grid grid, object match)
        {
            Button button = null;
            List<Task> listTask = new List<Task>();
            var allButtonInGrid = grid.Children.OfType<Button>().ToList();
            for (int i = 0; i < allButtonInGrid.Count; i++)//Find a button 
            {
                int j = i;
                var task = Task.Factory.StartNew(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (allButtonInGrid[j].Content.Equals(match.ToString()))
                        {
                            button = allButtonInGrid[j];
                        }
                    });
                });
                listTask.Add(task);
            }
            await Task.WhenAll(listTask);
            //var result = new Tuple<List<Task>, Button>(listTask, button);
            return new Tuple<List<Task>, Button>(listTask, button);
        }

        /// <summary>
        /// Update X,Y for button (node)
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="nodeDelete"></param>
        async void UpdateButtonAfterDeleteAsync(Grid grid, int nodeDelete)
        {

            var nodeDel = Tree.FindNode(new Node<int>(nodeDelete));
            //Delete button have one or none child
            var nodePa = Tree.FindParent(new Node<int>(nodeDel.Data));//Parent of nodeDelete
            if (nodePa.Item1 != null)
            {
                var line = FindLineInGrid(grid, $"{"Btn" + nodePa.Item1.Data.ToString() + "Btn" + nodeDel.Data.ToString()}");
                grid.Children.Remove(line);
            }
            //Relayout line

            var lineL = nodeDel.Left == null ? null : FindLineInGrid(grid, $"{"Btn" + nodeDel.Data.ToString() + "Btn" + nodeDel.Left.Data.ToString()}");
            var lineR = nodeDel.Right == null ? null : FindLineInGrid(grid, $"{"Btn" + nodeDel.Data.ToString() + "Btn" + nodeDel.Right.Data.ToString()}");
            grid.Children.Remove(lineR);
            grid.Children.Remove(lineL);
            var nodeL = nodeDel.Left;
            var nodeR = nodeDel.Right;
            var nodeP = nodePa.Item1;
            var isRight = nodePa.Item2 > 0;
            var taskL = Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() => { RelayoutButtonAfterDeleteAsync(grid, nodeP, isRight, nodeL); });
            });
            var taskR = Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() => { RelayoutButtonAfterDeleteAsync(grid, nodeP, isRight, nodeR); });
            });
            await Task.WhenAll(new Task[] { taskL, taskR });
            //}

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="nodeParent"></param>
        /// <param name="isRight"></param>
        /// <param name="node"></param>
        async void RelayoutButtonAfterDeleteAsync(Grid grid, Node<int> nodeParent, bool isRight, Node<int> node = null)
        {
            if (node == null)
            {
                return;
            }
            if (nodeParent != null)
            {
                var nameLine = $"{"Btn" + nodeParent.Data.ToString() + "Btn" + node.Data.ToString()}";
                var line = FindLineInGrid(grid, nameLine);
                grid.Children.Remove(line);
                var remainingSpace = grid.ActualWidth / Math.Pow(2, ((nodeParent.Y + VerticalMarging) / VerticalMarging));
                node.X = nodeParent.X + (isRight == true ? remainingSpace : -remainingSpace);
                node.Y = nodeParent.Y + VerticalMarging;
                DrawLine(grid, nodeParent.X, node.X, nodeParent.Y, node.Y, isRight, nameLine);
            }
            else
            {
                node.X = grid.ActualWidth / 2;
                node.Y = VerticalMarging;
            }
            var button = grid.Children.OfType<Button>().Where(p => p.Name.Equals("Btn" + node.Data.ToString())).Single();
            AnimationButtonMovetTo(node.X, node.Y, button);

            var taskR = Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() => { RelayoutButtonAfterDeleteAsync(grid, node, true, node.Right); });
            });
            var taskL = Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() => { RelayoutButtonAfterDeleteAsync(grid, node, false, node.Left); });
            });
            await Task.WhenAll(new Task[] { taskL, taskR });
        }

        /// <summary>
        /// Move a button from here to (x,y) with animation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="button"></param>
        private void AnimationButtonMovetTo(double x, double y, Button button)
        {
            if (button == null)
            {
                return;
            }
            Storyboard sb = new Storyboard();
            ThicknessAnimation animation = new ThicknessAnimation(new Thickness(x, y, 0, 0), TimeSpan.FromSeconds(1));
            Storyboard.SetTarget(animation, button);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            sb.Children.Add(animation);
            sb.Completed += (o, s) =>
            {
                var margin = button.Margin;
                button.BeginAnimation(Button.MarginProperty, null);
                button.Margin = new Thickness(x, y, 0, 0);
            };
            sb.Begin();
        }

        #endregion



        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}