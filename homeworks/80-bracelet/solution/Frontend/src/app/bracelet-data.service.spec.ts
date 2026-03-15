import { TestBed } from '@angular/core/testing';
import { BraceletDataService, BraceletItem } from './bracelet-data.service';

describe('BraceletDataService', () => {
  let service: BraceletDataService;

  beforeEach(() => {
    service = TestBed.inject(BraceletDataService);
  });

  describe('serialize', () => {
    it('should return an empty string for an empty array', () => {
      expect(service.serialize([])).toBe('');
    });

    it('should serialize a single letter', () => {
      const items: BraceletItem[] = [{ type: 'letter', value: 'A' }];
      expect(service.serialize(items)).toBe('A');
    });

    it('should serialize letters and spacers joined by pipes', () => {
      const items: BraceletItem[] = [
        { type: 'letter', value: 'H' },
        { type: 'spacer', value: 'pink', hex: '#f4a0a0' },
        { type: 'letter', value: 'I' },
      ];
      expect(service.serialize(items)).toBe('H|pink|I');
    });

    it('should serialize a longer bracelet', () => {
      const items: BraceletItem[] = [
        { type: 'letter', value: 'A' },
        { type: 'spacer', value: 'blue', hex: '#a0a0f4' },
        { type: 'letter', value: 'B' },
        { type: 'spacer', value: 'mint', hex: '#a0d4a0' },
        { type: 'letter', value: 'C' },
      ];
      expect(service.serialize(items)).toBe('A|blue|B|mint|C');
    });
  });

  describe('parse', () => {
    it('should return an empty array for an empty string', () => {
      expect(service.parse('')).toEqual([]);
    });

    it('should parse a single letter', () => {
      expect(service.parse('A')).toEqual([{ type: 'letter', value: 'A' }]);
    });

    it('should parse letters and spacers with correct hex values', () => {
      const result = service.parse('H|pink|I');
      expect(result).toEqual([
        { type: 'letter', value: 'H' },
        { type: 'spacer', value: 'pink', hex: '#f4a0a0' },
        { type: 'letter', value: 'I' },
      ]);
    });

    it('should use fallback hex for unknown color names', () => {
      const result = service.parse('A|unknown|B');
      expect(result[1]).toEqual({ type: 'spacer', value: 'unknown', hex: '#ccc' });
    });

    it('should parse a longer bracelet correctly', () => {
      const result = service.parse('X|blue|Y|lime|Z');
      expect(result).toEqual([
        { type: 'letter', value: 'X' },
        { type: 'spacer', value: 'blue', hex: '#a0a0f4' },
        { type: 'letter', value: 'Y' },
        { type: 'spacer', value: 'lime', hex: '#d4f4a0' },
        { type: 'letter', value: 'Z' },
      ]);
    });
  });

  describe('round-trip', () => {
    it('should produce the same string after parse then serialize', () => {
      const input = 'A|pink|B|cyan|C';
      const items = service.parse(input);
      expect(service.serialize(items)).toBe(input);
    });
  });
});
