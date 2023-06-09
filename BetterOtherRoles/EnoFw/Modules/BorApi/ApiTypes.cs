﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using BetterOtherRoles.EnoFw.Libs;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Modules.BorApi;

public class MyAccount
{
    [JsonPropertyName("id")]
    public uint Id { get; set; }
    
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; }
    
    [JsonPropertyName("friendCode")]
    public string FriendCode { get; set; }
    
    [JsonPropertyName("isAdmin")]
    [JsonConverter(typeof(BoolConverter))]
    public bool IsAdmin { get; set; }
    
    [JsonPropertyName("xp")]
    public uint Xp { get; set; }
    
    [JsonPropertyName("lobbyNameColor")]
    public string LobbyNameColor { get; set; }
    
    [JsonPropertyName("lobbyOutlineColor")]
    public string LobbyOutlineColor { get; set; }
    
    [JsonPropertyName("lobbyTitle")]
    public string LobbyTitle { get; set; }
}

public class PublicAccountInfo
{
    [JsonPropertyName("friendCode")]
    public string FriendCode { get; set; }
    
    [JsonPropertyName("lobbyNameColor")]
    [JsonConverter(typeof(UnityColorConverter))]
    public Color LobbyNameColor { get; set; }
    
    [JsonPropertyName("lobbyOutlineColor")]
    [JsonConverter(typeof(UnityColorConverter))]
    public Color LobbyOutlineColor { get; set; }
    
    [JsonPropertyName("lobbyTitle")]
    public string LobbyTitle { get; set; }
    
    [JsonPropertyName("level")]
    public uint Level { get; set; }
    
    [JsonPropertyName("isAdmin")]
    public bool IsAdmin { get; set; }
}

public class ApiCustomOption
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("color")]
    [JsonConverter(typeof(UnityColorConverter))]
    public Color Color { get; set; }
    
    [JsonPropertyName("defaultValue")]
    public int DefaultValue { get; set; }
    
    [JsonPropertyName("children")]
    public List<ApiCustomOption> Children { get; set; }
}

public class CustomOptionValue
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
    
    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class MyPreset
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("ownerId")]
    public int OwnerId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("isPublic")]
    [JsonConverter(typeof(BoolConverter))]
    public bool IsPublic { get; set; }
}