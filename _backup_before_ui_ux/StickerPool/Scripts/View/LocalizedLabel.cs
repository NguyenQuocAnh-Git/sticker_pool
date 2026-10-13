using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace StickerPool.View
{
    /// <summary>
    /// Text that follows a Localization string entry and re-renders on locale change.
    /// Set the entry in the Inspector for static labels, or call <see cref="SetKey"/> from code.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LocalizedLabel : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField] LocalizedString entry = new LocalizedString();

        LocalizedString current;
        object[] arguments;
        bool subscribed;

        public TMP_Text Text => text;

        void Reset() => text = GetComponent<TMP_Text>();

        void OnEnable()
        {
            if (current == null && !entry.IsEmpty) current = entry;
            Subscribe();
        }

        void OnDisable() => Unsubscribe();

        /// <summary>Binds to a copy of the given entry so shared assets are never mutated.</summary>
        public void SetKey(LocalizedString source, params object[] args)
        {
            Unsubscribe();
            current = source == null || source.IsEmpty
                ? null
                : new LocalizedString(source.TableReference, source.TableEntryReference);
            arguments = args;
            if (isActiveAndEnabled) Subscribe();
        }

        public void SetArguments(params object[] args)
        {
            arguments = args;
            if (current == null) return;
            current.Arguments = args;
            if (subscribed) current.RefreshString();
        }

        /// <summary>Shows plain text and stops following any entry.</summary>
        public void SetText(string value)
        {
            Unsubscribe();
            current = null;
            text.text = value;
        }

        void Subscribe()
        {
            if (current == null || subscribed) return;
            if (arguments != null && arguments.Length > 0) current.Arguments = arguments;
            current.StringChanged += OnStringChanged;
            subscribed = true;
        }

        void Unsubscribe()
        {
            if (!subscribed) return;
            current.StringChanged -= OnStringChanged;
            subscribed = false;
        }

        void OnStringChanged(string value) => text.text = value;
    }
}
