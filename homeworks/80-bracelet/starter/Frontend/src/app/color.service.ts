import { Injectable } from '@angular/core';

/**
 * Provides the available spacer-bead colors for friendship bracelets.
 *
 * Inject this service wherever you need access to the palette of spacer colors
 * or need to convert a color name to its hex code.
 *
 * @example
 * ```ts
 * // Preferred injection style (standalone / signal-based components):
 * private colorService = inject(ColorService);
 *
 * // Alternative constructor injection:
 * constructor(private colorService: ColorService) {}
 * ```
 */
@Injectable({ providedIn: 'root' })
export class ColorService {
  /**
   * All available spacer-bead colors as an array of `{ hex, name }` objects.
   *
   * Use this to render a color palette in a template.
   *
   * @example
   * ```ts
   * // Component class
   * readonly colors = inject(ColorService).spacerColors;
   * ```
   *
   * ```html
   * <!-- Template: render a button for each color -->
   * @for (color of colors; track color.name) {
   *   <button [style.background-color]="color.hex"
   *           (click)="pickColor(color)">
   *     {{ color.name }}
   *   </button>
   * }
   * ```
   */
  readonly spacerColors: { hex: string; name: string }[] = [
    { hex: '#f4a0a0', name: 'pink' },
    { hex: '#f4d4a0', name: 'peach' },
    { hex: '#a0d4a0', name: 'mint' },
    { hex: '#a0a0f4', name: 'blue' },
    { hex: '#d4a0f4', name: 'purple' },
    { hex: '#f4a0d4', name: 'rose' },
    { hex: '#a0f4f4', name: 'cyan' },
    { hex: '#d4f4a0', name: 'lime' },
    { hex: '#f4c8a0', name: 'sand' },
  ];

  /**
   * Maps a color name (e.g. `'pink'`) to its hex code (e.g. `'#f4a0a0'`).
   *
   * Use this when you have a color name from serialized bracelet data
   * and need the corresponding hex value for rendering.
   *
   * @example
   * ```ts
   * const colorService = inject(ColorService);
   *
   * // Look up a single color:
   * const hex = colorService.colorNameToHex['mint'];  // '#a0d4a0'
   *
   * // With a fallback for unknown names:
   * const safeHex = colorService.colorNameToHex[name] ?? '#ccc';
   * ```
   */
  readonly colorNameToHex: Record<string, string> = Object.fromEntries(
    this.spacerColors.map(c => [c.name, c.hex])
  );
}
