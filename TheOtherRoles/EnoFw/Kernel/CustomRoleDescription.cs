using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using TheOtherRoles.EnoFw.Utils;
using TheOtherRoles.Players;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Kernel;
public class CustomRoleDescription
{

    public readonly string Key;
    public readonly string Name;
    public Color Color;
    public string IntroDescription;
    public string ShortDescription;
    public RoleId RoleId;
    public bool IsNeutral;
    public bool IsModifier;
    public int SelectionIndex;
    public string Content;
    public OptionBehaviour OptionBehaviour;
    public readonly CustomRoleDescription Parent;
    public readonly bool IsHeader;
    
    public bool HasChildren => Tab.Role.Any(o => o.Parent != null && o.Parent.Key == Key);
    
    public List<CustomRoleDescription> Children => Tab.Role.Where(o => o.Parent != null && o.Parent.Key == Key).ToList();

    private CustomRoleDescription(
        RoleInfo roleInfo,
        string key,
        string content = "",
        bool isHeader = false,
        CustomRoleDescription parent = null)
    {
        Key = key;
        Name = roleInfo.name;
        Color = roleInfo.color;
        IntroDescription = roleInfo.introDescription;
        ShortDescription = roleInfo.shortDescription;
        RoleId = roleInfo.roleId;
        IsNeutral = roleInfo.isNeutral;
        IsModifier = roleInfo.isModifier;
        Content = content;



        // SelectionIndex = Mathf.Clamp(Entry.Value, 0, StringSelections.Count - 1);
    }

    public class Tab
    {
        public static readonly List<Tab> Tabs = new();
        public static List<CustomRoleDescription> Role => Tabs.SelectMany(tab => tab.RoleDescription)
            .ToList();
        
        public readonly string Key;
        public readonly string Title;
        public readonly string IconPath;
        public readonly List<CustomRoleDescription> RoleDescription = new();

        public Tab(string key, string title, string iconPath)
        {
            Key = key;
            Title = title;
            IconPath = iconPath;

            Tabs.Add(this);
        }
        
        public CustomRoleDescription CreateRoleDescription(
            RoleInfo roleInfo,
            string key,
            string name,
            string content,
            CustomRoleDescription parent = null)
        {
            var customOption = new CustomRoleDescription(
                roleInfo,
                key,
                content = "",
                parent == null,
                parent);
            
            return Add(customOption);
        }
        
        private CustomRoleDescription Add(CustomRoleDescription roleDescription)
        {
            RoleDescription.Add(roleDescription);
            return roleDescription;
        }
    }
}