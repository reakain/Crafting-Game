using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CraftingGame
{
    public class Inventory
    {
        Dictionary<string, ItemBundle> bundles = new Dictionary<string, ItemBundle>();

        public IEnumerable<ItemBundle> Bundles { get { return bundles.Values; } }

        public void AddItem(ItemBundle item)
        {
            ChangeAmount(item.Name, item.Amount);
        }

        public void ChangeAmount(string item, int amount)
        {
            if (amount == 0)
                return;

            ItemBundle bundle = null;
            if (!bundles.TryGetValue(item, out bundle))
                CreateBundle(item, amount);
            else
            {
                bundle.Change(amount);
                Prune(bundle);
            }
        }

        public int GetAmount(string item)
        {
            ItemBundle bundle = null;
            if (!bundles.TryGetValue(item, out bundle))
                return 0;
            else
                return bundle.Amount;
        }

        public void SetAmount(string item, int amount)
        {
            if (amount == 0)
            {
                Remove(item);
                return;
            }

            ItemBundle bundle = null;
            if (!bundles.TryGetValue(item, out bundle))
                bundle = CreateBundle(item, amount);
            else
                bundle.Set(amount);

            Prune(bundle);
        }

        public bool Contains(Inventory items)
        {
            return items.Bundles.All(item => GetAmount(item.Name) >= item.Amount);
        }

        public void Remove(string item)
        {
            bundles.Remove(item);
        }

        public void Remove(Inventory items)
        {
            if (!Contains(items))
                throw new System.ArgumentException("Can't remove items because not all of them exist in this ItemSet", "items");

            foreach (var item in items.Bundles)
                ChangeAmount(item.Name, -item.Amount);
        }

        private void Prune(ItemBundle bundle)
        {
            if (bundle.IsEmpty)
                Remove(bundle.Name);
        }

        private ItemBundle CreateBundle(string item, int amount)
        {
            ItemBundle bundle = new ItemBundle(item, amount);
            bundles.Add(item, bundle);
            return bundle;
        }

        public override string ToString()
        {
            return string.Join(", ", Bundles.Select(bundle => bundle.ToString()).ToArray());
        }
    }

    public class ItemBundle
    {
        public readonly string Name;
        public int Amount { get; private set; }
        public bool IsEmpty { get { return Amount == 0; } }

        public ItemBundle(string name, int amount = 1)
        {
            ThrowNegativeAmounts(amount);
            Name = name;
            Amount = amount;
        }

        private static void ThrowNegativeAmounts(int amount)
        {
            if (amount < 0)
                throw new System.ArgumentException("ItemStack does not support negative amount", "amount");
        }

        public void Change(int amount)
        {
            int result = Amount + amount;
            ThrowNegativeAmounts(result);
            Amount = result;
        }

        public void Set(int amount)
        {
            ThrowNegativeAmounts(amount);
            Amount = amount;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Amount, Name);
        }

        public ItemBundle Clone()
        {
            return new ItemBundle(Name, Amount);
        }
    }

    #region ItemDatabases
    [System.Serializable]
    public class ItemDatabase
    {
        public Item[] items;
        public static ItemDatabase CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<ItemDatabase>(jsonString);
        }
    }

    [System.Serializable]
    public class Item
    {
        public string name = "";
        public string desc = "";
        public string atlas = "";
        public string sprite = "";
        public string action = "";
        public bool key;
    }
    #endregion
}