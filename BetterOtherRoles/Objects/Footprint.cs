using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BetterOtherRoles.EnoFw.Roles.Crewmate;
using BetterOtherRoles.EnoFw.Roles.Impostor;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using BetterOtherRoles.Utilities;
using UnityEngine;

namespace BetterOtherRoles.Objects 
{
    public class FootprintHolder : MonoBehaviour
    {
        static FootprintHolder() => ClassInjector.RegisterTypeInIl2Cpp<FootprintHolder>();

        public FootprintHolder(IntPtr ptr) : base(ptr) { }

        private static FootprintHolder _instance;
        public static FootprintHolder Instance
        {
            get => _instance ? _instance : _instance = new GameObject("FootprintHolder").AddComponent<FootprintHolder>();
            set => _instance = value;

        }
        
        private static Sprite _footprintSprite;
        private static Sprite FootprintSprite => _footprintSprite ??= Helpers.loadSpriteFromResources("BetterOtherRoles.Resources.Footprint.png", 600f);

        private static bool AnonymousFootprints => Detective.Instance.AnonymousFootprint;
        private static float FootprintDuration => Detective.Instance.FootprintDuration;
        
        private class Footprint
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public PlayerControl Owner;
            public GameData.PlayerInfo Data;
            public float Lifetime;

            public Footprint()
            {
                GameObject = new("Footprint") { layer = 8 };
                Transform = GameObject.transform;
                Renderer = GameObject.AddComponent<SpriteRenderer>();
                Renderer.sprite = FootprintSprite;
                Renderer.color = Color.clear;
                GameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            }
        }

       

        private readonly ConcurrentBag<Footprint> _pool = new();
        private readonly List<Footprint> _activeFootprints = new();
        private readonly List<Footprint> _toRemove = new();
        
        [HideFromIl2Cpp]
        public void MakeFootprint(PlayerControl player)
        {
            if (!_pool.TryTake(out var print))
            {
                print = new();
            }

            print.Lifetime = FootprintDuration;

            var p = player.transform.position;
            var pos = new Vector3(p.x, p.y, p.z + 0.005f);
            print.Transform.SetPositionAndRotation(pos, Quaternion.EulerRotation(0, 0, UnityEngine.Random.Range(0.0f, 360.0f)));
            print.GameObject.SetActive(true);
            print.Owner = player;
            print.Data = player.Data;
            _activeFootprints.Add(print);
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            _toRemove.Clear();
            foreach (var activeFootprint in _activeFootprints)
            {
                var p = activeFootprint.Lifetime / FootprintDuration;
                
                if (activeFootprint.Lifetime <= 0)
                {
                    _toRemove.Add(activeFootprint);
                    continue;
                }
                
                Color color;
                if (AnonymousFootprints || Camouflager.Instance.CamouflageTimer > 0)
                {
                    color = Palette.PlayerColors[6];
                }
                else if (activeFootprint.Owner == Morphling.Instance.Player && Morphling.Instance.MorphTimer > 0 && Morphling.Instance.MorphTarget && Morphling.Instance.MorphTarget.Data != null)
                {
                    color = Palette.PlayerColors[Morphling.Instance.MorphTarget.Data.DefaultOutfit.ColorId];
                }
                else
                {
                    color = Palette.PlayerColors[activeFootprint.Data.DefaultOutfit.ColorId];
                }

                color.a = Math.Clamp(p, 0f, 1f);
                activeFootprint.Renderer.color = color;

                activeFootprint.Lifetime -= dt;
            }
            
            foreach (var footprint in _toRemove)
            {
                footprint.GameObject.SetActive(false);
                _activeFootprints.Remove(footprint);
                _pool.Add(footprint);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}