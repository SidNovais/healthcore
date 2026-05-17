import { Component, input, computed, viewChildren, ElementRef, effect } from '@angular/core';
import JsBarcode from 'jsbarcode';
import type { SampleSummary } from '../../core/domain/sample-summary';

@Component({
  selector: 'app-print-labels-card',
  standalone: true,
  template: `
    <div data-testid="print-labels-section" class="print-labels-section">
      @for (sample of samplesWithBarcode(); track sample.id) {
        <div data-testid="barcode-label" class="label-item">
          <span class="tube-type">{{ sample.tubeType }}</span>
          <svg #barcodesvg class="barcode-svg"></svg>
          <span class="barcode-text mono">{{ sample.barcode }}</span>
        </div>
      }
      @if (samplesWithBarcode().length > 0) {
        <button class="btn-print" (click)="print()">Print Labels</button>
      }
    </div>
  `,
  styles: [`
    .print-labels-section { display: flex; flex-direction: column; gap: 1rem; padding: 0.5rem 0; }
    .label-item { display: flex; flex-direction: column; align-items: flex-start; gap: 0.25rem; padding: 0.5rem; border: 1px solid #e5e7eb; border-radius: 2px; background: #fff; }
    .tube-type { font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.06em; color: #374151; }
    .barcode-svg { max-width: 100%; }
    .barcode-text { color: #6b7280; font-size: 0.75rem; }
    .mono { font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace; }
    .btn-print { margin-top: 0.5rem; padding: 0.4rem 1rem; background: #0284c7; color: #fff; border: none; border-radius: 2px; font-size: 0.85rem; font-weight: 500; cursor: pointer; }
    .btn-print:hover { background: #0369a1; }

    @media print {
      .btn-print { display: none; }
    }
  `],
})
export class PrintLabelsCardComponent {
  readonly samples = input<SampleSummary[]>([]);

  protected readonly samplesWithBarcode = computed(() =>
    this.samples().filter(s => s.barcode !== null && s.barcode !== '')
  );

  private readonly barcodeSvgs = viewChildren<ElementRef<SVGElement>>('barcodesvg');

  constructor() {
    effect(() => {
      const svgElements = this.barcodeSvgs();
      const barcoded = this.samplesWithBarcode();

      if (svgElements.length === 0 || barcoded.length === 0) return;

      svgElements.forEach((ref, index) => {
        const sample = barcoded[index];
        if (sample?.barcode) {
          JsBarcode(ref.nativeElement, sample.barcode, {
            format: 'CODE128',
            displayValue: false,
            width: 2,
            height: 60,
            margin: 4,
          });
        }
      });
    });
  }

  protected print(): void {
    window.print();
  }
}
