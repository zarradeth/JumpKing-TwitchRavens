﻿using PBJKModBase.Twitch.Settings;
using Logging.API;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Net;

namespace PBJKModBase.Twitch
{
    /// <summary>
    /// A class whose purpose is to make a <see cref="TwitchClient"/>
    /// </summary>
    public class TwitchClientFactory
    {
        private readonly ILogger logger;
        private readonly UserSettings userSettings;

        private static TwitchClient twitchClient;
        private static object clientLock = new object();

        /// <summary>
        /// Constructor for creating a <see cref="TwitchClientFactory"/>
        /// </summary>
        /// <param name="userSettings">A <see cref="UserSettings"/> class to get settings info from</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation for logging</param>
        public TwitchClientFactory(UserSettings userSettings, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            twitchClient = null;
        }

        /// <summary>
        /// Gets a <see cref="TwitchClient"/> instance for interfacing with twitch chat
        /// </summary>
        public TwitchClient GetTwitchClient()
        {
            lock (clientLock)
            {
                if (twitchClient != null)
                {
                    return twitchClient;
                }
                else
                {
                    // Initialise the settings and attempt to load the OAuth Token
                    string oAuthToken = userSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.OAuthKey, string.Empty);
                    string twitchName = userSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);

                    // If the Oauth Token is bad, exit now
                    if (string.IsNullOrWhiteSpace(oAuthToken))
                    {
                        logger.Error($"No valid OAuth token found in the {PBJKModBaseTwitchSettingsContext.SettingsFileName} file!");
                        return null;
                    }
                    if (string.IsNullOrWhiteSpace(twitchName))
                    {
                        logger.Error($"No valid TwitchAccountName found in the {PBJKModBaseTwitchSettingsContext.SettingsFileName} file!");
                        return null;
                    }

                    oAuthToken = oAuthToken.Trim();
                    twitchName = twitchName.Trim();
                    logger.Information($"Setting up Twitch Chat Client for '{twitchName}'");

                    try
                    {
                        var credentials = new ConnectionCredentials(twitchName, oAuthToken);
                        var clientOptions = new ClientOptions();
                        WebSocketClient webSocketClient = new WebSocketClient(clientOptions);
                        twitchClient = new TwitchClient(webSocketClient);
                        twitchClient.Initialize(credentials, twitchName);

                        //twitchClient.OnLog += TwitchClient_OnLog;

                        twitchClient.Connect();
                        return twitchClient;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Encountered error when trying to create Twitch Client. Check your Oauth token is correct!");
                        return null;
                    }
                }
            }
        }

        //private void TwitchClient_OnLog(object sender, TwitchLib.Client.Events.OnLogArgs e)
        //{
        //    logger.Information($"LOG {e.BotUsername} | {e.Data}");
        //}
    }
}
