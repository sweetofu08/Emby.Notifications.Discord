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

namespace Emby.Notifications.Discord
{
    public class Notifier : IUserNotifier
    {
        private ILogger _logger;
        private IServerApplicationHost _appHost;
        private IHttpClient _httpClient;
        private IJsonSerializer _jsonSerializer;

        public Notifier(ILogger logger, IServerApplicationHost applicationHost, IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _appHost = applicationHost;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
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

            var discordMessage = new DiscordMessage
            {
                avatar_url = AvatarUrl,
                username = Username,
                embeds = new List<DiscordEmbed>()
                        {
                            new DiscordEmbed()
                            {
                                title = requestName,
                                description = request.Description,
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

            return DiscordWebhookHelper.ExecuteWebhook(discordMessage, Url, _jsonSerializer, _httpClient, cancellationToken);
        }
    }
}
