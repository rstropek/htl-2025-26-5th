import { Component, inject, output, signal } from '@angular/core';
import { BraceletItem } from '../bracelet-data.service';
import { ColorService } from '../color.service';

const LETTERS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
const SYMBOLS = ['\u2665', '\u2605'];
const ALL_LETTERS = [...LETTERS.split(''), ...SYMBOLS];

/**
 * UI component for composing a friendship bracelet.
 *
 * A bracelet is built by alternating between letter cubes and spacer beads:
 * letter, spacer, letter, spacer, ... , letter.
 * It must start and end with a letter. Maximum 10 letters.
 *
 * This component is purely a UI composer — it does NOT call the backend API.
 * Validation and "Add to Order" logic live in the parent component (`OrderCreate`).
 *
 * The parent receives bracelet data reactively via the {@link braceletChanged} output,
 * which emits a `BraceletItem[]` after every mutation.
 */
@Component({
  selector: 'app-bracelet-builder',
  templateUrl: './bracelet-builder.html',
  styleUrl: './bracelet-builder.css',
})
export class BraceletBuilder {
  /** The current list of items on the bracelet, in order. */
  readonly items = signal<BraceletItem[]>([]);

  /** Emits the current bracelet items after every mutation. */
  readonly braceletChanged = output<BraceletItem[]>();

/** All available letters and symbols that can be placed on the bracelet. */
  readonly letters = ALL_LETTERS;

  /** All available spacer bead colors. */
  readonly colors = inject(ColorService).spacerColors;

  /** Adds a letter cube to the bracelet. */
  addLetter(letter: string): void {
    this.items.update(items => [...items, { type: 'letter', value: letter }]);
    this.emitData();
  }

  /** Adds a colored spacer bead to the bracelet. */
  addSpacer(color: { hex: string; name: string }): void {
    this.items.update(items => [...items, { type: 'spacer', value: color.name, hex: color.hex }]);
    this.emitData();
  }

  /** Removes the last item from the bracelet (undo). */
  undo(): void {
    this.items.update(items => items.slice(0, -1));
    this.emitData();
  }

  /** Clears the bracelet. */
  reset(): void {
    this.items.set([]);
    this.emitData();
  }

  private emitData(): void {
    this.braceletChanged.emit(this.items());
  }
}
