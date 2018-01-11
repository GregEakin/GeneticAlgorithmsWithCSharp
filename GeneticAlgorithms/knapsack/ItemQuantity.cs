using System;

namespace GeneticAlgorithms.knapsack
{
    public class ItemQuantity : IComparable
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
                    return Item != that.Item
                        ? Item.CompareTo(that.Item)
                        : Quantity.CompareTo(that.Quantity);
                default:
                    throw new ArgumentException("Object is not a Fitness");
            }
        }
    }
}