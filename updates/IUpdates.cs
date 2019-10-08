using System;
using WUApiLib;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

using plugin;

namespace updates
{
    [PluginAttribute(PluginName = "Updates")]
    public class IUpdates : IInputPlugin
    {
        public event EventHandler<MessageEventArgs> MessageEvent;

        public string Execute(JObject set)
        {
            // Checks if automatic updates are enabled
            AutomaticUpdates automaticUpdates = new AutomaticUpdates();
            var automaticUpdatesEnabled = automaticUpdates.ServiceEnabled;
            
            // Checks updates available
            UpdateSession uSession = new UpdateSession();

            // IUpdateSearcher class
            // https://docs.microsoft.com/en-us/windows/desktop/api/wuapi/nn-wuapi-iupdatesearcher
            IUpdateSearcher uSearcher = uSession.CreateUpdateSearcher();
            uSearcher.ServerSelection = ServerSelection.ssDefault;
            uSearcher.IncludePotentiallySupersededUpdates = true;
            uSearcher.Online = false;
            try
            {
                // Number of available updates
                ISearchResult sResult = uSearcher.Search("IsInstalled=0 And IsHidden=0");

                var numUpdatesAvailable = sResult.Updates.Count;

                var updatesAvailable = new List<Models.Update>();
                // IUpdate class
                // https://docs.microsoft.com/en-us/windows/desktop/api/wuapi/nn-wuapi-iupdate
                foreach (IUpdate iupdate in sResult.Updates)
                {
                    var categories = new JArray();
                    // IUpdate class
                    // https://docs.microsoft.com/en-us/windows/desktop/api/wuapi/nn-wuapi-icategory
                    foreach (ICategory icategory in iupdate.Categories)
                    {
                        categories.Add(icategory.Name);
                    }

                    // Create Update
                    var update = new Models.Update
                    {
                        ID = iupdate.Identity.UpdateID,
                        DatePublished = iupdate.LastDeploymentChangeTime.ToShortDateString(),
                        Title = iupdate.Title,
                        Categories = String.Join(", ", categories)
                    };

                    // Add update to the list
                    updatesAvailable.Add(update);
                }

                // Updates history
                int count = uSearcher.GetTotalHistoryCount();
                var history = uSearcher.QueryHistory(0, count).Cast<IUpdateHistoryEntry>();

                var lastInstalled = history.Where(u => (u.HResult == 0) && 
                                                 (!u.Title.Contains("Definition Update for Microsoft Endpoint Protection")) &&
                                                 (!u.Title.Contains("Definition Update for Windows Defender Antivirus")) &&
                                                 (!u.Title.Contains("Update for Windows Defender Antivirus antimalware platform")) &&
                                                 (!u.Title.Contains("Security Intelligence Update for Windows Defender Antivirus")))
                                           .First().Date;

                // Create the Machine info object to be returned
                var machineInfo = new Models.MachineInfo
                {
                    HostName = System.Environment.MachineName,
                    AutomaticUpdates = automaticUpdatesEnabled,
                    LastTimeUpdated = lastInstalled,
                    NumUpdatesAvailable = numUpdatesAvailable,
                    UpdatesAvailable = updatesAvailable
                };

                return JsonConvert.SerializeObject(machineInfo);
            }
            catch (InvalidOperationException ioe)
            {
                throw new Exceptions.EmptyHistoryException("Could not find any successful installation in the update history", ioe);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}