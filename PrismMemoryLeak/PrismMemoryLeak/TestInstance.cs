using System;

using Prism.Events;

namespace PrismMemoryLeak
{
    internal class TestInstance
    {
        #region Constructors

        public TestInstance(EventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<PubSubEvent<string>>().Subscribe(TestSubscriptionMethod);
        }

        #endregion

        #region Methods

        private void TestSubscriptionMethod(string obj)
        {
            Console.WriteLine(obj);
        }

        #endregion
    }
}