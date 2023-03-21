using System;
using System.Linq;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Drawing;
using System.IO;

namespace Emby.Notifications.Discord
{
    public class Plugin : BasePlugin, IHasWebPages, IHasThumbImage, IHasTranslations
    {
        public override string Name
        {
            get { return StaticName; }
        }

        public static string StaticName = "Discord";

        private const string EditorJsName = "discordnotificationeditorjs";

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = EditorJsName,
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.entryeditor.js"
                },
                new PluginPageInfo
                {
                    Name = "discordeditortemplate",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.entryeditor.template.html",
                    IsMainConfigPage = false
                }
            };
        }

        public string NotificationSetupModuleUrl => GetPluginPageUrl(EditorJsName);

        public TranslationInfo[] GetTranslations()
        {
            string basePath = GetType().Namespace + ".strings.";

            return GetType()
                .Assembly
                .GetManifestResourceNames()
                .Where(i => i.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                .Select(i => new TranslationInfo
                {
                    Locale = Path.GetFileNameWithoutExtension(i.Substring(basePath.Length)),
                    EmbeddedResourcePath = i

                }).ToArray();
        }

        public override string Description
        {
            get
            {
                return "Sends notifications to Discord via webhooks.";
            }
        }

        private Guid _id = new Guid("05C9ED79-5645-4AA2-A161-D4F29E63535C");
        public override Guid Id
        {
            get { return _id; }
        }

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.jpg");
        }

        public ImageFormat ThumbImageFormat
        {
            get
            {
                return ImageFormat.Jpg;
            }
        }
    }
}