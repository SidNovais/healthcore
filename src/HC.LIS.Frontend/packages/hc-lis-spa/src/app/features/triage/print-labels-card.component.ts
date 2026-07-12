import { Component, input, computed, viewChildren, ElementRef, effect } from '@angular/core';
import JsBarcode from 'jsbarcode';
import type { SampleSummary } from '../../core/domain/sample-summary';
import { HcButton } from '../../ui/button/button';

@Component({
  selector: 'app-print-labels-card',
  standalone: true,
  imports: [HcButton],
  templateUrl: './print-labels-card.component.html',
  styleUrl: './print-labels-card.component.css',
})
export class PrintLabelsCardComponent {
  readonly samples = input<SampleSummary[]>([]);
  readonly showPrintButton = input<boolean>(true);

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
