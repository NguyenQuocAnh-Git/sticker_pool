using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>A yes/no question in front of anything destructive (restart, leave the level).</summary>
    public sealed class ConfirmPopupView : PopupView
    {
        [SerializeField] LocalizedLabel titleLabel;
        [SerializeField] LocalizedLabel bodyLabel;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;

        Action onConfirm;

        protected override void Awake()
        {
            base.Awake();
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Close);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
        }

        public void Ask(LocalizedString title, LocalizedString body, Action confirmed)
        {
            onConfirm = confirmed;
            titleLabel.SetKey(title);
            bodyLabel.SetKey(body);
            Open();
        }

        void Confirm()
        {
            var action = onConfirm;
            onConfirm = null;
            CloseImmediate();
            action?.Invoke();
        }
    }
}
