using System.ComponentModel;

namespace Server.Tools
{
    internal static class RecipeBackendTool
    {
        [Description("Update the recipe with new or modified content.")]
        public static string UpdateRecipe(string recipe)
        {
            // In a real implementation, this would call a backend API to update the recipe.
            return $"Recipe updated successfully: {recipe}";
        }
    }
}