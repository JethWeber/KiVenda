---
name: KiVenda System
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
  on-surface-variant: '#3e4a3d'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#6e7b6c'
  outline-variant: '#bdcaba'
  surface-tint: '#006e2d'
  primary: '#006b2c'
  on-primary: '#ffffff'
  primary-container: '#00873a'
  on-primary-container: '#f7fff2'
  inverse-primary: '#62df7d'
  secondary: '#4059aa'
  on-secondary: '#ffffff'
  secondary-container: '#8fa7fe'
  on-secondary-container: '#1d3989'
  tertiary: '#00628d'
  on-tertiary: '#ffffff'
  tertiary-container: '#007cb1'
  on-tertiary-container: '#fcfcff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#7ffc97'
  primary-fixed-dim: '#62df7d'
  on-primary-fixed: '#002109'
  on-primary-fixed-variant: '#005320'
  secondary-fixed: '#dce1ff'
  secondary-fixed-dim: '#b6c4ff'
  on-secondary-fixed: '#00164e'
  on-secondary-fixed-variant: '#264191'
  tertiary-fixed: '#c9e6ff'
  tertiary-fixed-dim: '#89ceff'
  on-tertiary-fixed: '#001e2f'
  on-tertiary-fixed-variant: '#004c6e'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.3'
  title-lg:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: '1.4'
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.5'
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: '1.4'
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '600'
    lineHeight: '1.4'
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  unit: 4px
  container-padding: 24px
  gutter: 16px
  sidebar-width: 260px
  card-gap: 20px
---

## Brand & Style

This design system is engineered for the Angolan commercial landscape, balancing high-performance utility with a premium, approachable aesthetic. The brand personality is rooted in **efficiency, reliability, and modernism**, designed to transform complex inventory and sales data into a clear, actionable interface.

The visual style is a hybrid of **Modern Corporate and Soft Minimalist**, drawing inspiration from Microsoft Fluent 2 and high-end FinTech platforms. It utilizes significant whitespace to reduce cognitive load for operators and business owners. Key characteristics include:
- **Clarity over Clutter:** Every element serves a functional purpose.
- **Precision:** Fine lines and intentional alignment reflect professional rigor.
- **Layered Depth:** Subtle use of semi-transparent materials and shadows to define hierarchy without visual noise.

## Colors

The palette is anchored by **Growth Green (#16A34A)**, symbolizing prosperity and "Go" actions in a commercial context. This is balanced by **Trust Blue (#1E3A8A)**, used for institutional elements and secondary navigation to provide a sense of stability.

The neutral scale favors cool grays to maintain a "clean" feel. Backgrounds use a tiered approach: 
- **Page Background:** A very light tint (#F8FAFC) to differentiate from pure white containers.
- **Surface Background:** Pure white (#FFFFFF) for cards and primary UI elements.
- **Interaction States:** Use 5% - 10% opacity overlays of the primary color for hover states rather than changing the base color drastically.

## Typography

The system uses **Inter** exclusively to ensure maximum legibility across various monitor resolutions. The hierarchy is strictly enforced:
- **Weight Usage:** Use `600` (Semi-bold) for section headers and `400` (Regular) for data and prose. `500` (Medium) is reserved for interactive elements like buttons and menu items.
- **Contrast:** Always use `text_primary` for headings and `text_secondary` for supportive text or descriptions.
- **Data Tables:** Numerical data should use a tabular figures feature (tnum) if available to ensure columns of numbers align vertically for easy scanning.

## Layout & Spacing

The design system follows a **Fixed-Fluid Hybrid** model. Since this is desktop software, it prioritizes a 12-column grid within the main content area while the sidebar remains fixed.

- **Sidebar:** Fixed at 260px. This houses the primary navigation and user profile.
- **Main Canvas:** Uses a fluid grid with a maximum content width of 1600px to prevent lines of text from becoming too long on ultra-wide monitors.
- **Rhythm:** All spacing is based on a 4px baseline. Components should generally use 8px, 16px, or 24px increments for padding and margins.
- **Negative Space:** Maintain a minimum of 24px padding around the main viewport edges to ensure the UI feels airy and premium.

## Elevation & Depth

This system uses a **Layered Surface** philosophy to communicate hierarchy.

1.  **Level 0 (Base):** The page background (#F8FAFC).
2.  **Level 1 (Cards/Sidebar):** Pure white surfaces with a 1px border (#E2E8F0) and a very soft, diffused shadow (`0 1px 3px rgba(0,0,0,0.05)`).
3.  **Level 2 (Modals/Popovers):** Higher elevation with a 12% opacity blur (Glassmorphism effect) and a more pronounced shadow (`0 10px 25px rgba(0,0,0,0.1)`).

**Glassmorphism:** Apply a `backdrop-filter: blur(12px)` to sidebars and top navigation bars to allow a hint of the background color to bleed through, creating a sense of material depth similar to Windows 11 "Mica" or "Acrylic" effects.

## Shapes

The design system utilizes a **Soft Geometric** shape language. 
- **Standard Radius:** 12px is the default for cards, input fields, and buttons.
- **Large Radius:** 24px or fully rounded for "Pill" tags/chips.
- **Consistency:** All interactive elements must share the same corner radius to maintain a cohesive look. Avoid sharp corners unless used for data-grid cells where density is required.

## Components

### Buttons
- **Primary:** High-impact green (#16A34A) with white text. 12px rounded corners. Use a subtle inner-glow on top to give a tactile feel.
- **Secondary:** Transparent background with a `secondary_color` border and text.
- **Ghost:** No border or background, used for low-priority actions in tables.

### Navigation Sidebar
- Fixed to the left. 
- Active state uses a 4px vertical "pill" indicator in Primary Green on the left edge of the menu item.
- Menu items should have a subtle hover background (`#F1F5F9`).

### Data Tables
- **Styling:** Clean rows with 1px light gray bottom borders. No vertical lines.
- **Hover:** Rows should highlight in a very faint blue or gray to help user focus.
- **Typography:** Labels in `label-sm` (uppercase) for headers.

### Cards
- Used for dashboard metrics and grouping related form fields.
- 12px border radius.
- Minimalist headers with Lucide icons on the top right.

### Input Fields
- Height of 40px or 44px for accessibility.
- 1px border (#CBD5E1) that transitions to Primary Green on focus.
- Subtle shadow when focused to provide a "lifted" appearance.

### Icons
- **System:** Lucide icons (2px stroke width).
- **Scale:** 20px for sidebar/general UI, 16px for inline table actions.