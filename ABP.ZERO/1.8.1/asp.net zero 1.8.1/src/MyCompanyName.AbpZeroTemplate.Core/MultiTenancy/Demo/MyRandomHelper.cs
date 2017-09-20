using System.Collections.Generic;
using System.Linq;
using Abp;

namespace MyCompanyName.AbpZeroTemplate.MultiTenancy.Demo
{
    public static class MyRandomHelper
    {
        //This method can be moved to ABP framework later (to RandomHelper class).
        public static List<T> GenerateRandomizedList<T>(IEnumerable<T> items)
        {
            var currentList = new List<T>(items);
            var randomList = new List<T>();
            
            while (currentList.Any())
            {
                var randomIndex = RandomHelper.GetRandom(0, currentList.Count);
                randomList.Add(currentList[randomIndex]);
                currentList.RemoveAt(randomIndex);
            }

            return randomList;
        }
    }
}