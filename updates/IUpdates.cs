using plugin;
using System;
using WUApiLib;
using Newtonsoft.Json.Linq;

namespace updates
{
    [PluginAttribute(PluginName = "Updates")]
    public class IUpdates : IInputPlugin
    {
        public string Execute(JObject set)
        {
            dynamic package = new JObject();
            package.automaticUpdates = null;
            package.numInstalledUpdates = null;
            package.installedUpdates = new JArray();
            dynamic update = new JObject();

            // Checks if automatic updates are enabled
            AutomaticUpdates automaticUpdates = new AutomaticUpdates();
            package.automaticUpdates = automaticUpdates.ServiceEnabled;


            // Checks installed updates
            UpdateSession uSession = new UpdateSession();

            // IUpdateSearcher class
            // https://docs.microsoft.com/en-us/windows/desktop/api/wuapi/nn-wuapi-iupdatesearcher
            IUpdateSearcher uSearcher = uSession.CreateUpdateSearcher();
            uSearcher.ServerSelection = ServerSelection.ssDefault;
            uSearcher.IncludePotentiallySupersededUpdates = true;
            uSearcher.Online = false;
            try
            {
                // Number of installed updates
                ISearchResult sResult = uSearcher.Search("IsInstalled=1 And IsHidden=0");

                package.numInstalledUpdates = sResult.Updates.Count;

                // IUpdate class
                // https://docs.microsoft.com/en-us/windows/desktop/api/wuapi/nn-wuapi-iupdate
                foreach (IUpdate iupdate in sResult.Updates)
                {
                    update.id = iupdate.Identity.UpdateID;
                    update.revision = iupdate.Identity.RevisionNumber;
                    update.dep_date = iupdate.LastDeploymentChangeTime.ToShortDateString();
                    update.title = iupdate.Title;

                    var categories = new JArray();
                    // IUpdate class
                    // https://docs.microsoft.com/en-us/windows/desktop/api/wuapi/nn-wuapi-icategory
                    foreach (ICategory icategory in iupdate.Categories)
                    {
                        categories.Add(icategory.Name);
                    }

                    update.categories = String.Join(", ", categories);

                    package.installedUpdates.Add(update);
                }




                // Updates history
                int count = uSearcher.GetTotalHistoryCount();
                Console.WriteLine("COOOOOOOOOOOOOOOOOUNT: " + count);
                IUpdateHistoryEntryCollection history = uSearcher.QueryHistory(0, count);
                for (int i = 0; i < count; ++i)
                {
                    Console.WriteLine(string.Format("ID: {0}\tRevision: {1}\tDate: {2}\tTitle: {3}", history[i].UpdateIdentity.UpdateID, history[i].UpdateIdentity.RevisionNumber, history[i].Date.ToShortDateString(), history[i].Title));
                }



                return package.ToString();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong: " + ex.Message);
            }

            return null;
        }

    }
}