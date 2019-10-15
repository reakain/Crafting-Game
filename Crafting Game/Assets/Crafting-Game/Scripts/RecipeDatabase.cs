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

        public IEnumerable<Recipe> GetCraftableRecipes(Inventory availableItems)
        {
            foreach (var recipe in recipes.Values)
                if (recipe.CanCraft(availableItems))
                    yield return recipe;
        }

        public void Craft(string recipe, Inventory inventory)
        {
            inventory.AddItem(Get(recipe).Craft(inventory));
        }

        public bool CanCraft(string recipe, Inventory inventory)
        {
            return Get(recipe).CanCraft(inventory);
        }

        public void LoadFromJson(string jsonString)
        {
            var recipesdata = RecipeDataJson.CreateFromJson(jsonString);
            foreach(var reci in recipesdata.recipes)
            {
                Create(reci.name, reci.qty);
                foreach(var item in reci.items)
                {
                    recipes[reci.name].Require(item.name, item.qty);
                }
            }
        }
        
    }

    public class RecipeDataJson
        {
        public RecipeJson[] recipes;

        public static RecipeDataJson CreateFromJson(string jsonString)
        {
            return JsonUtility.FromJson<RecipeDataJson>(jsonString);
        }
    }

    public class RecipeJson
    {
        public string name;
        public int qty;
        public ItemJson[] items;
    }

    public class ItemJson
    {
        public string name;
        public int qty;
    }
}