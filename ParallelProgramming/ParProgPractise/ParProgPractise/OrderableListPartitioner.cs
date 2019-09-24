using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections;
using System.Threading;

namespace ParProgPractise
{
    public class OrderableListPartitioner<T> : OrderablePartitioner<T>
    {
        private IList<T> _input;

        public OrderableListPartitioner(
            IList<T> input):base(true,false,true)
        {
            _input = input;
        }

        public override bool SupportsDynamicPartitions => true;


        public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(int partitionCount)
        {
            var dynamicPartitions = GetOrderableDynamicPartitions();

            var partitions = new IEnumerator<KeyValuePair<long, T>>[partitionCount];

            for (int i = 0; i < partitionCount; i++)
            {
                partitions[i] = dynamicPartitions.GetEnumerator();
            }

            return partitions;
        }

        public override IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
        {
            return new ListDynamicPartitions(_input);
        }

        private class ListDynamicPartitions : IEnumerable<KeyValuePair<long, T>>
        {
            private IList<T> _input;
            private int _index;
            public ListDynamicPartitions(IList<T> input)
            {
                _input = input;
            }

            public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
            {
                while (true)
                {
                    var elemIndex
                        = Interlocked.Increment(ref _index) - 1;

                    if (elemIndex > _input.Count)
                    {
                        yield break;
                    }

                    yield return new KeyValuePair<long, T>(elemIndex, _input[elemIndex]);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
