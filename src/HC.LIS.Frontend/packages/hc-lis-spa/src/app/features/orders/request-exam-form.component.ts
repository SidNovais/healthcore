import { Component, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { RequestExamParams } from '../../core/application/i-orders-port';

@Component({
  selector: 'app-request-exam-form',
  standalone: true,
  imports: [FormsModule],
  template: `
    <form class="exam-form" (ngSubmit)="submit()">
      <div class="field">
        <label for="exam-mnemonic">Exam Mnemonic</label>
        <input
          id="exam-mnemonic"
          data-testid="exam-mnemonic-input"
          type="text"
          [(ngModel)]="examMnemonic"
          name="examMnemonic"
          required
          placeholder="e.g. GLU, HBA1C"
          class="mono-input"
        />
      </div>
      <div class="field">
        <label for="container-type">Container Type</label>
        <input
          id="container-type"
          data-testid="container-type-input"
          type="text"
          [(ngModel)]="containerType"
          name="containerType"
          required
          placeholder="e.g. RedTop, EDTA"
          class="mono-input"
        />
      </div>
      <button
        type="submit"
        data-testid="request-exam-btn"
        class="btn-primary"
        [disabled]="!examMnemonic || !containerType"
      >
        Add Exam
      </button>
    </form>
  `,
  styles: [`
    .exam-form { display: flex; flex-direction: column; gap: 1rem; }
    .field { display: flex; flex-direction: column; gap: 0.25rem; }
    label { font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.08em; color: var(--color-text-muted, #6b7280); }
    .mono-input {
      font-family: 'JetBrains Mono', 'IBM Plex Mono', monospace;
      padding: 0.5rem 0.75rem;
      border: 1px solid var(--color-border, #d1d5db);
      border-radius: 0;
      font-size: 0.9rem;
      background: var(--color-surface, #fff);
      width: 100%;
      box-sizing: border-box;
    }
    .mono-input:focus { outline: 2px solid #00897B; outline-offset: -1px; }
    .btn-primary {
      align-self: flex-start;
      padding: 0.5rem 1.25rem;
      background: #00897B;
      color: #fff;
      border: none;
      border-radius: 2px;
      font-size: 0.875rem;
      cursor: pointer;
      font-weight: 500;
    }
    .btn-primary:disabled { opacity: 0.45; cursor: not-allowed; }
  `],
})
export class RequestExamFormComponent {
  readonly examSubmitted = output<RequestExamParams>();

  protected examMnemonic = '';
  protected containerType = '';

  protected submit(): void {
    this.examSubmitted.emit({
      examMnemonic: this.examMnemonic,
      specimenMnemonic: 'BLOOD',
      materialType: 'WholeBlood',
      containerType: this.containerType,
      additive: 'EDTA',
      processingType: 'Standard',
      storageCondition: 'RoomTemp',
    });
    this.examMnemonic = '';
    this.containerType = '';
  }
}
