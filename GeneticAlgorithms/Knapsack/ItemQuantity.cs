using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.Knapsack
{
    public class ItemQuantity : IComparable, IComparable<ItemQuantity>
    {
        public Resource Item { get; }
        public int Quantity { get; }

        public ItemQuantity(Resource item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case ItemQuantity that:
                    return CompareTo(that);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }

        public int CompareTo(ItemQuantity that)
        {
            if (ReferenceEquals(this, that)) return 0;
            if (that is null) return 1;
            var itemComparison = Comparer<Resource>.Default.Compare(Item, that.Item);
            if (itemComparison != 0) return itemComparison;
            return Quantity.CompareTo(that.Quantity);
        }
    }
}