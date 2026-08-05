import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PhysiciansService } from './physicians.service';
import type { PhysicianSearchResult } from '../../../core/domain/physician-search-result';
import { HcAlert } from '../../../ui/alert/alert';
import { HcButton } from '../../../ui/button/button';
import { HcCard } from '../../../ui/card/card';
import { HcField } from '../../../ui/field/field';
import { HcInput } from '../../../ui/input/input';
import { HcLabel } from '../../../ui/input/label';

@Component({
  selector: 'app-physician-form',
  standalone: true,
  imports: [FormsModule, HcAlert, HcButton, HcCard, HcField, HcInput, HcLabel],
  templateUrl: './physician-form.component.html',
  styleUrl: './physician-form.component.css',
})
export class PhysicianFormComponent implements OnInit {
  private readonly service = inject(PhysiciansService);

  /** The row being edited, or null to register a new physician. */
  readonly physician = input<PhysicianSearchResult | null>(null);

  readonly close = output<void>();

  protected fullName = '';
  protected licenceNumber = '';
  protected readonly error = signal<string | null>(null);
  protected readonly submitting = signal(false);

  /**
   * The host recreates this component every time the dialog opens, so the row to edit is
   * known before the first change-detection pass. Prefilling here rather than in an
   * effect is what puts the values in the inputs on that first pass instead of the next.
   */
  ngOnInit(): void {
    const editing = this.physician();
    this.fullName = editing?.fullName ?? '';
    this.licenceNumber = editing?.licenceNumber ?? '';
  }

  protected async onSubmit(): Promise<void> {
    const fullName = this.fullName.trim();
    if (!fullName) {
      return;
    }
    const licenceNumber = this.licenceNumber.trim() || undefined;
    const editing = this.physician();

    this.submitting.set(true);
    try {
      if (editing) {
        await this.service.update(editing.id, { fullName, licenceNumber });
      } else {
        await this.service.register({ fullName, licenceNumber });
      }
      this.error.set(null);
      this.close.emit();
    } catch (err) {
      this.error.set(
        err instanceof Error && err.message.trim()
          ? err.message
          : 'Failed to save the physician. Please check the fields and try again.',
      );
    } finally {
      this.submitting.set(false);
    }
  }
}
