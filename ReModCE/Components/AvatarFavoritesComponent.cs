﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;

namespace ReModCE.Components
{
    internal class AvatarFavoritesComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList _avatarList;
        private ReUiButton _favoriteButton;

        private readonly AvatarList _allAvatars =
            new AvatarList();

        private readonly List<ReAvatar> _savedAvatars;

        public AvatarFavoritesComponent()
        {
            if (File.Exists("UserData/ReModCE/avatars.json"))
            {
                _savedAvatars = JsonConvert.DeserializeObject<List<ReAvatar>>(File.ReadAllText("UserData/ReModCE/avatars.json"));
            }
            else
            {
                _savedAvatars = new List<ReAvatar>();
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            _avatarList = new ReAvatarList("ReModCE Favorites", this);
            foreach (var avi in _savedAvatars.Distinct().Select(x => x.AsApiAvatar()).ToList())
            {
                _allAvatars.Add(avi);
            }

            _avatarList.AvatarPedestal.field_Internal_Action_3_String_GameObject_AvatarPerformanceStats_0 = new Action<string, GameObject, AvatarPerformanceStats>(OnAvatarInstantiated);

            _favoriteButton = new ReUiButton("Favorite", new Vector2(-600f, 375f), new Vector2(0.5f, 1f), () => FavoriteAvatar(_avatarList.AvatarPedestal.field_Internal_ApiAvatar_0),
                GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Favorite Button").transform.parent);

            if (uiManager.IsRemodLoaded)
            {
                _favoriteButton.Position += new Vector3(UiManager.ButtonSize, 0f);
            }
        }
        private void OnAvatarInstantiated(string url, GameObject avatar, AvatarPerformanceStats avatarPerformanceStats)
        {
            _favoriteButton.Text = HasAvatarFavorited(_avatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id) ? "Unfavorite" : "Favorite";
        }

        private void FavoriteAvatar(ApiAvatar apiAvatar)
        {
            var hasFavorited = HasAvatarFavorited(apiAvatar.id);
            if (!hasFavorited)
            {
                _allAvatars.Add(apiAvatar);
                _favoriteButton.Text = "Unfavorite";
                OnFavoriteAvatar(apiAvatar);
            }
            else
            {
                _allAvatars.Remove(apiAvatar);
                _favoriteButton.Text = "Favorite";
                OnUnfavoriteAvatar(apiAvatar);
            }

            _avatarList.Refresh(_allAvatars);
        }

        private bool HasAvatarFavorited(string id)
        {
            foreach (var avi in _allAvatars)
            {
                if (avi.id == _avatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id)
                {
                    return true;
                }
            }

            return false;
        }


        public void OnFavoriteAvatar(ApiAvatar avatar)
        {
            if (_savedAvatars.FirstOrDefault(a => a.Id == avatar.id) == null)
            {
                _savedAvatars.Add(new ReAvatar(avatar));
            }
            SaveAvatarsToDisk();
        }

        public void OnUnfavoriteAvatar(ApiAvatar avatar)
        {
            _savedAvatars.RemoveAll(a => a.Id == avatar.id);
            SaveAvatarsToDisk();
        }

        private void SaveAvatarsToDisk()
        {
            Directory.CreateDirectory("UserData/ReModCE");
            File.WriteAllText("UserData/ReModCE/avatars.json", JsonConvert.SerializeObject(_savedAvatars));
        }

        public AvatarList GetAvatars()
        {
            return _allAvatars;
        }
    }
}
