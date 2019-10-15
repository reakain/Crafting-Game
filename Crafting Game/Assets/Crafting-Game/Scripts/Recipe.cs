using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CraftingGame
{
    public class Recipe
    {
        private readonly ItemBundle output;
        private readonly Inventory ingredients;

        public string Name { get { return output.Name; } }
        public int Amount { get { return output.Amount; } }

        public Recipe(string item, int amount)
        {
            output = new ItemBundle(item, amount);
            ingredients = new Inventory();
        }

        public static Recipe For(string item, int amount = 1)
        {
            return new Recipe(item, amount);
        }

        public Recipe Require(string item, int amount = 1)
        {
            ingredients.ChangeAmount(item, amount);
            return this;
        }

        public bool CanCraft(Inventory availableItems)
        {
            return availableItems.Contains(ingredients);
        }

        public ItemBundle Craft(Inventory availableItems)
        {
            availableItems.Remove(ingredients);
            return output.Clone();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(output);
            sb.Append(" = ");
            sb.Append(ingredients);
            return sb.ToString();
        }
    }
}