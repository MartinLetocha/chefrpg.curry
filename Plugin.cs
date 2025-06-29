extern alias UNITYENGINE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using UNITYENGINE::UnityEngine;

namespace CurryRPG;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        try
        {
            HarmonyFileLog.Enabled = true;
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
            Logger.LogInfo($"Patching successful!");
        }
        catch (Exception e)
        {
            Logger.LogError($"Patching failed: {e}");
            throw;
        }

        PlayStaticStats.TestingBuild = true;
    }

    private void Start()
    {
        try
        {
            string pluginPath = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);

            //Checks for which saves the mod should work
            var which = File.ReadAllText(pluginPath + "/choose_saves.txt").Split(',');

            //Checks if the recipe should be removed from save file.
            for (int i = 1; i < which.Length + 1; i++)
            {
                if (which[i-1] == "n")
                {
                    Logger.LogWarning($"Skipping removal of recipe in save {i}, user choice");
                    continue;
                }

                if (!File.Exists(pluginPath + "/delete.txt"))
                {
                    var main = ItemSaveIO.LoadRecipeStatus(i, "RecipeMain");
                    if (main == null)
                    {
                        Logger.LogWarning($"Skipping removal of recipe in save {i}, save doesn't exist");
                        continue;
                    }
                    var savedRecipes = main.SavedSlots.ToList();
                    foreach (var recipe in savedRecipes.ToList())
                    {
                        if (recipe == null)
                        {
                            continue;
                        }

                        if (recipe.SellPrice != 2000000)
                        {
                            continue;
                        }

                        savedRecipes.Remove(recipe);
                        Logger.LogInfo("Recipe found.");
                    }

                    main.SavedSlots = savedRecipes.ToArray();
                    ItemSaveIO.SaveRecipeStatus(i, main, "RecipeMain");
                    Logger.LogInfo("Recipe has been successfully deleted in save " + i);
                    which[i-1] = "n";
                }
            }

            //Checks if the recipe is already in the save
            string foundID = "";
            bool[] existsInSave = new bool[which.Length];
            List<RecipeContainerSaveData> mainRecipes = new();
            for (int i = 1; i < which.Length + 1; i++)
            {
                if (which[i-1] == "n")
                {
                    Logger.LogWarning($"Skipping check of recipe in save {i}, user choice");
                    mainRecipes.Add(null);
                    continue;
                }

                mainRecipes.Add(ItemSaveIO.LoadRecipeStatus(i, "RecipeMain"));
                if (mainRecipes[i - 1] == null)
                {
                    Logger.LogWarning($"Skipping check of recipe in save {i}, save doesn't exist");
                    continue;
                }

                foreach (var recipe in mainRecipes[i - 1].SavedSlots)
                {
                    if (recipe == null)
                    {
                        continue;
                    }

                    if (recipe.SellPrice != 2000000)
                    {
                        continue;
                    }
                    existsInSave[i-1] = true;
                    foundID = recipe.RecipeID;
                    Logger.LogInfo($"Curry found in save {i}. ID: {foundID}");
                }
            }

            //Inputs the recipe into the database
            var db = Resources.FindObjectsOfTypeAll<RecipeDataBase>().First();
            var field = AccessTools.Field(typeof(RecipeDataBase), "recipeItems");
            var oldArray = (RecipeItem[])field.GetValue(db);
            var list = oldArray.ToList();
            var ingDB = Resources.FindObjectsOfTypeAll<IngredientDatabase>().First();
            string fullPath = Path.Combine(pluginPath, "Assets");

            RecipeItem curry = CreateRecipeItem(ingDB, fullPath, "WildRice", "WildTomato", "RawPheasant", "Chili");
            AccessTools.Field(typeof(RecipeItem), "id")
                .SetValue(curry, (foundID == "" ? Guid.NewGuid().ToString() : foundID));
            list.Add(curry);
            field.SetValue(db, list.ToArray());
            Logger.LogInfo("Added Curry recipe to database.");

            //Inputs the recipe into the save
            for (int i = 1; i < which.Length + 1; i++)
            {
                if (which[i-1] == "n")
                {
                    Logger.LogWarning($"Skipping load of recipe into save {i}, user choice");
                    continue;
                }
                if (existsInSave[i - 1])
                {
                    Logger.LogWarning($"Skipping load of recipe into save {i}, save already has recipe");
                    continue;
                }
                if (mainRecipes[i - 1] == null)
                {
                    Logger.LogWarning($"Skipping load of recipe into save {i}, save doesn't exist");
                    continue;
                }
                
                var recipesList = mainRecipes[i-1].SavedSlots.ToList();
                var recipeSlotSave = new RecipeSlotSaveData(curry.ID, 0, false, false, "0", "0", 2000000, 1, 1);

                recipesList.Add(recipeSlotSave);
                mainRecipes[i-1].SavedSlots = recipesList.ToArray();
                ItemSaveIO.SaveRecipeStatus(i, mainRecipes[i-1], "RecipeMain");
                Logger.LogInfo("Added Curry recipe to save " + i);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            throw;
        }
    }

    RecipeItem CreateRecipeItem(IngredientDatabase ingredientDB, string pathToImages, string ingredient1,
        string ingredient2, string ingredient3 = "", string ingredient4 = "", string ingredient5 = "")
    {
        var items = ingredientDB.GetAllIngredients();
        RecipeItem curry = ScriptableObject.CreateInstance<RecipeItem>();

        curry.applianceType = RecipeItem.ApplianceType.Stove;
        curry.rarity = Rarity.Epic;
        curry.dishType = RecipeItem.DishType.Meat;
        curry.MaximumStacks = 1;
        curry.recipeCategory = RecipeItem.RecipeCategory.BBQ;
        curry.recipeType = RecipeItem.RecipeType.MainCourse;
        curry.name = "Recipe Curry";
        curry.DishKey = "Curry";
        curry.Ingredient1 = ingredient1 == "" ? null : items.FirstOrDefault(x => x.ItemKey == ingredient1);
        curry.Ingredient2 = ingredient2 == "" ? null : items.FirstOrDefault(x => x.ItemKey == ingredient2);
        curry.Ingredient3 = ingredient3 == "" ? null : items.FirstOrDefault(x => x.ItemKey == ingredient3);
        curry.Ingredient4 = ingredient4 == "" ? null : items.FirstOrDefault(x => x.ItemKey == ingredient4);
        curry.Ingredient5 = ingredient5 == "" ? null : items.FirstOrDefault(x => x.ItemKey == ingredient5);
        curry.FoodArt = LoadSprite(pathToImages + "/FA Curry.png");
        curry.Icon = LoadSprite(pathToImages + "/Small Food Curry.png");
        curry.EmptySprite = LoadSprite(pathToImages + "/Empty Curry.png");
        return curry;
    }

    [HarmonyPatch(typeof(LocalizationSystem), nameof(LocalizationSystem.GetLocalisedValue))]
    class Patch_GetLocalisedValue
    {
        static bool Prefix(string key, ref string __result)
        {
            if (key == "Curry_Recipe")
            {
                __result = "Curry";
                return false;
            }

            return true;
        }
    }

    private static Sprite LoadSprite(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Logger.LogError($"Sprite for dish not found: {filePath}");
            return null;
        }

        byte[] pngData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(4, 2, TextureFormat.ARGB32, false);

        UnityEngine.ImageConversion.LoadImage(texture, pngData);
        texture.filterMode = FilterMode.Point;
        texture.mipMapBias = 0;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.name = filePath;
        texture.Apply();
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            24f
        );
    }
}