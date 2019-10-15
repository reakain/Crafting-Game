using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// https://answers.unity.com/questions/1077066/how-to-implement-item-combination.html
public class CraftableSystemDemo : MonoBehaviour
{
    private RecepieDatabase database = new RecepieDatabase();
    private ItemSet inventory = new ItemSet();
    private StringBuilder log = new StringBuilder();

    void Awake()
    {
        try
        {
            CreateRecipes();
            PopulateInventory();

            // Display what we got at the moment.
            LogCraftableRecepies();
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
        LogCraftableRecepies();
    }

    private void LogCraftableRecepies()
    {
        Log("Craftable Recepies");
        Log("==================");

        foreach (var recepie in database.GetCraftableRecepies(inventory))
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

public class RecepieDatabase
{
    Dictionary<string, Recipe> recepies = new Dictionary<string, Recipe>();

    public void Add(Recipe recepie)
    {
        recepies.Add(recepie.Name, recepie);
    }

    public Recipe Create(string item, int amount = 1)
    {
        Recipe recepie = Recipe.For(item, amount);
        Add(recepie);
        return recepie;
    }

    public Recipe Get(string recepie)
    {
        return recepies[recepie];
    }

    public IEnumerable<Recipe> GetCraftableRecepies(ItemSet availableItems)
    {
        foreach (var recepie in recepies.Values)
            if (recepie.CanCraft(availableItems))
                yield return recepie;
    }

    public void Craft(string recepie, ItemSet inventory)
    {
        inventory.AddItem(Get(recepie).Craft(inventory));
    }

    public bool CanCraft(string recepie, ItemSet inventory)
    {
        return Get(recepie).CanCraft(inventory);
    }
}

public class ItemSet
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

    public bool Contains(ItemSet items)
    {
        return items.Bundles.All(item => GetAmount(item.Name) >= item.Amount);
    }

    public void Remove(string item)
    {
        bundles.Remove(item);
    }

    public void Remove(ItemSet items)
    {
        if (!Contains(items))
            throw new ArgumentException("Can't remove items because not all of them exist in this ItemSet", "items");

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

public class Recipe
{
    private readonly ItemBundle output;
    private readonly ItemSet ingredients;

    public string Name { get { return output.Name; } }
    public int Amount { get { return output.Amount; } }

    public Recipe(string item, int amount)
    {
        output = new ItemBundle(item, amount);
        ingredients = new ItemSet();
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

    public bool CanCraft(ItemSet availableItems)
    {
        return availableItems.Contains(ingredients);
    }

    public ItemBundle Craft(ItemSet availableItems)
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
            throw new ArgumentException("ItemStack does not support negative amount", "amount");
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