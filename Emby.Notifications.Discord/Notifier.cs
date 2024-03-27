using System.Collections.Generic;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Model.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Notifications;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.IO;

namespace Emby.Notifications.Discord
{
    public class Notifier : IUserNotifier
    {
        private ILogger _logger;
        private IServerApplicationHost _appHost;
        private IHttpClient _httpClient;
        private IJsonSerializer _jsonSerializer;
        private IFileSystem _fileSystem;

        public Notifier(ILogger logger, IServerApplicationHost applicationHost, IHttpClient httpClient, IJsonSerializer jsonSerializer, IFileSystem fileSystem)
        {
            _logger = logger;
            _appHost = applicationHost;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _fileSystem = fileSystem;
        }

        private Plugin Plugin => _appHost.Plugins.OfType<Plugin>().First();

        public string Name => Plugin.StaticName;

        public string Key => "discordnotifications";

        public string SetupModuleUrl => Plugin.NotificationSetupModuleUrl;

        public Task SendNotification(InternalNotificationRequest request, CancellationToken cancellationToken)
        {
            var options = request.Configuration.Options;

            var serverName = request.Server.Name;

            string footerText = $"From {serverName}";
            string requestName = request.Title;

            options.TryGetValue("AvatarUrl", out string AvatarUrl);
            options.TryGetValue("Username", out string Username);
            options.TryGetValue("MentionType", out string MentionType);
            options.TryGetValue("Url", out string Url);

            var description = request.Description;

            // empty bodies are not allowed apparently
            if (string.IsNullOrEmpty(description))
            {
                description = request.Item?.Name ?? "Emby Server Notification";
            }

            var discordMessage = new DiscordMessage
            {
                avatar_url = AvatarUrl,
                username = Username,
                embeds = new List<DiscordEmbed>()
                        {
                            new DiscordEmbed()
                            {
                                title = requestName,
                                description = description,
                                footer = new Footer
                                {
                                    icon_url = AvatarUrl,
                                    text = footerText
                                },
                                timestamp = DateTime.Now
                            }
                        }
            };

            if (string.Equals(MentionType, "everyone", StringComparison.OrdinalIgnoreCase))
            {
                discordMessage.content = "@everyone";
            }
            else if (string.Equals(MentionType, "here", StringComparison.OrdinalIgnoreCase))
            {
                discordMessage.content = "@here";
            }

            // image example
            // first get an image

            // get a series or album image if available, otherwise, the image from the media
            var image = request.GetSeriesImageInfo(MediaBrowser.Model.Entities.ImageType.Primary)
                ?? request.GetSeriesImageInfo(MediaBrowser.Model.Entities.ImageType.Thumb)
                ?? request.GetImageInfo(MediaBrowser.Model.Entities.ImageType.Primary);

            string imageUrl = null;

            if (image != null)
            {
                // if the raw bytes are needed
                // var bytes = await _fileSystem.ReadAllBytesAsync(image.ImageInfo.Path).ConfigureAwait(false);

                // or if an image url is needed
                imageUrl = image.GetRemoteApiImageUrl(new ApiImageOptions
                {
                    Format = "jpg"
                });
            }

            if (!string.IsNullOrEmpty(imageUrl))
            {
                discordMessage.embeds[0].thumbnail = new Thumbnail
                {
                    url = imageUrl
                };
            }

            return DiscordWebhookHelper.ExecuteWebhook(discordMessage, Url, _jsonSerializer, _httpClient, cancellationToken);
        }
    }
}
