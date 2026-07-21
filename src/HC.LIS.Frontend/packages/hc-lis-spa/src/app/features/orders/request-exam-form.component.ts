import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { RequestExamParams } from '../../core/application/i-orders-port';
import { HcButton } from '../../ui/button/button';
import { HcField } from '../../ui/field/field';
import { HcInput } from '../../ui/input/input';
import { HcLabel } from '../../ui/input/label';

@Component({
  selector: 'app-request-exam-form',
  standalone: true,
  imports: [FormsModule, HcButton, HcField, HcInput, HcLabel],
  templateUrl: './request-exam-form.component.html',
  styleUrl: './request-exam-form.component.css',
})
export class RequestExamFormComponent {
  readonly examSubmitted = output<RequestExamParams>();
  /** Set by the parent while the exam request is in flight, to show the pending state. */
  readonly pending = input(false);

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
