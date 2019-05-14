using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Set_Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //I don't think these need to be public. I'll leave them here for now because it's convenient, but it's something I'll likely have to come back to
        private string recent;
        private List<string> allPeriods;
        private List<string> allTiers;
        private int level;
        private bool max, maxPlus, maxZero, maxMaxPlus;
        private List<Pokemon> sets;

        public MainWindow()
        {
            InitializeComponent();
            
            allPeriods = new List<string>();
            recent = ScrapeStats("https://www.smogon.com/stats/", ref allPeriods).DocumentNode.SelectSingleNode("//pre//a[last()]").InnerText;
            allPeriods.Remove("../");
            cbxPeriod.ItemsSource = allPeriods;

            RunTest();//TEST... obviously
        }

        private void RunTest()//TEST... obviously
        {
            BtnRecent_Click(null, null);
            cbxTier.SelectedValue = "gen7ubers-1760.json";
            tbxLevel.Text = "100";
            BtnGetStats_Click(null, null);
        }

        //Updates target list with text of all links present on page in url. Also returns HtmlDoc since that's useful
        private HtmlDocument ScrapeStats(string url, ref List<string> target)
        {
            WebClient web = new WebClient();
            string html = web.DownloadString(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//pre//a"))
            {
                target.Add(node.InnerText);
            }

            return doc;
        }
        //Currently a mod of ScrapeStats, will want this to convert the the data at the url into json, to then be converted into an actual class.
        //Also this probably isn't worth putting into its own function, I'm just tinkering with firefox atm so I cbf looking up docs
        //TWEAK?
        private HtmlDocument ScrapeJSON(string url)
        {
            WebClient web = new WebClient();
            string html = web.DownloadString(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc;
        }

        private void BtnRecent_Click(object sender, RoutedEventArgs e)
        {
            //If most recent selected, use that to bring up list of tiers.
            allTiers = new List<string>();
            ScrapeStats("https://www.smogon.com/stats/" + recent+"chaos/", ref allTiers);
            cbxTier.ItemsSource = allTiers;
            cbxPeriod.SelectedIndex = cbxPeriod.Items.Count - 1;

            cbxTier.IsEnabled = true;
            btnGetStats.IsEnabled = true;
        }

        private void CbxPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Bring up list of tiers for selected period
            allTiers = new List<string>();
            ScrapeStats("https://www.smogon.com/stats/" + cbxPeriod.SelectedValue+"chaos/", ref allTiers);
            allTiers.Remove("../");
            cbxTier.ItemsSource = allTiers;

            cbxTier.IsEnabled = true;
            btnGetStats.IsEnabled = true;
        }

        private void BtnGetStats_Click(object sender, RoutedEventArgs e)
        {
            //For these dictionaries and level, -1 is used to indicate no input
            level = -1;
            Dictionary<string, int> ranks = new Dictionary<string, int>
            {
                {"pokeRank",-1 },
                {"movesRank",-1 },
                {"abilitiesRank",-1 },
                {"itemsRank", -1 },
                {"evsRank",-1 }
            };
            Dictionary<string, float> usages = new Dictionary<string, float>
            {
                {"pokeUsage",-1 },
                {"movesUsage",-1 },
                {"abilitiesUsage",-1 },
                {"itemsUsage", -1 },
                {"evsUsage",-1 }
            };

            //Storing all checkbox values, as well as a variable indicating whether or not any of them are true.
            //Globals because they're needed input validation, but they're applied when generating output rather than processing data, which is a separate event
            max = (bool)chxMax.IsChecked;
            maxPlus = (bool)chxMaxPlus.IsChecked;
            maxZero = (bool)chxMaxZero.IsChecked;
            maxMaxPlus = (bool)chxMaxMaxPlus.IsChecked;

            bool additionalEVs = Check4Bools(max, maxPlus, maxZero, maxMaxPlus);
            ////////////////////////
            //Validate input

            if (!ValidateNumbers(ranks, usages, additionalEVs, ref level))
            {
                MessageBox.Show("Error with inputs. Please make sure level and ranks are integers greater than zero and that usage % is a value 0-100. Either a rank or usage value must be entered for each category, except for EVs if an additional spread is checked");
                return;
            }

            //Get stats
            string url = "https://www.smogon.com/stats/" + cbxPeriod.SelectedValue+"chaos/"+cbxTier.SelectedValue;
            WebClient web = new WebClient();
            string json = web.DownloadString(url);//ISSUE unhandled exception here if stats not available, also I've encountered issues where data ended up being null, indicating partial data was received I guess

            JObject data = (JObject)JObject.Parse(json)["data"];

            //Collect pokemon + usage, then apply filters before processing other attributes
            UsageCollection usg = new UsageCollection();
            IEnumerable<JProperty> mons = data.Properties();
            IEnumerator<JProperty> enumerator = mons.GetEnumerator();
            while (enumerator.MoveNext() != false)
            {
                //TWEAK probably just data[name][usage] cast to a float would be sufficient, but cbf testing that rn
                JObject curMon = (JObject)data[enumerator.Current.Name];
                if(float.TryParse((string)curMon["usage"], out float f))
                {
                    usg.Add(enumerator.Current.Name, f);
                }  
            }

            //////////The rest of this section of code is inadequately TESTED////////////////
            sets = new List<Pokemon>();
            List<string> names = GetNames(ranks["pokeRank"], usages["pokeUsage"], usg);
            if (names != null)
            {
                foreach (string name in names) sets.Add(new Pokemon(name));
            }
            else
            {
                MessageBox.Show("Error generating correct list of pokemon.");
                return;
            }
            
            //Load list of damaging attacks
            StreamReader reader = new StreamReader("attacks.json");
            JObject attacks = JObject.Parse(reader.ReadToEnd());
            reader.Close();
            string[] dmgAttacks = attacks["attacks"].ToObject<string[]>();

            //Fill out the rest of the pokemon class
            foreach (Pokemon pokemon in sets)
            {
                //Remove non-damaging moves
                //TWEAK currently this gets the top ranking moves before checking whether or not they're attacking. May want to check first, then if some are removed, replace them with the next attacking moves
                UsageCollection u = GetData((JObject)data[pokemon.Name]["moves"]);
                List<string> moves = GetNames(ranks["movesRank"], usages["movesUsage"], u);
                //List<string> moves=GetNames(ranks["movesRank"], usages["movesUsage"], GetData((JObject)data[pokemon.Name]["Moves"]));
                moves.RemoveAll(i => !dmgAttacks.Contains(i));
                pokemon.Moves = moves;

                pokemon.Abilities = GetNames(ranks["abilitiesRank"], usages["abilitiesUsage"], GetData((JObject)data[pokemon.Name]["Abilities"]));
                pokemon.Items=GetNames(ranks["itemsRank"], usages["itemsUsage"], GetData((JObject)data[pokemon.Name]["Items"]));
                //Note- additional spreads are to be added when generating list/importables/setdex
                pokemon.Spreads= GetNames(ranks["evsRank"], usages["evsUsage"], GetData((JObject)data[pokemon.Name]["Spreads"]));
            }
            Console.WriteLine("hi");
        }

        //Validates the inputs from the text boxes in the main window    
        //Doesn't strictly need to be in its own function, but it does avoid unnecessarily checking all inputs even after input is found to be false. Could've just used goto for that, but I think this is just neater
        //Level takes a reference because it seems right. I could just as easily remove it as a parameter and just rely on the global, but that seems like the wrong thing to do. Could just be me though.
        private bool ValidateNumbers(Dictionary<string, int> ranks, Dictionary<string, float> usages, bool ignoreEVs, ref int level)
        {
            if (!(int.TryParse(tbxLevel.Text, out level) && level > 0)) return false;
            if (!ValidatePositive(tbxPokeRank.Text, ranks,"pokeRank") & !ValidatePercent(tbxPokeUsage.Text, usages,"pokeUsage")) return false;
            if (!ValidatePositive(tbxMovesRank.Text, ranks,"movesRank") & !ValidatePercent(tbxMovesUsage.Text, usages,"movesUsage")) return false;
            if (!ValidatePositive(tbxItemsRank.Text, ranks,"itemsRank") & !ValidatePercent(tbxItemsUsage.Text, usages,"itemsUsage")) return false;
            if (!ValidatePositive(tbxAbilitiesRank.Text, ranks,"abilitiesRank") & !ValidatePercent(tbxAbilitiesUsage.Text, usages,"abilitiesUsage")) return false;
            if (!ignoreEVs)
            {
                if (!ValidatePositive(tbxEVsRank.Text, ranks,"movesRank") & !ValidatePercent(tbxEVsUsage.Text, usages,"evsUsage")) return false;
            }
            return true;
        }

        //Checks 4 bool values and returns true if any of them are true
        private bool Check4Bools(bool b1, bool b2, bool b3, bool b4)
        {
            if (b1) return true;
            if (b2) return true;
            if (b3) return true;
            if (b4) return true;
            return false;
        }

        //Checks that an integer is >0, saves it to the target if it was passed by reference and returns whether or not it's valid
        private bool ValidatePositive(string input, Dictionary<string, int> ranks, string key)
        {
            int i=-1;
            if (int.TryParse(input, out i) && i > 0)
            {
                ranks[key] = i;
                return true;
            }
            else return false;
        }

        //Checks that an float is >=0 and <=100, converts to decimal, saves it to the target and returns whether or not it's valid
        private bool ValidatePercent(string input, Dictionary<string, float> usages, string key)
        {
            float f = -1;
            if (float.TryParse(input, out f))
            {
                if (f >= 0 && f <= 100)
                {
                    usages[key] =f/100;
                    return true;
                }
                else return false;
            }
            else return false;
        }

        private List<string> GetNames(int rank, float usg, UsageCollection coll)
        {
            if (rank >= 0 && usg >= 0) return coll.ApplyFilter(usg, rank);
            else if (rank < 0 && usg >= 0)return coll.ApplyFilter(usg);
            else if (rank >= 0 && usg < 0) return coll.ApplyFilter(rank);
            else return null; 
        }

        //Goes through usage stats and gets list of elements (moves, etc.) along with usages. Assumes JObject is at correct level, rather than being a level or more below. Goes through all properties, so not suitable for the pokemon themselves (e.g. should be data[pokemon][moves])
        private UsageCollection GetData(JObject data)
        {
            UsageCollection result = new UsageCollection();

            IEnumerable<JProperty> elements = data.Properties();
            IEnumerator<JProperty> enumerator = elements.GetEnumerator();
            while (enumerator.MoveNext() != false)
            {
                string curEl = (string)data[enumerator.Current.Name];
                if (float.TryParse(curEl, out float f))//TWEAK is this validation necessary? Otherwise just casting straight to float would work
                {
                    result.Add(enumerator.Current.Name, f);
                }
            }

            return result;
        }
    }
}
