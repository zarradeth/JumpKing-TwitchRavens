﻿using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Settings;
using Logging.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IYouDiedSubtextGetter"/> which gets the subtext from
    /// an expected file
    /// </summary>
    public class YouDiedSubtextFileGetter : IYouDiedSubtextGetter
    {
        private readonly List<string> subtexts;
        private readonly Random random;
        private readonly ILogger logger;

        private DateTime lastEditTime;

        /// <summary>
        /// Ctor for creating a <see cref="YouDiedSubtextFileGetter"/>
        /// </summary>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public YouDiedSubtextFileGetter(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            subtexts = new List<string>();
            random = new Random(DateTime.Now.Millisecond);

            // Parsing subtext list
            subtexts = ParseSubtextFile();
        }

        /// <summary>
        /// Parse the contents of the subtext file, returns default values if it fails
        /// </summary>
        private List<string> ParseSubtextFile()
        {
            List<string> subtexts = new List<string>();
            try
            {
                FileInfo fileInfo = new FileInfo(JumpKingModifiersModSettingsContext.FallDamageSubtextsFilePath);
                if (fileInfo.Exists)
                {
                    string[] fileContents = File.ReadAllLines(JumpKingModifiersModSettingsContext.FallDamageSubtextsFilePath);
                    for (int i = 0; i < fileContents.Length; i++)
                    {
                        string line = fileContents[i].Trim();
                        if (line.Length <= 0 || line[0] == JumpKingModifiersModSettingsContext.CommentCharacter)
                        {
                            continue;
                        }

                        subtexts.Add(line);
                    }
                    lastEditTime = fileInfo.LastWriteTimeUtc;
                    logger.Information($"Successfully loaded 'You Died' Subtexts File with '{subtexts.Count}' entries");
                }
                else
                {
                    logger.Warning($"Unable to find 'You Died' Subtexts List at '{JumpKingModifiersModSettingsContext.FallDamageSubtextsFilePath}', using default values instead!");
                    subtexts.AddRange(JumpKingModifiersModSettingsContext.GetDefaultFallDamageSubtexts());
                }
            }
            catch (Exception e)
            {
                logger.Error($"Encountered exception when loading 'You Died' Subtexts, using default values instead: {e.ToString()}");
                subtexts.AddRange(JumpKingModifiersModSettingsContext.GetDefaultFallDamageSubtexts());
            }
            return subtexts;
        }

        /// <summary>
        /// Returns whether the Subtext file has changed from the last edit time recorded
        /// </summary>
        private bool HasSubtextFileChanged()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(JumpKingModifiersModSettingsContext.FallDamageSubtextsFilePath);
                if (fileInfo.Exists)
                {
                    return fileInfo.LastAccessTimeUtc > lastEditTime;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to check if Subtext file has changed, using existing values!");
                return false;
            }

            return false;
        }

        /// <inheritdoc/>
        public string GetYouDiedSubtext()
        {
            if (subtexts == null || subtexts.Count <= 0)
            {
                return "I dont know what to put here, Have a good day!";
            }

            if (HasSubtextFileChanged())
            {
                logger.Information($"You Died Subtexts file has changed, reloading!");
                subtexts.Clear();
                subtexts.AddRange(ParseSubtextFile());
            }

            int index = random.Next(0, subtexts.Count);

            return subtexts[index];
        }
    }
}
