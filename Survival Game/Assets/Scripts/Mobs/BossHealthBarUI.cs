using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BossHealthBarUI : MonoBehaviour
{
    private static BossHealthBarUI instance;

    public static BossHealthBarUI Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject go = new GameObject("BossHealthBarUI");
                instance = go.AddComponent<BossHealthBarUI>();
            }
            return instance;
        }
    }

    public static void Hide(EnemyHealth boss)
    {
        if (instance != null)
        {
            instance.HideBoss(boss);
        }
    }

    [Header("Layout")]
    public float barWidth = 900f;
    public float barHeight = 16f;
    public float topMargin = 40f;
    public float spacing = 40f;
    public int maxBars = 4;

    [Header("Colours")]
    public Color borderColour = new Color(0.72f, 0.6f, 0.3f, 1f);
    public Color backgroundColour = new Color(0.05f, 0.04f, 0.04f, 0.9f);
    public Color ghostColour = new Color(0.9f, 0.75f, 0.35f, 1f);

    [Header("Biome Fill Colours")]
    public Color plainsColour = new Color(0.65f, 0.08f, 0.08f, 1f);
    public Color iceColour = new Color(0.25f, 0.6f, 0.9f, 1f);
    public Color desertColour = new Color(0.9f, 0.5f, 0.1f, 1f);
    public Color swampColour = new Color(0.3f, 0.6f, 0.2f, 1f);

    [Header("Behaviour")]
    public float ghostDelay = 0.6f;       // seconds before the ghost bar starts draining
    public float ghostDrainSpeed = 0.5f;  // fraction of the bar per second
    public float damageResetTime = 2f;    // damage number resets after this long without hits
    public float fadeSpeed = 3f;
    public float slideSpeed = 10f;
    public float deathLingerTime = 1.5f;  // how long the empty bar stays after the boss dies

    private class BossBar
    {
        public EnemyHealth boss;
        public RectTransform root;
        public CanvasGroup group;
        public RectTransform fillRect;
        public RectTransform ghostRect;
        public Text nameText;
        public Text damageText;
        public float lastHealth;
        public float ghostFraction;
        public float lastDamageTime;
        public float accumulatedDamage;
        public bool leaving;
        public float leaveTime;
    }

    private List<BossBar> bars = new List<BossBar>();
    private Font font;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // API

    public void Show(EnemyHealth newBoss, string displayName)
    {
        if (newBoss == null) return;

        // Already has a bar: just cancel any fade-out
        BossBar existing = FindBar(newBoss);
        if (existing != null)
        {
            existing.leaving = false;
            return;
        }

        if (bars.Count >= maxBars) return;

        BossBar bar = Build(displayName, GetBiomeColour(newBoss.biomeType));
        bar.boss = newBoss;
        bar.lastHealth = newBoss.CurrentHealth;
        bar.ghostFraction = SafeFraction(newBoss.CurrentHealth, newBoss.MaxHealth);
        SetFill(bar.fillRect, bar.ghostFraction);
        SetFill(bar.ghostRect, bar.ghostFraction);

        // Start in its own slot so it doesn't slide in from the top
        bar.root.anchoredPosition = new Vector2(0f, SlotY(bars.Count));
        bars.Add(bar);
    }
    
    private void HideBoss(EnemyHealth b)
    {
        BossBar bar = FindBar(b);
        if (bar != null && !bar.leaving)
        {
            bar.leaving = true;
            bar.leaveTime = Time.time;
        }

    }

    private BossBar FindBar(EnemyHealth b)
    {
        foreach (BossBar bar in bars)
        {
            if (bar.boss == b) return bar;
        }
        return null;
    }

    private Color GetBiomeColour(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Plains => plainsColour,
            BiomeType.Ice => iceColour,
            BiomeType.Desert => desertColour,
            BiomeType.Swamp => swampColour,
            _ => Color.white,
        };
    }

    void Update()
    {
        // Loop backwards so removing a bar doesn't skip the next one
        for (int i = bars.Count - 1; i >= 0; i--)
        {
            BossBar bar = bars[i];
            UpdateBar(bar);

            bool fadingOut = bar.leaving && Time.time >= bar.leaveTime;
            float targetAlpha = fadingOut ? 0f : 1f;
            bar.group.alpha = Mathf.MoveTowards(bar.group.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            if (fadingOut && bar.group.alpha <= 0f)
            {
                Destroy(bar.root.gameObject);
                bars.RemoveAt(i);
                continue;
            }

            // Slide into this bar's slot (bars below move up when one is removed)
            Vector2 pos = bar.root.anchoredPosition;
            pos.y = Mathf.Lerp(pos.y, SlotY(i), slideSpeed * Time.deltaTime);
            bar.root.anchoredPosition = pos;
        }
    }

    private void UpdateBar(BossBar bar)
    {
        // Boss was killed: show it empty for a moment, then fade.
        if (bar.boss == null)
        {
            if (!bar.leaving)
            {
                bar.leaving = true;
                bar.leaveTime = Time.time + deathLingerTime;
                SetFill(bar.fillRect, 0f);
            }
            // Drain the ghost down to empty
            bar.ghostFraction = Mathf.MoveTowards(bar.ghostFraction, 0f, ghostDrainSpeed * Time.deltaTime);
            SetFill(bar.ghostRect, bar.ghostFraction);
            return;
        }

        float current = bar.boss.CurrentHealth;
        float fraction = SafeFraction(current, bar.boss.MaxHealth);

        if (current < bar.lastHealth)
        {
            bar.accumulatedDamage += bar.lastHealth - current;
            bar.lastDamageTime = Time.time;
            bar.damageText.text = Mathf.CeilToInt(bar.accumulatedDamage).ToString();
        }
        bar.lastHealth = current;

        SetFill(bar.fillRect, fraction);

        if (fraction > bar.ghostFraction)
        {
            bar.ghostFraction = fraction; // healed
        }
        else if (Time.time - bar.lastDamageTime > ghostDelay)
        {
            bar.ghostFraction = Mathf.MoveTowards(bar.ghostFraction, fraction, ghostDrainSpeed * Time.deltaTime);
        }
        SetFill(bar.ghostRect, bar.ghostFraction);

        if (bar.accumulatedDamage > 0f && Time.time - bar.lastDamageTime > damageResetTime)
        {
            bar.accumulatedDamage = 0f;
            bar.damageText.text = "";
        }
    }

    private float SlotY(int index)
    {
        return -topMargin - (barHeight + spacing) * index;
    }

    // Construction of UI

    private BossBar Build(string displayName, Color fillColour)
    {
        BossBar bar = new BossBar();

        // Root (top centre)
        RectTransform root = NewRect("BossBar", transform);
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.sizeDelta = new Vector2(barWidth, 60f);
        bar.root = root;

        bar.group = root.gameObject.AddComponent<CanvasGroup>();
        bar.group.alpha = 0f;
        bar.group.blocksRaycasts = false;
        bar.group.interactable = false;

        // Name (top left)
        bar.nameText = NewText("Name", root, font, 26, TextAnchor.LowerLeft);
        bar.nameText.text = displayName;
        SetRow(bar.nameText.rectTransform);

        // Damage number (top right)
        bar.damageText = NewText("Damage", root, font, 24, TextAnchor.LowerRight);
        bar.damageText.color = new Color(1f, 0.9f, 0.6f, 1f);
        SetRow(bar.damageText.rectTransform);

        // Border
        Image border = NewImage("Border", root, borderColour);
        RectTransform br = border.rectTransform;
        br.anchorMin = new Vector2(0f, 0f);
        br.anchorMax = new Vector2(1f, 0f);
        br.pivot = new Vector2(0.5f, 0f);
        br.sizeDelta = new Vector2(0f, barHeight + 4f);
        br.anchoredPosition = Vector2.zero;

        // Background (inset to leave a thin border)
        Image bg = NewImage("Background", br, backgroundColour);
        Stretch(bg.rectTransform, 2f);

        // Ghost (lagging) bar, then the real fill on top of it
        Image ghost = NewImage("Ghost", bg.rectTransform, ghostColour);
        bar.ghostRect = ghost.rectTransform;
        Stretch(bar.ghostRect, 0f);

        Image fill = NewImage("Fill", bg.rectTransform, fillColour);
        bar.fillRect = fill.rectTransform;
        Stretch(bar.fillRect, 0f);

        return bar;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static Image NewImage(string name, Transform parent, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static Text NewText(string name, Transform parent, Font font, int size, TextAnchor anchor)
    {
        RectTransform rt = NewRect(name, parent);
        Text txt = rt.gameObject.AddComponent<Text>();
        txt.font = font;
        txt.fontSize = size;
        txt.alignment = anchor;
        txt.color = new Color(0.95f, 0.92f , 0.85f, 1f);
        txt.raycastTarget = false;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        rt.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);
        return txt;
    }

    private static void Stretch(RectTransform rt, float inset)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    private static void SetFill(RectTransform rt, float fraction)
    {
        rt.anchorMax = new Vector2(Mathf.Clamp01(fraction), rt.anchorMax.y);
    }

    private static float SafeFraction(float current, float max)
    {
        return (max > 0f) ? Mathf.Clamp01(current / max) : 0f;
    }

    private void SetRow(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, 36f);
        rt.anchoredPosition = Vector2.zero;
    }


}