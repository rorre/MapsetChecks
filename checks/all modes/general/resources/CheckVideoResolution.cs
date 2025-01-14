﻿using MapsetParser.objects;
using MapsetParser.objects.events;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MapsetChecks.checks.general.resources
{
    public class CheckVideoResolution : GeneralCheck
    {
        public override CheckMetadata GetMetadata() => new CheckMetadata()
        {
            Category = "Resources",
            Message = "Too high video resolution.",
            Author = "Naxess",

            Documentation = new Dictionary<string, string>()
            {
                {
                    "Purpose",
                    @"
                    Keeping video files under a certain resolution threshold."
                },
                {
                    "Reasoning",
                    @"
                    Videos, having multiple frames stacked after each other, naturally take more file size than any regular background image. 
                    Because of this, the resolution threshold needs to be lower than for backgrounds in order to keep file size reasonable, 
                    even if it is possible to download without the video.
                    <note>
                        This is partly to ensure a reasonable load on the server, not only on the players' end, due to ranked content 
                        being downloaded more often.
                    </note>"
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                { "Resolution",
                    new IssueTemplate(Issue.Level.Problem,
                        "\"{0}\"",
                        "file name")
                    .WithCause(
                        "A video has a width exceeding 1280 pixels or a height exceeding 720 pixels.") },
                
                { "Leaves Folder",
                    new IssueTemplate(Issue.Level.Problem,
                        "\"{0}\" leaves the current song folder, which shouldn't ever happen.",
                        "file name")
                    .WithCause(
                        "The file path of a video starts with two dots.") },

                { "Missing",
                    new IssueTemplate(Issue.Level.Warning,
                        "\"{0}\" is missing, so unable to check that.",
                        "file name")
                    .WithCause(
                        "A video referenced is not present.") },

                { "Exception",
                    new IssueTemplate(Issue.Level.Error,
                        "\"{0}\" returned exception \"{1}\", so unable to check that.",
                        "file name", "exception")
                    .WithCause(
                        "An exception occurred trying to parse a video.") }
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
                    if (aTagFile.file.Properties.VideoWidth > 1280 ||
                        aTagFile.file.Properties.VideoHeight > 720)
                    {
                        issues.Add(new Issue(GetTemplate("Resolution"), null,
                            aTagFile.templateArgs[0]));
                    }

                    return issues;
                }))
            {
                // Returns issues from both non-faulty and faulty files.
                yield return issue;
            }
        }
    }
}
