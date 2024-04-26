using DynamicData;
using DynamicData.Binding;
using Leepfrog.WpfFramework.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Specialized;
using DynamicData.PLinq;

namespace WPF45_LiveShapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //The dispatcher times which will 
        //Update Price property after specific duration 
        DispatcherTimer dispTimer = new DispatcherTimer();

        ObservableCollection<Product> lstProducts = new ObservableCollection<Product>();


        public MainWindow()
        {
            InitializeComponent();
            Random random = new Random();

            var Products = new ProductCollection();
            dispTimer.Interval = TimeSpan.FromMilliseconds(3000);
            dispTimer.Tick += dispTimer_Tick;
            dispTimer.Start();
            foreach (var item in Products)
            {
                lstProducts.Add(new Product()
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price
                });
            }



            void dispTimer_Tick(object sender, EventArgs e)
            {

                foreach (var pc in lstProducts)
                {
                    pc.Price += random.Next(100) - 5;
                }
            }

        }



        /// <summary>
        /// Code for showing product information in DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                dgproducts.ItemsSource = Sorter.SortTheTop<ObservableCollection<Product>, Product, Leader, int, int>(
                    lstProducts,
                    a => a.ProductId,
                    p => p.Price,
                    5,
                     (a, b) =>
                    {

                        return new Leader
                        {
                            Rank = b + 1,
                            UserName = a.ProductName,
                            Price = a.Price

                        };
                        ;
                    }, new Func<Product, bool>((a) => a.ProductName.Contains("Mobile") == false), a => a.Price); ; ;
            }
            else if (1 == 2)
            {
                ICollectionView pView = CollectionViewSource.GetDefaultView(lstProducts);
                pView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
                var productview = (ICollectionViewLiveShaping)pView;
                productview.IsLiveSorting = true;
                dgproducts.ItemsSource = pView;
            }
            else if (false)
            {
                dgproducts.ItemsSource = Sorter2.SortTheTop<ObservableCollection<Product>, Product, Leader, int, int>(
                 lstProducts,
                 a => a.ProductId,
                 5,
                  (a, b) =>
                  {

                      return new Leader
                      {
                          Rank = b + 1,
                          UserName = a.ProductName,
                          Price = a.Price,
                          ProductId = a.ProductId
                      };
                      ;
                  }, new Func<Product, bool>((a) => a.ProductName.Contains("Mobile") == false), a => a.Price); ; ;

            }
        }
    }



    class Sorter2
    {
        public static ObservableCollection<TR> SortTheTop<TCollection, T, TR, TKey, TProperty>(
          TCollection _collection,
          Func<T, TKey> funcKey,
          //IObservable<Unit> observable,
          int top,
          Func<T, int, TR> func,
          Func<T, bool> predicate,
          Func<T, IComparable> funcSort1)
          where T : INotifyPropertyChanged
          where TCollection : INotifyCollectionChanged, IEnumerable<T>
        {
            var productsChangeSet = _collection
                .ToObservableChangeSet<TCollection, T>()
                .AddKey(funcKey)
                .Filter(predicate);

            var x = new ObservableCollection<TR>();
            var xy = new ObservableCollection<T>();

            //var propertyChanges =
            //    productsChangeSet.WhenPropertyChanged<T, TKey, TProperty>(funcProperty)
            //    .Select(_ => Unit.Default);
            var dispTimer = new DispatcherTimer();
            dispTimer.Interval = TimeSpan.FromMilliseconds(3000);
            dispTimer.Tick += (s, e) =>
            {
                var newCollection = _collection.OrderByDescending(ea => funcSort1(ea)).Take(top).Select((ae, i) => (ae, i)).ToArray();

                foreach (var item in newCollection)
                {
                    if (xy.Any(xx => funcKey(xx).Equals(funcKey(item.ae))))
                    {
                        var index = xy.IndexOf(item.ae);

                        if (index != item.i)
                        {

                            var xx = xy.Single(xdx => funcKey(xdx).Equals(funcKey(item.ae)));
                            x.Replace(x.ElementAt(index), func(xx, index));
                            x.Replace(x.ElementAt(item.i), func(item.ae, item.i));

                            xy.Move(index, item.i);
                            //x.Move(index, item.i);

                            //x.Move(index, item.i);
                            //x.Replace(x.);
                            //x.RemoveAt(index);

                            //x.Insert(item.i, func(item.ae, item.i));
                            ////if (index < newCollection.Length)
                            //x.RemoveAt(index);
                        }
                    }
                    else
                    {
                        xy.Insert(item.i, item.ae);
                        //if (x.Count > item.i)
                        //    x.RemoveAt(item.i);

                        x.Insert(item.i, func(item.ae, item.i));
                    }
                }

                while (x.Count > top)
                {
                    xy.RemoveAt(x.Count - 1);
                    x.RemoveAt(x.Count - 1);
                }
            };
            dispTimer.Start();


            return x;
            //productsChangeSet.Sort(SortExpressionComparer<T>.Descending(funcSort1), propertyChanges)

            //    .Top(top)

            //    .Bind(out var top3Products)
            //    .Subscribe();

            //productsChangeSet
            //.Transform((a) => func(a, top3Products), propertyChanges.Select(cs => predicate))
            //.Sort(SortExpressionComparer<TR>.Descending(funcSort2))
            //.Top(top)


            //.Bind(out var collection).Subscribe();
            //return collection;
        }

    }


    class Sorter
    {
        public static ReadOnlyObservableCollection<TR> SortTheTop<TCollection, T, TR, TKey, TProperty>(
            TCollection _collection,
            Func<T, TKey> funcKey,
            System.Linq.Expressions.Expression<Func<T, TProperty>> funcProperty,
            int top,
            Func<T, int, TR> func,
            Func<T, bool> predicate,
            Func<T, IComparable> funcSort)
            where T : INotifyPropertyChanged
            where TCollection : INotifyCollectionChanged, IEnumerable<T>
        {
            var changeSet = _collection
                .ToObservableChangeSet<TCollection, T>()
                .AddKey(funcKey)
                .Filter(predicate);

            //var propertyChanges =
            //    productsChangeSet.WhenPropertyChanged<T, TKey, TProperty>(funcProperty)
            //    .Select(_ => Unit.Default);

            var propertyChanges = Observable.Create<Unit>(observer =>
            {
                var dispTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(1000)
                };
                dispTimer.Start();

                return Observable
                .FromEventPattern(a => dispTimer.Tick += a, a => dispTimer.Tick -= a)
                .Select(a => Unit.Default)
                .Subscribe(observer);
            });

            var sortedAndTop = changeSet.Sort(SortExpressionComparer<T>.Descending(funcSort), propertyChanges)
            .Top(top);

            sortedAndTop
                .Bind(out var sortedAndTopCollection)
                .Subscribe();

            Dictionary<TR, int> dictionary = new Dictionary<TR, int>();

            sortedAndTop
            .Transform((a) =>
            {
                var index = sortedAndTopCollection.IndexOf(a);
                var ouput = func(a, index);
                dictionary[ouput] = index;
                return ouput;
            }, propertyChanges.Select(cs => predicate))
            .Sort(SortExpressionComparer<TR>.Ascending(a => dictionary[a]))


            .Bind(out var collection).Subscribe();
            return collection;
        }

    }

    public class Leader : ViewModelBase
    {
        private Color color;
        private string userName;
        private string firstName;
        private string lastName;
        private int rank;
        private int holesPlayed;
        private int strokes;
        private int timeInPlay;
        private Guid scorecardId;
        private double price;

        public Color Color { get => color; set { color = value; this.RaisePropertyChanged(); } }
        public string UserName
        {
            get => userName; set
            {
                userName = value; this.RaisePropertyChanged();
            }
        }

        public int Rank
        {
            get => rank; set
            {
                rank = value; this.RaisePropertyChanged();
            }
        }

        public double Price
        {
            get => price; set
            {
                price = value;
                //this.RaisePropertyChanged();
            }
        }

        public int ProductId { get; internal set; }
    }


    public class TaskProperty<T> : ViewModelBase where T : struct
    {
        private readonly Func<Task<T>> func;
        Task<T> pointsTask;

        public TaskProperty(Func<Task<T>> func)
        {
            this.func = func;
        }

        public T? Value
        {
            get
            {
                if (pointsTask?.Status == TaskStatus.RanToCompletion)
                    return pointsTask.Result;
                else
                    pointsTask = Task.Run(async () =>
                    {
                        var stat = await func().ConfigureAwait(false);

                        //stat.PropertyChanged += (s, e) =>
                        //{
                        //    raisePropertyChanged(nameof(Value));
                        //};
                        RaisePropertyChanged(nameof(Value));
                        return stat;
                    });
                return null;
            }
        }
    }
}