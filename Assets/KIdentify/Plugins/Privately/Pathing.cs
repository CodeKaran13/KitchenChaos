using System;
using System.IO;
using UnityEngine;

namespace Paths
{
    public class Pathing
    {
        public static string AppDataPath
        {
            get
            {
                return Application.dataPath; // gets app install path...
            }
        }

        // gets streaming assets raw path... 
        public static Uri AppContentDataUri
        {
            get
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {

                    var uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "file";
                    uriBuilder.Path = Path.Combine(AppDataPath, "Raw");
                    return uriBuilder.Uri;
                }
                else if (Application.platform == RuntimePlatform.Android)
                {

                    return new Uri("jar:file://" + Application.dataPath + "!/assets");
                }
                else
                {

                    var uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "file";
                    uriBuilder.Path = Path.Combine(AppDataPath, "StreamingAssets");
                    return uriBuilder.Uri;
                }
            }
        }

    }
}