﻿using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MALClient.UWP.Shared.Managers
{
    /// <summary>
    /// Disabled Permament workaround
    /// </summary>
    //public static class StoreLogoWorkaroundHacker
    //{
    //    const string xml = @"<tile>
    //                          <visual>
    //                            <binding template=""TileMedium"">
    //                              <image src='ms-appx:///Assets/MalClientTransparentLogo300x300.png'/>
    //                            </binding>                                
    //                            <binding template=""TileSmall"">
    //                              <image src='ms-appx:///Assets/MalClientTransparentLogo150x150.png'/>
    //                            </binding>
    //                          </visual>
    //                        </tile>";

    //    public static void Hack()
    //    {
    //        try
    //        {
    //            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
    //            XmlDocument document = new XmlDocument();
    //            document.LoadXml(xml);
    //            tileUpdater.Update(new TileNotification(document));
    //        }
    //        catch (Exception e)
    //        {
    //            //hack failed
    //        }

    //    }
    //}
}
