﻿using MapsetParser.objects;
using MapsetParser.statics;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MapsetChecks.checks.general.files
{
    public class CheckZeroBytes : GeneralCheck
    {
        public override CheckMetadata GetMetadata() => new CheckMetadata()
        {
            Category = "Files",
            Message = "0-byte files.",
            Author = "Naxess",

            Documentation = new Dictionary<string, string>()
            {
                {
                    "Purpose",
                    @"
                    Ensuring all files can be uploaded properly during the submission process."
                },
                {
                    "Reasoning",
                    @"
                    0-byte files prevent other files in the song folder from properly uploading. Mappers sometimes attempt to silence 
                    certain hit sound files by completely removing its audio data, but this often results in a file completely devoid 
                    of any data which makes it 0-byte. Instead, use <a href=""https://up.ppy.sh/files/blank.wav"">this 44-byte file</a> 
                    which osu provides to mute hit sound files.
                    <image>
                        https://i.imgur.com/Qb9z95T.png
                        A 0-byte slidertick hit sound file.
                    </image>"
                }
            }
        };
        
        public override Dictionary<string, IssueTemplate> GetTemplates()
        {
            return new Dictionary<string, IssueTemplate>()
            {
                { "0-byte",
                    new IssueTemplate(Issue.Level.Problem,
                        "\"{0}\"",
                        "path")
                    .WithCause(
                        "A file in the song folder contains no data; consists of 0 bytes.") },

                { "Exception",
                    new IssueTemplate(Issue.Level.Error,
                        "\"{0}\" threw \"{1}\"",
                        "path", "exception")
                    .WithCause(
                        "A file which was attempted to be checked could not be opened.") }
            };
        }

        public override IEnumerable<Issue> GetIssues(BeatmapSet aBeatmapSet)
        {
            foreach (string filePath in aBeatmapSet.songFilePaths)
            {
                Issue errorIssue = null;
                FileInfo file = null;
                try
                {
                    file = new FileInfo(filePath);
                }
                catch (Exception exception)
                {
                    errorIssue = new Issue(GetTemplate("Exception"), null,
                        PathStatic.RelativePath(filePath, aBeatmapSet.songPath),
                        exception);
                }

                if (errorIssue != null)
                {
                    yield return errorIssue;
                    continue;
                }

                if (file.Length == 0)
                    yield return new Issue(GetTemplate("0-byte"), null,
                        PathStatic.RelativePath(filePath, aBeatmapSet.songPath));
            }
        }
    }
}
