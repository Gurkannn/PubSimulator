using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubTest
{
    public static class NameGenerator
    {
        public static List<string> FirstNameList = new List<string>() {
            "Iva",
"Jalisa",
"Kathrine",
"Cleopatra",
"Jani",
"Marissa",
"Joelle",
"Dorothy",
"Otto",
"Jamison",
"Elza",
"Kendrick",
"Iesha",
"Eleanora",
"Marlo",
"Clarisa",
"Ming",
"Carlee",
"Rosalee",
"Minnie",
"Janett",
"Laurette",
"Luci",
"Lahoma",
"Maya",
"Siobhan",
"Jacquelin",
"Erline",
"Allena",
"Amira",
"Jarred",
"Marni",
"Jaye",
"Noelle",
"Forest",
"Samual",
"Margart",
"Malissa",
"Jerald",
"Shawnee",
"Marcelino",
"Britteny",
"Glady",
"Willard",
"Chanell",
"Brandon",
"Willette",
"Reggie",
"Sallie",
            "Sabra"  };
        public static string GetRandomName()
        {
            Random rand = new Random();
            var nameIndex = rand.Next(0, FirstNameList.Count - 1);
            return FirstNameList[nameIndex];
        }
    }
}
