using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReligionChecker
{
    enum Religions
    {
        christianity,
        islam,
        buddhism,
        judaism,
        hinduism,
        sintoism,
        taoism,
        confucianism,
        witoutReligion
    }

    class State
    {
        public string Name { get; set; }
        public Religions Religion { get; set; }

        public static Religions GetReligion(List<string> fileContent)
        {
            foreach (var content in fileContent)
            {
                if(content.Contains("set_state_flag"))
                {
                    string religionString = content.Trim().Replace(" ", "").Remove(0, "set_state_flag".Length + 1);

                    switch (religionString)
                    {
                        case "christianity": return Religions.christianity;
                        case "islam": return Religions.islam;
                        case "buddhism": return Religions.buddhism;
                        case "judaism": return Religions.judaism;
                        case "hinduism": return Religions.hinduism;
                        case "sintoism": return Religions.sintoism;
                        case "taoism": return Religions.taoism;
                        case "confucianism": return Religions.confucianism;
                        default: return Religions.witoutReligion;
                    }
                }
            }
           
            return Religions.witoutReligion;
        }
    }
}
