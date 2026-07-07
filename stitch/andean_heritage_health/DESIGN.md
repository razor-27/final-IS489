---
name: Andean Heritage Health
colors:
  surface: '#f8f9ff'
  surface-dim: '#cbdbf5'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff4ff'
  surface-container: '#e5eeff'
  surface-container-high: '#dce9ff'
  surface-container-highest: '#d3e4fe'
  on-surface: '#0b1c30'
  on-surface-variant: '#3c4946'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#6c7a76'
  outline-variant: '#bbcac5'
  surface-tint: '#006b5f'
  primary: '#006b5f'
  on-primary: '#ffffff'
  primary-container: '#00a896'
  on-primary-container: '#00352e'
  inverse-primary: '#59dbc7'
  secondary: '#4f5e7e'
  on-secondary: '#ffffff'
  secondary-container: '#cadaff'
  on-secondary-container: '#505f7f'
  tertiary: '#5f5e59'
  on-tertiary: '#ffffff'
  tertiary-container: '#97958f'
  on-tertiary-container: '#2e2e2a'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#79f7e3'
  primary-fixed-dim: '#59dbc7'
  on-primary-fixed: '#00201c'
  on-primary-fixed-variant: '#005047'
  secondary-fixed: '#d7e2ff'
  secondary-fixed-dim: '#b7c7eb'
  on-secondary-fixed: '#091b37'
  on-secondary-fixed-variant: '#374765'
  tertiary-fixed: '#e5e2db'
  tertiary-fixed-dim: '#c9c6c0'
  on-tertiary-fixed: '#1c1c18'
  on-tertiary-fixed-variant: '#474742'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  headline-xl:
    fontFamily: Inter
    fontSize: 40px
    fontWeight: '700'
    lineHeight: 48px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
    letterSpacing: 0.05em
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 4px
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 40px
  container-max: 1200px
  gutter: 24px
---

## Brand & Style
The design system for this medical appointment platform bridges modern clinical efficiency with subtle cultural resonance. The brand personality is **Professional, Trustworthy, and Locally Attuned**, designed to evoke a sense of calm reliability for patients in the Wari region.

The design style follows a **Corporate / Modern** aesthetic with a "Warm Minimalist" twist. It utilizes generous whitespace to reduce cognitive load—essential for healthcare contexts—while incorporating structural motifs inspired by Wari textiles (rhythmic grids and balanced proportions). The UI avoids cold, clinical sterility by using softened edges and warm neutral backgrounds, ensuring the technology feels like a supportive companion rather than a rigid institutional barrier.

## Colors
The palette is anchored in **Deep Navy (#1B2B48)** to establish institutional authority and stability. **Teal/Turquoise (#00A896)** serves as the primary action color, symbolizing vitality and health. 

We utilize a **Warm Neutral (#F4F1EA)** for secondary surfaces to provide a subtle "earthy" touch that softens the digital experience, contrasting with a crisp **Off-white (#F9FAFB)** for primary content areas. Semantic colors for Success, Warning, and Error follow standard healthcare patterns but are calibrated for high legibility against the light background.

## Typography
This design system uses **Inter** exclusively for its exceptional legibility in data-heavy environments. The typographic scale is highly hierarchical: 
- **Headlines:** Use a tighter letter-spacing and heavier weights to convey confidence.
- **Body:** Uses standard weights with increased line height to ensure medical instructions and appointment details are easily readable for all age groups.
- **Labels:** Specifically designed for metadata (dates, times, department names) using medium weights and slightly increased tracking for clarity at small sizes.

## Layout & Spacing
The layout employs a **Fluid Grid** system with a max-width container for desktop viewing. 
- **Desktop:** 12-column grid, 24px gutters, and 40px side margins.
- **Tablet:** 8-column grid, 16px gutters, and 24px side margins.
- **Mobile:** 4-column grid, 16px gutters, and 16px side margins.

A strict 4px spacing scale (base-4) ensures visual rhythm. Large "Safe Areas" (40px+) are used between major sections to prevent the UI from feeling cluttered, which is vital for patient accessibility and stress reduction.

## Elevation & Depth
Depth is conveyed through **Tonal Layers** and **Ambient Shadows**. We avoid heavy, black shadows in favor of soft, color-tinted blurs.
- **Level 0 (Base):** Off-white surface.
- **Level 1 (Cards):** White background with a 1px border in a pale neutral (#E2E8F0) and a very soft 4px blur shadow.
- **Level 2 (Dropdowns/Modals):** White background with a 12px blur shadow, tinted with the Secondary Navy at 5% opacity to maintain color harmony.
- **Interactive Depth:** Buttons use a subtle "press" effect (reducing elevation) rather than significant color shifts to maintain a professional feel.

## Shapes
The shape language is defined by **Rounded (8px)** corners. This radius provides a friendly, approachable feel while remaining structured enough for a medical professional context.
- **Standard (Base):** 8px for buttons, inputs, and small widgets.
- **Large (Containers):** 16px (rounded-lg) for main content cards and appointment modules.
- **Extra Large (Hero):** 24px (rounded-xl) for featured banners or medical category selections.
- **Interactive Elements:** Checkboxes use a 4px radius to distinguish them from round radio buttons.

## Components
- **Buttons:** Primary buttons are solid Teal (#00A896) with white text. Secondary buttons use a Navy (#1B2B48) outline. All buttons have a height of 48px for touch-accessibility.
- **Input Fields:** Use a subtle neutral-100 fill with a bottom-only border or a light 1px stroke. Focus states use a 2px Teal glow.
- **Cards:** Appointment cards must feature a "status stripe" on the left edge (using semantic colors) to provide immediate visual cues.
- **Chips:** Used for medical specialties (e.g., "Cardiology"). These use a light Teal background with dark Teal text, avoiding heavy borders.
- **Lists:** High-contrast list items with 16px vertical padding, separated by subtle hairlines (#F1F5F9).
- **Navigation:** A clean top bar with Navy text and a Primary Teal indicator for active states. Use high-quality iconography with a 2px stroke weight for consistency.