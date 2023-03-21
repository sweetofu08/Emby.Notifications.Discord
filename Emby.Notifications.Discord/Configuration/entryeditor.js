define(['globalize', 'pluginManager', 'emby-input', 'emby-select'], function (globalize, pluginManager) {
    'use strict';

    function EntryEditor() {
    }

    EntryEditor.setObjectValues = function (context, entry) {

        entry.FriendlyName = context.querySelector('.txtFriendlyName').value;
        entry.Options.Url = context.querySelector('.txtDiscordWebhookUri').value;
        entry.Options.AvatarUrl = context.querySelector('.txtAvatarUrl').value;
        entry.Options.Username = context.querySelector('.txtUsername').value;
        entry.Options.MentionType = context.querySelector('.mentionType').value;
    };

    EntryEditor.setFormValues = function (context, entry) {

        context.querySelector('.txtFriendlyName').value = entry.FriendlyName || '';
        context.querySelector('.txtDiscordWebhookUri').value = entry.Options.Url || '';
        context.querySelector('.txtAvatarUrl').value = entry.Options.AvatarUrl || '';
        context.querySelector('.txtUsername').value = entry.Options.Username || '';
        context.querySelector('.mentionType').value = entry.Options.MentionType || '';
    };

    EntryEditor.loadTemplate = function (context) {

        return require(['text!' + pluginManager.getConfigurationResourceUrl('discordeditortemplate')]).then(function (responses) {

            var template = responses[0];
            context.innerHTML = globalize.translateDocument(template);

            // setup any required event handlers here
        });
    };

    EntryEditor.destroy = function () {

    };

    return EntryEditor;
});