using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Underlords_Oracle.Models
{
    public class DictHeroes
    {
        public List<DictHero> HeroesList { get; set; }

        public DictHeroes()
        {
            HeroesList = new List<DictHero>();
        }
    }

    public class DictHero : Dictionary<string, string>
    {

    }

    public class HeroNameToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string heroName = (string)value;
            heroName = heroName.Replace(" ", "_");
            heroName = heroName.Replace("'", "");
            return $"/Resources/heroes/{heroName}_portrait_icon.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SynergiesToIconsListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string synergiesString = (string)value;
            string[] synergies = synergiesString.Split(" ");

            List<string> synergiesList = new();
            foreach (var synergy in synergies)
            {
                synergiesList.Add(synergy);
            }
            return synergiesList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SynergyNameToIconSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string synergyName = (string)value;
            return $"/Resources/synergies/{synergyName}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
