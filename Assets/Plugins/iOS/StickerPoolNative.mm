// Native helpers for Sticker Pool: Taptic Engine feedback and the "reduce motion" accessibility flag.
#import <UIKit/UIKit.h>

static UIImpactFeedbackGenerator *Impact(UIImpactFeedbackStyle style)
{
    static NSMutableDictionary<NSNumber *, UIImpactFeedbackGenerator *> *cache;
    if (cache == nil) cache = [NSMutableDictionary new];
    UIImpactFeedbackGenerator *generator = cache[@(style)];
    if (generator == nil)
    {
        generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
        cache[@(style)] = generator;
    }
    return generator;
}

// Keep in sync with StickerPool.View.HapticKind.
extern "C" void StickerPoolHaptic(int kind)
{
    if (@available(iOS 10.0, *))
    {
        switch (kind)
        {
            case 0: [Impact(UIImpactFeedbackStyleLight) impactOccurred]; break;
            case 1: [Impact(UIImpactFeedbackStyleMedium) impactOccurred]; break;
            case 2:
                if (@available(iOS 13.0, *)) [Impact(UIImpactFeedbackStyleSoft) impactOccurred];
                else [Impact(UIImpactFeedbackStyleLight) impactOccurred];
                break;
            case 3:
                if (@available(iOS 13.0, *)) [Impact(UIImpactFeedbackStyleRigid) impactOccurred];
                else [Impact(UIImpactFeedbackStyleHeavy) impactOccurred];
                break;
            case 4:
            {
                UISelectionFeedbackGenerator *selection = [UISelectionFeedbackGenerator new];
                [selection selectionChanged];
                break;
            }
            case 5:
            {
                UINotificationFeedbackGenerator *notification = [UINotificationFeedbackGenerator new];
                [notification notificationOccurred:UINotificationFeedbackTypeSuccess];
                break;
            }
            case 6:
            {
                UINotificationFeedbackGenerator *notification = [UINotificationFeedbackGenerator new];
                [notification notificationOccurred:UINotificationFeedbackTypeWarning];
                break;
            }
        }
    }
}

extern "C" int StickerPoolReduceMotion()
{
    return UIAccessibilityIsReduceMotionEnabled() ? 1 : 0;
}
