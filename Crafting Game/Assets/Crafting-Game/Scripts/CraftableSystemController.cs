using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CraftingGame
{
    public class CraftableSystemController : MonoBehaviour
    {
        private RecipeDatabase database = new RecipeDatabase();
        private Inventory inventory = new Inventory();
        private StringBuilder log = new StringBuilder();

        void Awake()
        {
            try
            {
                CreateRecipes();
                PopulateInventory();

                // Display what we got at the moment.
                LogCraftableRecipes();
                LogInventory();

                Craft("hammer");
                Craft("heavy hammer");
            }
            finally
            {
                PrintLog();
            }
        }

        private void CreateRecipes()
        {
            database
                .Create("flag")
                .Require("cloth")
                .Require("stick");

            database
                .Create("hammer")
                .Require("stone")
                .Require("stick");

            database
                .Create("heavy hammer")
                .Require("stone")
                .Require("hammer");

            database
                .Create("answer")
                .Require("keyboard")
                .Require("dedication", 10)
                .Require("code", 5);
        }

        private void PopulateInventory()
        {
            inventory.SetAmount("cloth", 2);
            inventory.SetAmount("stick", 2);
            inventory.SetAmount("stone", 2);
        }

        private void Craft(string item)
        {
            Log("Crafting " + item);
            Log();

            database.Craft(item, inventory);

            // Could be helpful to see what's changed.
            LogInventory();
            LogCraftableRecipes();
        }

        private void LogCraftableRecipes()
        {
            Log("Craftable Recepies");
            Log("==================");

            foreach (var recepie in database.GetCraftableRecipes(inventory))
                log.AppendLine("  " + recepie.ToString());

            Log();
        }

        private void LogInventory()
        {
            Log("Inventory Contents");
            Log("==================");

            foreach (ItemBundle bundle in inventory.Bundles)
                log.AppendLine("  " + bundle.ToString());

            Log();
        }

        private void Log(string message = "")
        {
            log.AppendLine(message);
        }

        private void PrintLog()
        {
            // It got tedious to see callstacks of all steps so I put it all in one StringBuilder...
            print(log);
        }
    }
}