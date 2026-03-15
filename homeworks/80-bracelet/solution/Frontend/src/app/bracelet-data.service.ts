import { inject, Injectable } from '@angular/core';
import { ColorService } from './color.service';

/** Represents a single item on the bracelet — either a letter cube or a colored spacer bead. */
export interface BraceletItem {
  /** Whether this item is a `'letter'` cube or a `'spacer'` bead. */
  type: 'letter' | 'spacer';
  /** The letter character (e.g. `'A'`) for letters, or the color name (e.g. `'pink'`) for spacers. */
  value: string;
  /** Hex color code for spacer beads (e.g. `'#f4a0a0'`). Only set when `type` is `'spacer'`. */
  hex?: string;
}

@Injectable({ providedIn: 'root' })
export class BraceletDataService {
  private colorService = inject(ColorService);

  serialize(items: BraceletItem[]): string {
    return items.map(i => i.value).join('|');
  }

  parse(data: string): BraceletItem[] {
    if (!data) {
      return [];
    }
    return data.split('|').map((part, i) => {
      if (i % 2 === 0) {
        return { type: 'letter' as const, value: part };
      } else {
        return { type: 'spacer' as const, value: part, hex: this.colorService.colorNameToHex[part] || '#ccc' };
      }
    });
  }
}
