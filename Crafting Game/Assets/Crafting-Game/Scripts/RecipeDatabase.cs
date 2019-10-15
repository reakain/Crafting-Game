using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CraftingGame
{
    public class RecipeDatabase
    {
        Dictionary<string, Recipe> recipes = new Dictionary<string, Recipe>();

        public void Add(Recipe recipe)
        {
            recipes.Add(recipe.Name, recipe);
        }

        public Recipe Create(string item, int amount = 1)
        {
            Recipe recipe = Recipe.For(item, amount);
            Add(recipe);
            return recipe;
        }

        public Recipe Get(string recipe)
        {
            return recipes[recipe];
        }

        public IEnumerable<Recipe> GetCraftablerecipes(ItemSet availableItems)
        {
            foreach (var recipe in recipes.Values)
                if (recipe.CanCraft(availableItems))
                    yield return recipe;
        }

        public void Craft(string recipe, ItemSet inventory)
        {
            inventory.AddItem(Get(recipe).Craft(inventory));
        }

        public bool CanCraft(string recipe, ItemSet inventory)
        {
            return Get(recipe).CanCraft(inventory);
        }
    }
}