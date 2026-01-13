# Prize Reveal Particle System Setup Guide

## Critical Configuration for UI Particles

### Main Module
- **Duration**: 1.0
- **Looping**: ☐ (unchecked)
- **Start Delay**: 0
- **Start Lifetime**: 0.5 to 1.0
- **Start Speed**: 100 to 200
- **Start Size**: 10 to 30 (Important for UI scale!)
- **Start Rotation**: 0 to 360
- **Start Color**: Gold/Yellow (try Color: R:1, G:0.9, B:0, A:1)
- **Gravity Modifier**: 50
- **Simulation Space**: **World** (CRITICAL!)
- **Simulation Speed**: 1
- **Scaling Mode**: Hierarchy
- **Play On Awake**: ☐ (MUST be unchecked!)
- **Max Particles**: 50

### Emission Module
- **☑ Enable**
- **Rate over Time**: 0
- **Bursts**: Add ONE burst:
  - Time: 0.00
  - Count: 20-30
  - Cycles: 1
  - Interval: 0.01

### Shape Module
- **☑ Enable**
- **Shape**: Circle
- **Radius**: 1 (Will be scaled by GameObject transform)
- **Radius Thickness**: 1 (emit from edge)
- **Arc**: 360
- **Mode**: Random

### Renderer Module (VERY IMPORTANT!)
- **Render Mode**: Billboard
- **Material**: Default-Particle (or Sprites-Default)
- **Sorting Layer**: Default
- **Order in Layer**: 10 (Must be higher than UI elements behind it!)
- **Min Particle Size**: 0
- **Max Particle Size**: 1000

### Color over Lifetime (Recommended)
- **☑ Enable**
- **Color**: Create gradient
  - Start: Your color with Alpha 255
  - End: Same color with Alpha 0

### Size over Lifetime (Recommended)
- **☑ Enable**
- **Size**: Curve from 1.0 to 0.3

---

## Transform Settings for PrizeRevealParticles GameObject

In the Transform component:
- **Position**: (0, 0, -10) - negative Z brings it forward
- **Rotation**: (0, 0, 0)
- **Scale**: (1, 1, 1)

---

## Common Issues & Solutions

### Issue: Particles spawn but aren't visible
**Solution**: Check Order in Layer in Renderer - increase to 10 or higher

### Issue: Particles are too small
**Solution**: Increase Start Size to 20-50

### Issue: Particles appear behind the card
**Solution**: 
- Set Position Z to -10 (more negative = more forward)
- Increase Order in Layer

### Issue: Particles don't play at all
**Solution**: 
- Ensure Play On Awake is UNCHECKED
- Check console for "Playing prize reveal particles!" message
- If message appears but no particles, it's a rendering issue

### Issue: Particles are in wrong position
**Solution**: Set Simulation Space to "World"

---

## Quick Test

1. Select PrizeRevealParticles in hierarchy
2. In the Particle System component, click the "Particle Effect" button
3. Particles should burst outward in a circle
4. If you see them in Scene view but not Game view:
   - Problem is rendering/sorting layer
   - Increase Order in Layer
