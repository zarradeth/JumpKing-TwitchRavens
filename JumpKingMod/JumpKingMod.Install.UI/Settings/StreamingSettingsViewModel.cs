﻿using JumpKingMod.Install.UI.API;
using JumpKingRavensMod.Install.UI;
using JumpKingRavensMod.Settings;
using Logging.API;
using PBJKModBase.Streaming.Settings;
using Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install.UI.Settings
{
    /// <summary>
    /// An implementation of <see cref="IInstallerSettingsViewModel"/> to handle platform agnostic streaming settings
    /// </summary>
    public class StreamingSettingsViewModel : INotifyPropertyChanged, IInstallerSettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;
        private readonly ILogger logger;

        /// <summary>
        /// What streaming platform is selected for use with the Mod
        /// </summary>
        public AvailableStreamingPlatforms SelectedStreamingPlatform
        {
            get
            {
                return selectedStreamingPlatform;
            }
            set
            {
                if (selectedStreamingPlatform != value)
                {
                    selectedStreamingPlatform = value;
                    RaisePropertyChanged(nameof(SelectedStreamingPlatform));
                    RaisePropertyChanged(nameof(IsStreamingOnTwitch));
                    RaisePropertyChanged(nameof(IsStreamingOnYouTube));
                }
            }
        }
        private AvailableStreamingPlatforms selectedStreamingPlatform;


        /// <summary>
        /// Converts the <see cref="SelectedStreamingPlatform"/> property into a bool which can be bound to the UI or filtered against
        /// </summary>
        public bool IsStreamingOnTwitch
        {
            get
            {
                return selectedStreamingPlatform == AvailableStreamingPlatforms.Twitch;
            }
            set
            {
                SelectedStreamingPlatform = value ? AvailableStreamingPlatforms.Twitch : AvailableStreamingPlatforms.YouTube;
            }
        }

        /// <summary>
        /// Converts the <see cref="SelectedStreamingPlatform"/> property into a bool which can be bound to the UI or filtered against
        /// </summary>
        public bool IsStreamingOnYouTube
        {
            get
            {
                return selectedStreamingPlatform == AvailableStreamingPlatforms.YouTube;
            }
            set
            {
                SelectedStreamingPlatform = value ? AvailableStreamingPlatforms.YouTube : AvailableStreamingPlatforms.Twitch;
            }
        }

        /// <summary>
        /// The <see cref="UserSettings"/> object for the streaming Mod settings
        /// </summary>
        public UserSettings StreamingSettings
        {
            get
            {
                return streamingSettings;
            }
            private set
            {
                streamingSettings = value;
                RaisePropertyChanged(nameof(StreamingSettings));
                RaisePropertyChanged(nameof(AreStreamingSettingsLoaded));
                updateSettingsCommand.RaiseCanExecuteChanged();
                loadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings streamingSettings;

        /// <summary>
        /// Returns whether the streaming settings are currently populated
        /// </summary>
        public bool AreStreamingSettingsLoaded
        {
            get
            {
                return StreamingSettings != null;
            }
        }

        /// <summary>
        /// Constructor for creating a <see cref="StreamingSettingsViewModel"/>
        /// </summary>
        /// <param name="updateSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the updating of settings</param>
        /// <param name="loadSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the loading of settings</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to use for logging</param>
        /// <exception cref="ArgumentNullException"></exception>
        public StreamingSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand, ILogger logger)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool LoadSettings(string gameDirectory, string modFolder, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to load Streaming settings as provided Game Directory was empty!");
                return false;
            }

            string expectedSettingsFilePath = Path.Combine(gameDirectory, PBJKModBaseStreamingSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                if (StreamingSettings == null)
                {
                    StreamingSettings = new UserSettings(expectedSettingsFilePath, PBJKModBaseStreamingSettingsContext.GetDefaultSettings(), logger);
                }

                // Load the initial data
                SelectedStreamingPlatform = StreamingSettings.GetSettingOrDefault(PBJKModBaseStreamingSettingsContext.SelectedStreamingPlatformKey, AvailableStreamingPlatforms.Twitch);
            }
            else
            {
                logger.Error($"Failed to load Streaming settings as the settings file couldn't be found at '{expectedSettingsFilePath}'");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool SaveSettings(string gameDirectory, string modFolder)
        {
            if (StreamingSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to save streaming settings, either internal settings object ({StreamingSettings}) is null, or no game directory was provided ({gameDirectory})");
                return false;
            }

            StreamingSettings.SetOrCreateSetting(PBJKModBaseStreamingSettingsContext.SelectedStreamingPlatformKey, SelectedStreamingPlatform.ToString());
            return true;
        }

        /// <inheritdoc/>
        public bool AreSettingsLoaded()
        {
            return AreStreamingSettingsLoaded;
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
