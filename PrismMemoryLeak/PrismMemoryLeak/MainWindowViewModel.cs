using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Events;

using PrismMemoryLeak.Annotations;

namespace PrismMemoryLeak
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly EventAggregator eventAggregator;

        private CancellationTokenSource cts;
        private int instancesAliveCount;
        private int subscriptionListCount;

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            StartMemoryTest = new RelayCommand(OnStartMemoryTest);
            StopMemoryTest = new RelayCommand(OnStopMemoryTest);
            PublishEvent = new RelayCommand(OnPublishEvent);
            CallGc = new RelayCommand(OnCallGc);
            eventAggregator = new EventAggregator();
        }

        #endregion

        #region Properties and Indexers

        public ICommand StartMemoryTest { get; set; }
        public ICommand StopMemoryTest { get; set; }
        public ICommand PublishEvent { get; set; }
        public ICommand CallGc { get; set; }

        public int InstancesAliveCount
        {
            get => instancesAliveCount;
            set
            {
                instancesAliveCount = value;
                OnPropertyChanged(nameof(InstancesAliveCount));
            }
        }

        public int SubscriptionListCount
        {
            get => subscriptionListCount;
            set
            {
                subscriptionListCount = value;
                OnPropertyChanged(nameof(SubscriptionListCount));
            }
        }

        #endregion

        #region Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }

        private static void OnCallGc(object obj)
        {
            GcClean();
        }

        private static void GcClean()
        {
            GC.Collect(3,
                GCCollectionMode.Default,
                true,
                true);

            GC.WaitForPendingFinalizers();

            GC.Collect(3,
                GCCollectionMode.Default,
                true,
                true);
        }

        private void OnPublishEvent(object obj)
        {
            eventAggregator.GetEvent<PubSubEvent<string>>().Publish("Published");
        }

        private void OnStopMemoryTest(object obj)
        {
            cts.Cancel();
        }

        private void OnStartMemoryTest(object obj)
        {
            cts = new CancellationTokenSource();

            Task.Factory.StartNew(MemoryTest,
                cts.Token);
        }

        private void MemoryTest()
        {
            var weakReferenceList = new List<WeakReference<TestInstance>>();

            while (!cts.Token.IsCancellationRequested)
            {
                for (var i = 0; i < 10000; i++)
                {
                    weakReferenceList.Add(new WeakReference<TestInstance>(new TestInstance(eventAggregator)));
                }

                GcClean();

                var subscriptionList = (IList)typeof(PubSubEvent<string>).GetProperty("Subscriptions",
                        BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(eventAggregator.GetEvent<PubSubEvent<string>>());

                InstancesAliveCount = weakReferenceList.Count(x => x.TryGetTarget(out _));
                if (subscriptionList != null)
                {
                    SubscriptionListCount = subscriptionList.Count;
                }
            }
        }

        #endregion

        #region Interface Implementations

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}