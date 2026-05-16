import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-barcode-form',
  standalone: true,
  imports: [FormsModule],
  template: `
    <form class="barcode-form" (ngSubmit)="submit()">
      <div class="form-row">
        <span class="tube-label mono">{{ tubeType() }}</span>
        <input
          data-testid="barcode-value-input"
          type="text"
          [(ngModel)]="barcodeValue"
          name="barcodeValue"
          required
          placeholder="Scan or type barcode"
          class="barcode-input mono"
        />
        <button
          type="submit"
          data-testid="create-barcode-btn"
          class="btn-create"
          [disabled]="!barcodeValue"
        >
          Create Barcode
        </button>
      </div>
    </form>
  `,
  styles: [`
    .barcode-form { padding: 0.5rem 0; }
    .form-row { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
    .tube-label { font-size: 0.8rem; font-weight: 600; color: #374151; min-width: 60px; }
    .barcode-input { font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace; padding: 0.3rem 0.5rem; border: 1px solid #d1d5db; border-radius: 0; font-size: 0.85rem; flex: 1; min-width: 160px; box-sizing: border-box; }
    .barcode-input:focus { outline: 2px solid #0284c7; outline-offset: -1px; }
    .btn-create { padding: 0.3rem 0.75rem; background: #0284c7; color: #fff; border: none; border-radius: 2px; font-size: 0.8rem; cursor: pointer; font-weight: 500; white-space: nowrap; }
    .btn-create:disabled { opacity: 0.45; cursor: not-allowed; }
    .mono { font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace; }
  `],
})
export class BarcodeFormComponent {
  readonly tubeType = input.required<string>();
  readonly barcodeCreated = output<string>();

  protected barcodeValue = '';

  protected submit(): void {
    if (!this.barcodeValue) return;
    this.barcodeCreated.emit(this.barcodeValue);
    this.barcodeValue = '';
  }
}
