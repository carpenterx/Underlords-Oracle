using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Underlords_Oracle.Models;

namespace Underlords_Oracle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private DictHeroes heroeDictionaries;
        private static List<string> statReferenceNames = new() { "Tier", "Health", "Mana", "DPS", "AttackRate", "MoveSpeed", "Range", "MagicResist", "Armor" };
        private static List<string> statPrettyNames = new() { "tier", "health", "mana", "DPS", "attack rate", "move speed", "attack range", "magic resist", "armor" };

        private List<string> statReferenceNamesWithoutTier = statReferenceNames.GetRange(1, statReferenceNames.Count - 1);
        private List<string> statPrettyNamesWithoutTier = statPrettyNames.GetRange(1, statPrettyNames.Count - 1);

        private List<string> synergiesList = new();

        private static readonly string APP_RESOURCES_NAMESPACE = "Underlords_Oracle.Resources.";

        public MainWindow()
        {
            InitializeComponent();

            heroeDictionaries = JsonConvert.DeserializeObject<DictHeroes>(ReadResource("underlords.json"));

            statsCombo.ItemsSource = statPrettyNames;
            statsCombo2.ItemsSource = statPrettyNamesWithoutTier;
            statsCombo3.ItemsSource = statPrettyNames;
            
            GenerateSynergiesList();
            synergyCombo5.ItemsSource = synergiesList;
            synergyCombo6.ItemsSource = synergiesList;
            statsCombo6.ItemsSource = statPrettyNames;
        }

        private string ReadResource(string fileName)
        {
            string resourceData = "";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(APP_RESOURCES_NAMESPACE + fileName))
            {
                TextReader tr = new StreamReader(stream);
                resourceData = tr.ReadToEnd();
            }
            return resourceData;
        }

        private void GenerateSynergiesList()
        {
            List<string> allSynergies = new();

            foreach (var hero in heroeDictionaries.HeroesList)
            {
                string synergyText = hero["Synergies"];
                string[] synergiesArray = synergyText.Split(" ");
                foreach (var synergy in synergiesArray)
                {
                    allSynergies.Add(synergy);
                }
            }

            synergiesList = allSynergies.Distinct().ToList();
            synergiesList.Sort();
        }

        private List<DictHero> FilterHeroesByTier(List<DictHero> heroes, string tier)
        {
            List<DictHero> filteredHeroes = new();

            foreach (var heroDictionary in heroes)
            {
                if(heroDictionary["Tier"] == tier)
                {
                    filteredHeroes.Add(heroDictionary);
                }
            }

            return filteredHeroes;
        }

        private List<DictHero> FilterHeroesBySynergy(List<DictHero> heroes, string synergy)
        {
            List<DictHero> filteredHeroes = new();

            foreach (var heroDictionary in heroes)
            {
                if (heroDictionary["Synergies"].Contains(synergy))
                {
                    filteredHeroes.Add(heroDictionary);
                }
            }

            return filteredHeroes;
        }

        private List<DictHero> FindHeroesByValue(List<DictHero> heroes, string valueName, int expectedResult, int stars = 1)
        {
            var startValueHero = heroes[0];
            string resultValue = startValueHero[valueName];
            string extractedResultValue = GetValue(valueName, resultValue, stars);

            List<DictHero> results = new();
            foreach (var hero in heroes)
            {
                string currentValue = hero[valueName];
                string extractedCurrentValue = GetValue(valueName, currentValue, stars);
                if (ComparisonIsValid(valueName, extractedCurrentValue, extractedResultValue, expectedResult))
                {
                    extractedResultValue = extractedCurrentValue;
                    results = new List<DictHero>
                    {
                        hero
                    };
                }
                else if (extractedCurrentValue == extractedResultValue)
                {
                    results.Add(hero);
                }
            }

            return results;
        }

        private string GetComparisonString(int expectedResult)
        {
            if (expectedResult == 1)
            {
                return "highest";
            }
            else
            {
                return "lowest";
            }
        }

        private string GetResults(List<DictHero> heroes)
        {
            string resultStr = "";
            foreach (var hero in heroes)
            {
                resultStr += hero["Name"] + "\n";
            }

            resultsList.ItemsSource = heroes;

            return resultStr;
        }

        private static bool ComparisonIsValid(string valueName, string currentValue, string resultValue, int expectedResult)
        {
            bool isValid = false;
            switch (valueName)
            {
                case "Tier":
                case "Mana":
                case "MoveSpeed":
                case "Range":
                case "Health":
                case "DPS":
                case "MagicResist":
                case "Armor":
                    isValid = IntComparison(currentValue, resultValue) == expectedResult;
                    break;
                case "AttackRate":
                    isValid = FloatComparison(currentValue, resultValue) == expectedResult;
                    break;
            }

            return isValid;
        }

        private static int IntComparison(string currentValue, string resultValue)
        {
            return int.Parse(currentValue).CompareTo(int.Parse(resultValue));
        }

        private static int FloatComparison(string currentValue, string resultValue)
        {
            return float.Parse(currentValue, NumberStyles.Any, CultureInfo.InvariantCulture).CompareTo(float.Parse(resultValue, NumberStyles.Any, CultureInfo.InvariantCulture));
        }

        private static string GetValue(string valueName, string value, int stars = 1)
        {
            string valueString = "";
            switch (valueName)
            {
                case "Tier":
                case "Mana":
                case "MoveSpeed":
                case "Range":
                    valueString = value;
                    break;
                case "Health":
                case "DPS":
                case "MagicResist":
                case "Armor":
                case "AttackRate":
                    valueString = ExtractValue(value, stars);
                    break;
            }
            return valueString;
        }

        private static string ExtractValue(string fullData, int stars = 1)
        {
            string[] values = fullData.Split("|");
            if(values.Length == 1)
            {
                return values[0];
            }
            else
            {
                return values[stars - 1];
            }
        }

        private void ExecuteFirstQuery(object sender, RoutedEventArgs e)
        {
            // hero with highest/lowest [Stat]
            string statName = statReferenceNames[statsCombo.SelectedIndex];
            string statPrettyName = statPrettyNames[statsCombo.SelectedIndex];
            int comparisonResultNumber = GetComparisonResultNumber(comparisonCombo.SelectedIndex);
            
            var result = FindHeroesByValue(heroeDictionaries.HeroesList, statName, comparisonResultNumber);

            string extractedValue = GetValue(statName, result[0][statName]);
            string comparisonString = GetComparisonString(comparisonResultNumber);
            queryTitle.Content = $"Hero with {comparisonString} {statPrettyName} ({extractedValue})";
            GetResults(result);
        }

        private int GetComparisonResultNumber(int index)
        {
            if(index == 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        private void ExecuteSecondQuery(object sender, RoutedEventArgs e)
        {
            // tier [X] hero with highest/lowest [Stat]
            string tierString = (tierCombo2.SelectedIndex + 1).ToString();
            string statName = statReferenceNamesWithoutTier[statsCombo2.SelectedIndex];
            string statPrettyName = statPrettyNamesWithoutTier[statsCombo2.SelectedIndex];
            int comparisonResultNumber = GetComparisonResultNumber(comparisonCombo2.SelectedIndex);

            var tierFilter = FilterHeroesByTier(heroeDictionaries.HeroesList, tierString);
            var result = FindHeroesByValue(tierFilter, statName, comparisonResultNumber);

            string extractedValue = GetValue(statName, result[0][statName]);
            string comparisonString = GetComparisonString(comparisonResultNumber);
            queryTitle.Content = $"Tier {tierString} hero with {comparisonString} {statPrettyName} ({extractedValue})";
            GetResults(result);
        }

        private void ExecuteThirdQuery(object sender, RoutedEventArgs e)
        {
            //[Y] star hero with highest/lowest [Stat]
            string statName = statReferenceNames[statsCombo3.SelectedIndex];
            string statPrettyName = statPrettyNames[statsCombo3.SelectedIndex];
            int comparisonResultNumber = GetComparisonResultNumber(comparisonCombo3.SelectedIndex);
            int stars = starCombo3.SelectedIndex + 1;

            var result = FindHeroesByValue(heroeDictionaries.HeroesList, statName, comparisonResultNumber, stars);

            string extractedValue = GetValue(statName, result[0][statName], stars);
            string comparisonString = GetComparisonString(comparisonResultNumber);
            queryTitle.Content = $"{stars} star hero with {comparisonString} {statPrettyName} ({extractedValue})";

            GetResults(result);
        }

        private void ExecuteFourthQuery(object sender, RoutedEventArgs e)
        {
            //(all) tier [X] heroes
            string tierString = (tierCombo4.SelectedIndex + 1).ToString();
            var result = FilterHeroesByTier(heroeDictionaries.HeroesList, tierString);
            queryTitle.Content = $"All tier {tierString} heroes";
            GetResults(result);
        }

        private void ExecuteFifthQuery(object sender, RoutedEventArgs e)
        {
            //(all) tier [X] [Synergy]
            string synergyString = synergiesList[synergyCombo5.SelectedIndex];
            string tierString = (tierCombo5.SelectedIndex + 1).ToString();
            var synergyResult = FilterHeroesBySynergy(heroeDictionaries.HeroesList, synergyString);
            var result = FilterHeroesByTier(synergyResult, tierString);
            queryTitle.Content = $"All tier {tierString} {synergyString}";
            GetResults(result);
        }

        private void ExecuteSixthQuery(object sender, RoutedEventArgs e)
        {
            //[Synergy] with highest/lowest [Stat]
            string synergyString = synergiesList[synergyCombo6.SelectedIndex];
            string statName = statReferenceNames[statsCombo6.SelectedIndex];
            string statPrettyName = statPrettyNames[statsCombo6.SelectedIndex];
            int comparisonResultNumber = GetComparisonResultNumber(comparisonCombo6.SelectedIndex);

            var synergyResult = FilterHeroesBySynergy(heroeDictionaries.HeroesList, synergyString);
            var result = FindHeroesByValue(synergyResult, statName, comparisonResultNumber);

            string extractedValue = GetValue(statName, result[0][statName]);
            string comparisonString = GetComparisonString(comparisonResultNumber);
            queryTitle.Content = $"{synergyString} with {comparisonString} {statPrettyName} ({extractedValue})";

            GetResults(result);
        }
    }
}
