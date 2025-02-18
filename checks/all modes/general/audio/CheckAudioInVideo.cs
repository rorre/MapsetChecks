﻿using MapsetParser.objects;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MapsetChecks.checks.general.audio
{
    public class CheckAudioInVideo : GeneralCheck
    {
        public override CheckMetadata GetMetadata() => new CheckMetadata()
        {
            Category = "Audio",
            Message = "Audio channels in video.",
            Author = "Naxess",

            Documentation = new Dictionary<string, string>()
            {
                {
                    "Purpose",
                    @"
                    Reducing the file size of videos."
                },
                {
                    "Reasoning",
                    @"
                    The audio track of videos will not play and usually take a similar amount of file size as any other audio file, 
                    so not removing the audio track means a noticeable amount of resources are wasted, even if the audio track is 
                    empty but still present."
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                { "Audio",
                    new IssueTemplate(Issue.Level.Problem,
                        "\"{0}\"",
                        "file name")
                    .WithCause(
                        "An audio track is present in one of the video files.") },
                
                { "Leaves Folder",
                    new IssueTemplate(Issue.Level.Problem,
                        "\"{0}\" leaves the current song folder, which shouldn't ever happen.",
                        "file name")
                    .WithCause(
                        "The file path of a video file starts with two dots.") },

                { "Missing",
                    new IssueTemplate(Issue.Level.Warning,
                        "\"{0}\" is missing, so unable to check that. Make sure you've downloaded with video.",
                        "file name")
                    .WithCause(
                        "A video file referenced is not present.") },

                { "Exception",
                    new IssueTemplate(Issue.Level.Error,
                        "\"{0}\" returned exception \"{1}\", so unable to check that.",
                        "file name", "exception")
                    .WithCause(
                        "An exception occurred trying to parse a video file.") }
            };
        }

        public override IEnumerable<Issue> GetIssues(BeatmapSet aBeatmapSet)
        {
            foreach (Issue issue in Common.GetTagOsuIssues(
                aBeatmapSet,
                aBeatmap => aBeatmap.videos.Count > 0 ? aBeatmap.videos.Select(aVideo => aVideo.path) : null,
                aTemplateArg => GetTemplate(aTemplateArg),
                aTagFile =>
                {
                    // Executes for each non-faulty video file used in one of the beatmaps in the set.
                    List<Issue> issues = new List<Issue>();
                    if (
                            aTagFile.file.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Video) &&
                            aTagFile.file.Properties.AudioChannels > 0)
                        issues.Add(new Issue(GetTemplate("Audio"), null,
                            aTagFile.templateArgs[0]));

                    return issues;
                }))
            {
                // Returns issues from both non-faulty and faulty files.
                yield return issue;
            }
        }
    }
}
