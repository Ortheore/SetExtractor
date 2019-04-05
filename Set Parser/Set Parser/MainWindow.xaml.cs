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

        public MainWindow()
        {
            InitializeComponent();
            
            allPeriods = new List<string>();
            recent = ScrapeStats("https://www.smogon.com/stats/", ref allPeriods).DocumentNode.SelectSingleNode("//pre//a[last()]").InnerText;
            cbxPeriod.ItemsSource = allPeriods;
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

            cbxTier.IsEnabled = true;
            btnGetStats.IsEnabled = true;
        }

        private void CbxPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Bring up list of tiers for selected period
            allTiers = new List<string>();
            ScrapeStats("https://www.smogon.com/stats/" + cbxPeriod.SelectedValue+"chaos/", ref allTiers);
            cbxTier.ItemsSource = allTiers;

            cbxTier.IsEnabled = true;
            btnGetStats.IsEnabled = true;
        }

        private void BtnGetStats_Click(object sender, RoutedEventArgs e)
        {
            //For these dictionaries, -1 is used to indicate no input
            Dictionary<string, int> lvlRanks = new Dictionary<string, int>
            {
                { "level", -1 },
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
            bool max = (bool)chxMax.IsChecked, maxPlus = (bool)chxMaxPlus.IsChecked, maxZero = (bool)chxMaxZero.IsChecked, maxMaxPlus = (bool)chxMaxMaxPlus.IsChecked;

            bool additionalEVs = Check4Bools(max, maxPlus, maxZero, maxMaxPlus);
            ////////////////////////
            //Validate input
            
            if (!ValidateNumbers(lvlRanks, usages, additionalEVs))
            {
                MessageBox.Show("Error with inputs. Please make sure level and ranks are integers greater than zero and that usage % is a value 0-100. Either a rank or usage value must be entered for each category, except for EVs if an additional spread is checked");
                return;
            }
            //Get stats
            string url = "https://www.smogon.com/stats/" + cbxPeriod.SelectedValue+"chaos/"+cbxTier.SelectedValue;
            WebClient web = new WebClient();
            string json = web.DownloadString(url);

            //Validate output
            //Apply filters
            //Probably want to store filtered output in external variable
        }

        //Validates the inputs from the text boxes in the main window    
        //Doesn't strictly need to be in its own function, but it does avoid unnecessarily checking all inputs even after input is found to be false. Could've just used goto for that, but I think this is just neater
        private bool ValidateNumbers(Dictionary<string, int> lvlRanks, Dictionary<string, float> usages, bool ignoreEVs)
        {
            if (!ValidatePositive(tbxLevel.Text, lvlRanks["level"])) return false;
            if (!ValidatePositive(tbxPokeRank.Text, lvlRanks["pokeRank"]) && !ValidatePercent(tbxPokeUsage.Text, usages["pokeUsage"])) return false;
            if (!ValidatePositive(tbxMovesRank.Text, lvlRanks["movesRank"]) && !ValidatePercent(tbxMovesUsage.Text, usages["movesUsage"])) return false;
            if (!ValidatePositive(tbxItemsRank.Text, lvlRanks["itemsRank"]) && !ValidatePercent(tbxItemsUsage.Text, usages["itemsUsage"])) return false;
            if (!ValidatePositive(tbxAbilitiesRank.Text, lvlRanks["abilitiesRank"]) && !ValidatePercent(tbxAbilitiesUsage.Text, usages["abilitiesUsage"])) return false;
            if (!ignoreEVs)
            {
                if (!ValidatePositive(tbxEVsRank.Text, lvlRanks["movesRank"]) && !ValidatePercent(tbxEVsUsage.Text, usages["evsUsage"])) return false;
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

        //Checks that an integer is >0, saves it to the target and returns whether or not it's valid
        private bool ValidatePositive(string input, int target)
        {
            if (int.TryParse(input, out target) && target > 0) return true;
            else return false;
        }

        //Checks that an float is >=0 and <=100, saves it to the target and returns whether or not it's valid
        private bool ValidatePercent(string input, float target)
        {
            if (float.TryParse(input, out target))
            {
                if (target >= 0 && target <= 100) return true;
                else return false;
            }
            else return false;
        }
    }
}
