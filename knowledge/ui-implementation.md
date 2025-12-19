# UI Implementation Guide

> **TRIGGER**: Use this documentation when implementing UI from mockups, converting designs to code, building frontend components, working with Figma/design files, or when asked about pixel-perfect implementation.

## Core Insight

Success with Claude for UI isn't about perfect prompts—it's about **context engineering, visual feedback loops, and understanding Claude's specific behaviors**.

Claude has unique characteristics:
- XML tag training (unlike GPT models)
- Superior vision capabilities
- Specific failure modes requiring tailored approaches

**Results**: 50-70% time savings, shipping apps in 16 hours that would take weeks manually.

## Prompt Engineering for UI

### Use XML Tags for Structure

Claude was specifically trained with XML, making it uniquely responsive to structured markup. This improves accuracy by ~20%.

**Essential XML structure for UI tasks**:

```xml
<mockup>
[Base64 image or file reference]
</mockup>

<specifications>
- Colors: #3B82F6 (primary), #10B981 (secondary), #F9FAFB (background)
- Typography: Inter font, 16px base, 1.5 line-height
- Spacing: 8px grid system (8, 16, 24, 32, 48, 64)
- Layout: Flexbox, mobile-first responsive
</specifications>

<requirements>
1. Extract ALL text exactly as shown in mockup
2. Match colors precisely using provided HEX codes
3. Implement responsive breakpoints at 640px, 768px, 1024px
4. No placeholders or abbreviated code
</requirements>

<tech_stack>
Next.js 14, TypeScript, Tailwind CSS 3.4, shadcn/ui components
</tech_stack>

<output_format>
Provide complete implementation with proper file structure:
- /components/ui/ for base components
- /components/features/ for feature modules
- /lib/ for utilities
</output_format>
```

**Benefits**:
- Prevents Claude from mixing instructions
- Improves accuracy by ~20%
- Creates parseability for extracting specific response sections

### Place Images Before Text

**Critical**: Always place images at the very start of prompts before any text. This optimizes Claude's vision processing and significantly improves accuracy.

For multiple images, label them clearly:
- "Image 1: Desktop mockup"
- "Image 2: Mobile view"
- "Image 3: Component states"

### Force Analysis Before Implementation

Use chain-of-thought with `<thinking>` tags:

```xml
<instructions>
Before generating code, analyze the mockup in <thinking> tags:
1. List all UI components visible in the design
2. Extract exact text content from the image
3. Identify the complete color palette with approximate values
4. Describe layout structure (grid, flexbox, positioning)
5. Note spacing patterns and measurements
6. Identify interactive elements and their states

Then generate the implementation in <code> tags.
</instructions>
```

**Impact**: Improves accuracy by 20%, reduces text omissions, creates audit trail you can verify before accepting code.

### Prefill Responses

Guide Claude's output by starting its response to control format and ensure consistency.

## Specification Requirements

### Always Provide Exact Values

Don't let Claude guess. Specify explicitly:

| Specification | Example |
|--------------|---------|
| **Colors** | `#3B82F6` (not "blue") |
| **Typography** | Inter, 16px base, 1.5 line-height |
| **Spacing** | 8px grid system (8, 16, 24, 32, 48, 64) |
| **Breakpoints** | 640px, 768px, 1024px, 1280px |
| **Border radius** | 4px, 8px, 12px |
| **Shadows** | `0 4px 6px -1px rgba(0,0,0,0.1)` |

### Tech Stack Declaration

Always specify your exact stack:

```xml
<tech_stack>
- Framework: Next.js 14 (App Router)
- Language: TypeScript 5.3
- Styling: Tailwind CSS 3.4
- Components: shadcn/ui
- Icons: Lucide React
- State: React hooks (no external state library)
</tech_stack>
```

### Output Format Requirements

Specify file structure expectations:

```xml
<output_format>
- One component per file
- Named exports only
- Props interface defined above component
- Include all imports
- No placeholder comments like "// rest of code here"
</output_format>
```

## Common Failure Modes & Solutions

### Text Omissions

**Problem**: Claude skips or abbreviates text from mockups.

**Solution**:
```xml
<requirements>
Extract and include ALL text exactly as shown:
- Headlines (word-for-word)
- Body copy (complete paragraphs)
- Button labels
- Navigation items
- Footer content
Do NOT abbreviate or paraphrase any text.
</requirements>
```

### Generic Styling

**Problem**: Claude uses default styles instead of matching mockup.

**Solution**: Provide explicit design tokens:
```xml
<design_tokens>
{
  "colors": {
    "primary": "#3B82F6",
    "primary-hover": "#2563EB",
    "secondary": "#10B981",
    "background": "#F9FAFB",
    "text": "#111827",
    "text-muted": "#6B7280"
  },
  "spacing": {
    "xs": "4px",
    "sm": "8px",
    "md": "16px",
    "lg": "24px",
    "xl": "32px"
  }
}
</design_tokens>
```

### Incomplete Code

**Problem**: Claude writes `// ... rest of component` or similar.

**Solution**:
```xml
<rules>
- Generate COMPLETE, RUNNABLE code
- No placeholders or abbreviated sections
- No "... rest of code here" comments
- Every function must be fully implemented
- All imports must be included
</rules>
```

