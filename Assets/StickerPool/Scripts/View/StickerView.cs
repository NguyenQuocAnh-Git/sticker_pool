using System;
using StickerPool.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StickerPool.View
{
    /// <summary>
    /// One sticker in the jar. The prefab root sits on the hitbox centre so physics rotates the
    /// sticker around it; the artwork is a child that is offset by hand in each prefab variant.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class StickerView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Rigidbody2D body;
        [SerializeField] SpriteRenderer visual;
        [SerializeField] SpriteRenderer shadow;
        [SerializeField] int jarSortingOrder = 10;
        [SerializeField] int flightSortingOrder = 150;

        public event Action<StickerView> Tapped;

        public StickerData Data { get; private set; }
        public int StickerId { get; private set; }
        public Rigidbody2D Body => body;

        float visualBaseAlpha = 1f;
        float shadowBaseAlpha = 1f;

        void Reset() => body = GetComponent<Rigidbody2D>();

        void Awake()
        {
            if (visual != null) visualBaseAlpha = visual.color.a;
            if (shadow != null) shadowBaseAlpha = shadow.color.a;
        }

        public void Place(StickerData data, int stickerId, Vector2 position, float angleRadians)
        {
            Data = data;
            StickerId = stickerId;

            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angleRadians * Mathf.Rad2Deg));
            body.position = position;
            body.rotation = angleRadians * Mathf.Rad2Deg;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = true;

            SetLayer(jarSortingOrder);
            SetAlpha(1f);
        }

        /// <summary>Hands the transform over to tweens: no physics, drawn above everything in the jar.</summary>
        public void EnterFlight()
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            SetLayer(flightSortingOrder);
        }

        public void Poke(Vector2 velocity)
        {
            body.linearVelocity = velocity;
        }

        public void SetAlpha(float alpha)
        {
            SetSpriteAlpha(visual, visualBaseAlpha, alpha);
            SetSpriteAlpha(shadow, shadowBaseAlpha, alpha);
        }

        public void Deactivate()
        {
            body.simulated = false;
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData) => Tapped?.Invoke(this);

        static void SetSpriteAlpha(SpriteRenderer sprite, float baseAlpha, float alpha)
        {
            if (sprite == null) return;
            var c = sprite.color;
            c.a = baseAlpha * alpha;
            sprite.color = c;
        }

        void SetLayer(int order)
        {
            if (shadow != null) shadow.sortingOrder = order - 1;
            visual.sortingOrder = order;
        }
    }
}
