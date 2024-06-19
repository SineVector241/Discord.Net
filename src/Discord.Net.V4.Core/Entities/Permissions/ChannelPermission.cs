namespace Discord;

public static class ChannelPermission
{
    public static readonly PermissionSet CreateInstantInvite = new(0);
    public static readonly PermissionSet ManageChannels = new(4);
    public static readonly PermissionSet AddReactions = new(6);
    public static readonly PermissionSet PrioritySpeaker = new(8);
    public static readonly PermissionSet Stream = new(9);
    public static readonly PermissionSet ViewChannel = new(10);
    public static readonly PermissionSet SendMessages = new(11);
    public static readonly PermissionSet SendTTSMessages = new(12);
    public static readonly PermissionSet ManageMessages = new(13);
    public static readonly PermissionSet EmbedLinks = new(14);
    public static readonly PermissionSet AttachFiles = new(15);
    public static readonly PermissionSet ReadMessageHistory = new(16);
    public static readonly PermissionSet MentionEveryone = new(17);
    public static readonly PermissionSet UseExternalEmojis = new(18);
    public static readonly PermissionSet Connect = new(20);
    public static readonly PermissionSet Speak = new(21);
    public static readonly PermissionSet MuteMembers = new(22);
    public static readonly PermissionSet DeafenMembers = new(23);
    public static readonly PermissionSet MoveMembers = new(24);
    public static readonly PermissionSet UseVad = new(25);
    public static readonly PermissionSet ManageRoles = new(28);
    public static readonly PermissionSet ManageWebhooks = new(29);
    public static readonly PermissionSet UseApplicationCommands = new(31);
    public static readonly PermissionSet RequestToSpeak = new(32);
    public static readonly PermissionSet ManageEvents = new(33);
    public static readonly PermissionSet ManageThreads = new(34);
    public static readonly PermissionSet CreatePublicThreads = new(35);
    public static readonly PermissionSet CreatePrivateThreads = new(36);
    public static readonly PermissionSet UseExternalStickers = new(37);
    public static readonly PermissionSet SendMessagesInThreads = new(38);
    public static readonly PermissionSet UseEmbeddedActivities = new(39);
    public static readonly PermissionSet UseSoundboard = new(42);
    public static readonly PermissionSet CreateEvents = new(44);
    public static readonly PermissionSet UseExternalSounds = new(45);
    public static readonly PermissionSet SendVoiceMessages = new(46);
    public static readonly PermissionSet SendPolls = new(49);
    public static readonly PermissionSet UseExternalApps = new(50);
}