### Wrong Component Library

**Problem**: Claude uses different component library than specified.

**Solution**: Be explicit and provide examples:
```xml
<component_library>
Use shadcn/ui components ONLY:
- Button from "@/components/ui/button"
- Card from "@/components/ui/card"
- Input from "@/components/ui/input"

DO NOT use:
- Material UI
- Chakra UI
- Ant Design
- Native HTML elements where shadcn component exists
</component_library>
```

## Visual Feedback Loop

### Screenshot-Driven Iteration

1. **Generate** initial implementation
2. **Screenshot** the rendered result
3. **Compare** side-by-side with mockup
4. **Provide** screenshot back to Claude with specific corrections:

```xml
<feedback>
[Screenshot of current implementation]

Compare to original mockup. Fix these issues:
1. Header padding is 24px, should be 16px
2. Button color is #3B82F6, should be #2563EB
3. Missing hover state on navigation items
4. Card shadow is too strong
</feedback>
```

### Diff-Based Corrections

For small fixes, specify exactly what to change:

```xml
<correction>
In the Header component:
- Line 15: Change `p-6` to `p-4`
- Line 23: Change `text-blue-500` to `text-blue-600`
- Line 31: Add `hover:bg-gray-100` to nav links
</correction>
```

## Component-First Approach

### Break Down Complex UIs

Instead of generating entire pages at once:

1. **Identify** atomic components first
2. **Generate** each component separately
3. **Compose** into larger structures
4. **Generate** page layout last

```xml
<approach>
Generate components in this order:
1. Button variants (primary, secondary, ghost)
2. Input fields (text, email, password)
3. Card component
4. Navigation header
5. Feature section
6. Full page composition
</approach>
```

### Component Specification Template

```xml
<component name="FeatureCard">
<description>
Card displaying a feature with icon, title, and description
</description>

<props>
- icon: LucideIcon (required)
- title: string (required)
- description: string (required)
- href?: string (optional link)
</props>

<visual_specs>
- Dimensions: 320px width, auto height
- Padding: 24px all sides
- Border: 1px solid #E5E7EB
- Border radius: 12px
- Shadow: 0 1px 3px rgba(0,0,0,0.1)
- Hover: shadow increases, slight scale (1.02)
</visual_specs>

<states>
- Default: as specified
- Hover: elevated shadow, scale 1.02
- Focus: blue outline (for keyboard nav)
</states>
</component>
```

## Responsive Implementation

### Mobile-First Approach

```xml
<responsive_strategy>
Implement mobile-first:
1. Base styles for mobile (< 640px)
2. sm: breakpoint (640px+)
3. md: breakpoint (768px+)
4. lg: breakpoint (1024px+)
5. xl: breakpoint (1280px+)

Stack all columns on mobile, expand to grid on desktop.
</responsive_strategy>
```

### Breakpoint Specifications

```xml
<breakpoints>
| Breakpoint | Width | Layout Changes |
|------------|-------|----------------|
| mobile | <640px | Single column, stacked nav |
| sm | 640px | 2-column grid |
| md | 768px | Sidebar appears |
| lg | 1024px | 3-column grid |
| xl | 1280px | Max-width container |
</breakpoints>
```

## Accessibility Requirements

### Include from the Start

```xml
<accessibility>
Required for all components:
- Semantic HTML elements (nav, main, article, section)
- ARIA labels for interactive elements
- Keyboard navigation support
- Focus visible states
- Color contrast minimum 4.5:1
- Alt text for images
- Skip links for navigation
</accessibility>
```

## Quality Checklist

Before accepting generated UI code:

### Visual Accuracy
- [ ] Colors match mockup exactly
- [ ] Typography matches (font, size, weight, line-height)
- [ ] Spacing matches design system
- [ ] All text content included (no omissions)
- [ ] Icons correct and properly sized

### Code Quality
- [ ] No placeholder comments
- [ ] All imports included
- [ ] TypeScript types defined
- [ ] Props interface complete
- [ ] No unused code

### Responsiveness
- [ ] Mobile layout works
- [ ] Tablet breakpoint handled
- [ ] Desktop layout correct
- [ ] No horizontal scroll on any viewport

### Interactivity
- [ ] Hover states implemented
- [ ] Focus states visible
- [ ] Click handlers connected
- [ ] Loading states if applicable
- [ ] Error states if applicable

### Accessibility
- [ ] Semantic HTML used
- [ ] ARIA labels present
- [ ] Keyboard navigable
- [ ] Color contrast sufficient

## Quick Reference Prompt Template

```xml
<mockup>
[Image placed FIRST]
</mockup>

<context>
Building: [Feature/component name]
Tech: [Next.js/React/Vue] + [TypeScript] + [Tailwind/CSS]
Components: [shadcn/ui/custom/etc]
</context>

<specifications>
Colors: [exact hex codes]
Typography: [font family, sizes]
Spacing: [grid system]
</specifications>

<requirements>
1. Match mockup pixel-perfectly
2. Extract ALL text verbatim
3. Complete, runnable code
4. No placeholders
5. Include all states (hover, focus, active)
</requirements>

<output>
Single file with:
- All imports
- TypeScript interfaces
- Complete component
- Export statement
</output>
```
