using System;
using WUApiLib;
using Newtonsoft.Json.Linq;

namespace plugin
{
    [PluginAttribute(PluginName = "Updates")]
    public class IUpdates : IInputPlugin
    {
        public string Execute()
        {
            dynamic package = new JObject();
            package.automaticUpdates = null;
            package.numInstalledUpdates = null;
            package.installedUpdates = new JArray();
            package.numUpdatesAvailable = null;
            package.updatesAvailable = new JArray();
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
                    update.date = iupdate.LastDeploymentChangeTime;
                    update.title = iupdate.Title;
                    update.categories = new JArray();
                    // IUpdate class
                    // https://docs.microsoft.com/en-us/windows/desktop/api/wuapi/nn-wuapi-icategory
                    foreach (ICategory icategory in iupdate.Categories)
                    {
                        update.categories.Add(icategory.Name);
                    }

                    package.installedUpdates.Add(update);
                }


                // Number of non installed updates

                sResult = uSearcher.Search("IsInstalled=0 And IsHidden=0");

                package.numUpdatesAvailable = sResult.Updates.Count;

                foreach (IUpdate iupdate in sResult.Updates)
                {
                    update.id = iupdate.Identity.UpdateID;
                    update.date = iupdate.LastDeploymentChangeTime;
                    update.title = iupdate.Title;

                    package.updatesAvailable.Add(update);
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